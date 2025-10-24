using System.Runtime.InteropServices;
using System.Text;
using HK.Net.Core;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Sdks;
public class Recorder : IDisposable
{
    private Int32 UserId { get; set; }= -1;
    private Int32 IsDownloading { get; set; }= -1;
    private CHCNetSDK.NET_DVR_USER_LOGIN_INFO LoginInfo { get; set; }
    private CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo { get; set; }
    private CHCNetSDK.NET_DVR_PICCFG_V40 ChannelImageInfo { get; set; }
    private CHCNetSDK.NET_DVR_IPPARACFG_V40 IpConfigInfo { get; set; }
    private CHCNetSDK.NET_DVR_GET_STREAM_UNION StreamConfigInfo { get; set; }
    private uint DigitalChannelTotalNumber { get; set; } = 0;
    private string Ip { get; set; } //设备IP地址或者域名
    private ushort Port { get; set; }//设备服务端口号
    private string UserName { get; set; }//设备登录用户名
    private string Password { get; set; }//设备登录密码

    public Recorder(string ip, ushort port = 8000, string username = "admin", string password = "11111111a")
    {
        Ip = ip;
        Port = port;
        UserName = username;
        Password = password;
    }

    public void Login()
    {
        if (!CHCNetSDK.NET_DVR_Init())
            throw new Exception(Error());
        if (UserId < 0)
        {
            byte[] bytesUserName = Encoding.Default.GetBytes(UserName);
            byte[] bytesPassword = Encoding.Default.GetBytes(Password);
            byte[] bytesDeviceAddress = Encoding.Default.GetBytes(Ip);

            byte[] inputUserName = new byte[CHCNetSDK.NET_DVR_LOGIN_USERNAME_MAX_LEN];
            byte[] inputPassword = new byte[CHCNetSDK.NET_DVR_LOGIN_PASSWD_MAX_LEN];
            byte[] inputDeviceAddress = new byte[CHCNetSDK.NET_DVR_DEV_ADDRESS_MAX_LEN];

            Array.Copy(bytesUserName, inputUserName, bytesUserName.Length);
            Array.Copy(bytesPassword, inputPassword, bytesPassword.Length);
            Array.Copy(bytesDeviceAddress, inputDeviceAddress, bytesDeviceAddress.Length);
            var loginInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO()
            {
                sDeviceAddress = inputDeviceAddress,
                wPort = Port,
                sUserName = inputUserName,
                sPassword = inputPassword,
                //登录回调函数
                cbLoginResult = (int lUserID, int dwResult, IntPtr lpDeviceInfo, IntPtr pUser) =>
                {
                
                }
            };
            
        
            var deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
            UserId = CHCNetSDK.NET_DVR_Login_V40(ref loginInfo, ref deviceInfo);
            LoginInfo = loginInfo;
            DeviceInfo = deviceInfo;
            
            DigitalChannelTotalNumber = DeviceInfo.struDeviceV30.byIPChanNum + 256 * (uint)DeviceInfo.struDeviceV30.byHighDChanNum;
            if (DigitalChannelTotalNumber > 0)
                GetIpChannels();
        }
    }
    

    
    private void GetIpChannels()
    {
        uint dwSize = (uint)Marshal.SizeOf(IpConfigInfo);
        IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
        Marshal.StructureToPtr(IpConfigInfo, ptrIpParaCfgV40, false);

        uint dwReturn = 0;
        int iGroupNo = 0; //该Demo仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取
        if (!CHCNetSDK.NET_DVR_GetDVRConfig(UserId, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            throw new Exception(Error());
        else
        {
            // succ
            IpConfigInfo = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));
            
            uint actualDigitalTotalNumber = 64;
            if (DigitalChannelTotalNumber < 64)
                actualDigitalTotalNumber = DigitalChannelTotalNumber; //如果设备IP通道小于64路，按实际路数获取
            

            for (int i = 0; i < actualDigitalTotalNumber; i++)
            {
              
                var byStreamType = IpConfigInfo.struStreamMode[i].byGetStreamType;
                StreamConfigInfo = IpConfigInfo.struStreamMode[i].uGetStream;
                var associateDeviceInfo = IpConfigInfo.struIPDevInfo[i];

                string associateDeviceIp = System.Text.Encoding.GetEncoding("GBK").GetString(associateDeviceInfo.struIP.sIpV4);
                string result = "";
                if (byStreamType != 0)
                    continue;
                //目前NVR仅支持0- 直接从设备取流一种方式
                dwSize = (uint)Marshal.SizeOf(StreamConfigInfo);
                IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                Marshal.StructureToPtr(StreamConfigInfo, ptrChanInfo, false);
                var ipChannelInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));
                
                if (ipChannelInfo.byEnable == 0)
                    continue;
                dwReturn = 0;
                Int32 nSize = Marshal.SizeOf(ChannelImageInfo);
                IntPtr ptrPicCfg = Marshal.AllocHGlobal(nSize);
                Marshal.StructureToPtr(ChannelImageInfo, ptrPicCfg, false);
                if (!CHCNetSDK.NET_DVR_GetDVRConfig(UserId, CHCNetSDK.NET_DVR_GET_PICCFG_V40, i + (int)IpConfigInfo.dwStartDChan, ptrPicCfg, (UInt32)nSize, ref dwReturn))
                    throw new Exception(Error());
                else
                {
                    ChannelImageInfo = (CHCNetSDK.NET_DVR_PICCFG_V40)Marshal.PtrToStructure(ptrPicCfg, typeof(CHCNetSDK.NET_DVR_PICCFG_V40));
                    result = System.Text.Encoding.GetEncoding("GBK").GetString(ChannelImageInfo.sChanName);
                }
                Console.WriteLine($"通道{i + 1}, IP{associateDeviceIp}, 名字{result}");
                Marshal.FreeHGlobal(ptrPicCfg);
                
                
                Marshal.FreeHGlobal(ptrChanInfo);
            
            }
        }
        Marshal.FreeHGlobal(ptrIpParaCfgV40);
    }
    
 
    public void StartDownload(int channel, DateTime start, DateTime end, string rootPath)
    {
        if (IsDownloading >= 0)
        {
            throw new Exception(Error());//正在下载，请先停止下载
            return;
        }

        CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
        struDownPara.dwChannel = (uint)channel; //通道号 Channel number  

        //设置下载的开始时间 Set the starting time
        struDownPara.struStartTime.dwYear = start.Year;
        struDownPara.struStartTime.dwMonth = start.Month;
        struDownPara.struStartTime.dwDay = start.Day;
        struDownPara.struStartTime.dwHour = start.Hour;
        struDownPara.struStartTime.dwMinute = start.Minute;
        struDownPara.struStartTime.dwSecond = start.Second;

        //设置下载的结束时间 Set the stopping time
        struDownPara.struStopTime.dwYear = end.Year;
        struDownPara.struStopTime.dwMonth = end.Month;
        struDownPara.struStopTime.dwDay = end.Day;
        struDownPara.struStopTime.dwHour = end.Hour;
        struDownPara.struStopTime.dwMinute = end.Minute;
        struDownPara.struStopTime.dwSecond = end.Second;

        string sVideoFileName;  //录像文件保存路径和文件名 the path and file name to save      
        sVideoFileName = rootPath + struDownPara.dwChannel+".mp4";

        //按时间下载 Download by time
        IsDownloading = CHCNetSDK.NET_DVR_GetFileByTime_V40(UserId, sVideoFileName, ref struDownPara);
        if (IsDownloading < 0)
            throw new Exception(Error());
            

        uint iOutValue = 0;
        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(IsDownloading, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            throw new Exception(Error());
    }
    public void StopDownload()
    {
        if(IsDownloading<0)
            return;            
        if (!CHCNetSDK.NET_DVR_StopGetFile(IsDownloading))
            throw new Exception(Error());
        IsDownloading = -1;
    }
    
    
    public string Error()
    {
        int code = (int) CHCNetSDK.NET_DVR_GetLastError();
        return $"错误码={code}, 错误描述={ErrorCode.GetDescription(code)}";
    }

    public void Dispose()
    {
        // 停止下载
        if (IsDownloading >= 0)
        {
            CHCNetSDK.NET_DVR_StopGetFile(IsDownloading);
            IsDownloading = -1;
        }
        // 注销登录
        if (UserId >= 0)
        {
            CHCNetSDK.NET_DVR_Logout(UserId);
            UserId = -1;
        }
        // 清理SDK
        CHCNetSDK.NET_DVR_Cleanup();
    }

    ~Recorder()
    {
        Dispose();
    }
}