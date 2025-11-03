using System.Runtime.InteropServices;
using System.Text;
using HK.Net.Core;

namespace RailwayAlarmBackend.Sdks;
public class Recorder : IDisposable
{
    private Int32 UserId { get; set; }= -1;
    private Int32 IsDownloading { get; set; }= -1;

    private Int32 m_lPlayHandle { get; set; } = -1;
    private CHCNetSDK.NET_DVR_USER_LOGIN_INFO LoginInfo { get; set; }
    private CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo { get; set; }
    private CHCNetSDK.NET_DVR_PICCFG_V40 ChannelImageInfo { get; set; }
    private CHCNetSDK.NET_DVR_IPPARACFG_V40 IpConfigInfo { get; set; }
    private CHCNetSDK.NET_DVR_GET_STREAM_UNION StreamConfigInfo { get; set; }
    private uint DigitalChannelTotalNumber { get; set; } = 0;
    public string Ip { get; set; } //设备IP地址或者域名
    public ushort Port { get; set; }//设备服务端口号
    public string UserName { get; set; }//设备登录用户名
    public string Password { get; set; }//设备登录密码

    public Recorder(string ip, ushort port ,string username = "admin", string password = "11111111a")
    {
        Ip = ip;
        Port = port;
        UserName = username;
        Password = password;
    }

    public string Login()
    {
        CHCNetSDK.NET_DVR_Init();
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
            if (UserId >= 0)
                return "";
        }

        return Error();
    }
    

    
    public void GetAssociatedIpList(ref List<String> ips, ref List<String> names)
    {
        
        ips = new List<string>();
        names = new List<string>();
        if (DigitalChannelTotalNumber <= 0)
            return;
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

                string associateDeviceIp = System.Text.Encoding.GetEncoding("GBK").GetString(associateDeviceInfo.struIP.sIpV4).Trim('\0');
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
                    result = System.Text.Encoding.GetEncoding("GBK").GetString(ChannelImageInfo.sChanName).Trim('\0');
                }
                Console.WriteLine($"{Ip}-通道{i + 1}- {associateDeviceIp}-{result}");
                ips.Add(associateDeviceIp);
                names.Add(result);
                Marshal.FreeHGlobal(ptrPicCfg);
                
                
                Marshal.FreeHGlobal(ptrChanInfo);
            
            }
        }
        Marshal.FreeHGlobal(ptrIpParaCfgV40);
        return;
    }
    
 public void StartPlayback(IntPtr handle, DateTime start, DateTime end, uint channel)
    {
        
        
        
        // CHCNetSDK.NET_DVR_SetPlayDataCallBack()
        if (m_lPlayHandle >= 0)
        {
            //如果已经正在回放，先停止回放
            if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
            {
                Console.WriteLine(Error());
                return;
            }
            

            m_lPlayHandle = -1;

            // PlaybackprogressBar.Value = 0;回放进度条
        }

        CHCNetSDK.NET_DVR_VOD_PARA struVodPara = new CHCNetSDK.NET_DVR_VOD_PARA();
        struVodPara.dwSize = (uint)Marshal.SizeOf(struVodPara);
        struVodPara.struIDInfo.dwChannel = (uint)(32 + channel) ; //通道号 Channel number  
        struVodPara.hWnd = handle;//回放窗口句柄
        //设置回放的开始时间 Set the starting time to search video files
        struVodPara.struBeginTime.dwYear = start.Year;
        struVodPara.struBeginTime.dwMonth = start.Month;
        struVodPara.struBeginTime.dwDay = start.Day;
        struVodPara.struBeginTime.dwHour = start.Hour;
        struVodPara.struBeginTime.dwMinute = start.Minute;
        struVodPara.struBeginTime.dwSecond = start.Second;

        //设置回放的结束时间 Set the stopping time to search video files
        struVodPara.struEndTime.dwYear = end.Year;
        struVodPara.struEndTime.dwMonth = end.Month;
        struVodPara.struEndTime.dwDay = end.Day;
        struVodPara.struEndTime.dwHour = end.Hour;
        struVodPara.struEndTime.dwMinute = end.Minute;
        struVodPara.struEndTime.dwSecond = end.Second;
        //按时间回放 Playback by time
        m_lPlayHandle = CHCNetSDK.NET_DVR_PlayBackByTime_V40(UserId, ref struVodPara);
        if (m_lPlayHandle < 0)
        {
            Console.WriteLine(Error());
            return;
        }

        uint iOutValue = 0;
        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
        {
            Console.WriteLine(Error());
            return;
        }
        // timerPlayback.Interval = 1000; 用于定时更新UI线程中视频进度条的定时器
        // timerPlayback.Enabled = true;
    }
    public void StopPlayback()
    {
        if (m_lPlayHandle < 0)
        {
            return;
        }

        //停止回放
        if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
        {
            Console.WriteLine(Error());
            return;
        }

        // PlaybackprogressBar.Value = 0;
        // timerPlayback.Stop();
            
        m_lPlayHandle = -1;
        // VideoPlayWnd.Invalidate();//刷新窗口    
   
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