using System.Runtime.InteropServices;
using System.Text;
using HK.Net.Core;
using Newtonsoft.Json;
using RailwayAlarmBackend.Interfaces;
using RailwayAlarmBackend.Models.Enums;

namespace RailwayAlarmBackend.Sdks;

class CheckResult
{
    public int AttrType { get; set; }
    public int AttrValue { get; set; }
    public string? Result { get; set; }

}
public class SuperBrain(
    string ip,
    string port,
    string username,
    string password,
    string alarmImageFolder,
    IAlarmTraceService? alarmTraceService = null)
    : IDisposable
{
    ~SuperBrain()
    {
        Dispose();
    }
    public void Dispose()
    {
        CHCNetSDK.NET_DVR_Logout(_userId);
        _userId = -1;
        CHCNetSDK.NET_DVR_Cleanup();
    }

    private IAlarmTraceService? AlarmTraceService { get; set; } = alarmTraceService;
    private string Ip { get; set; } = ip;
    private string Port { get; set; } = port;
    private string Username { get; set; } = username;
    private string Password { get; set; } = password;

    private CHCNetSDK.NET_DVR_PICCFG_V40 ChannelImageInfo { get; set; }
    private CHCNetSDK.NET_DVR_IPPARACFG_V40 IpConfigInfo { get; set; }
    private CHCNetSDK.NET_DVR_GET_STREAM_UNION StreamConfigInfo { get; set; }
    private uint DigitalChannelTotalNumber { get; set; } = 0;
    private string AlarmImageFolder { get; set; } = alarmImageFolder;
    private int _userId = -1;
    private int _iFileNumber = 0;
    private CHCNetSDK.MSGCallBack_V31 AlarmCallBack { get; set; }= null;
    private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

    public string GetModelInfo()
    {
        string strRequestUrl = "GET /ISAPI/Intelligent/AIOpenPlatform/algorithmModel/management?format=json";
        CHCNetSDK.NET_DVR_XML_CONFIG_INPUT pInputXml = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            Int32 nInSize = Marshal.SizeOf(pInputXml);
            pInputXml.dwSize = (uint)nInSize;

            
            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            pInputXml.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            pInputXml.dwRequestUrlLen = dwRequestUrlLen;

            string strInputParam = ""; // 如果是模型下发和任务下发会用到
            byte[] byInputParam = Encoding.UTF8.GetBytes(strInputParam);

            int iXMLInputLen = byInputParam.Length;
            pInputXml.lpInBuffer = Marshal.AllocHGlobal(iXMLInputLen);
            Marshal.Copy(byInputParam, 0, pInputXml.lpInBuffer, iXMLInputLen);
            pInputXml.dwInBufferSize = (uint)byInputParam.Length;

            CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT pOutputXml = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
            pOutputXml.dwSize = (uint)Marshal.SizeOf(pInputXml);
            pOutputXml.lpOutBuffer = Marshal.AllocHGlobal(3 * 1024 * 1024);
            pOutputXml.dwOutBufferSize = 3 * 1024 * 1024;
            pOutputXml.lpStatusBuffer = Marshal.AllocHGlobal(4096 * 4);
            pOutputXml.dwStatusSize = 4096 * 4;

            if (!CHCNetSDK.NET_DVR_STDXMLConfig(_userId, ref pInputXml, ref pOutputXml))
                throw new Exception(Error());

            uint iXMSize = pOutputXml.dwReturnedXMLSize;
            byte[] managedArray = new byte[iXMSize];
            Marshal.Copy(pOutputXml.lpOutBuffer, managedArray, 0, (int)iXMSize);
            string outXml = Encoding.UTF8.GetString(managedArray); 
            string outStatus = Marshal.PtrToStringAnsi(pOutputXml.lpStatusBuffer);

            Marshal.FreeHGlobal(pInputXml.lpRequestUrl);
            Marshal.FreeHGlobal(pOutputXml.lpOutBuffer);
            Marshal.FreeHGlobal(pOutputXml.lpStatusBuffer);
            return outXml;
    }
    
    
    public string Error()
    {
        int code = (int) CHCNetSDK.NET_DVR_GetLastError();
        return $"错误码={code}, 错误描述={ErrorCode.GetDescription(code)}";
    }
    
    
    
    
    
    /// 1 初始化
    /// 2 设置回调函数
    /// 3 登录
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Login()
    {
        //1. 必须初始化
        CHCNetSDK.NET_DVR_Init();
        //2. 配置透传报警信息类型 （可选）
        CHCNetSDK.NET_DVR_LOCAL_GENERAL_CFG struLocalCfg = new CHCNetSDK.NET_DVR_LOCAL_GENERAL_CFG();
        // 这样设置之后SDK底层会自动将报警事件信息数据（XML或者JSON格式的字符串）和图片数据（二进制数据）分离之后返回。如果不调用该接口设置byAlarmJsonPictureSeparate或者设置为0，默认直接以HTTP协议表单格式的报文数据返回即报警信息字符串和图片二进制数据一起返回，在同一个内存里面，需要自行解析。建议设置报警分离模式.
        struLocalCfg.byAlarmJsonPictureSeparate = 1;//控制JSON透传报警数据和图片是否分离，0-不分离(COMM_VCA_ALARM返回)，1-分离（分离后走COMM_ISAPI_ALARM回调返回）
        Int32 nSize = Marshal.SizeOf(struLocalCfg);
        IntPtr ptrLocalCfg = Marshal.AllocHGlobal(nSize);
        Marshal.StructureToPtr(struLocalCfg, ptrLocalCfg, false);
        if (!CHCNetSDK.NET_DVR_SetSDKLocalCfg(17, ptrLocalCfg))
            throw new Exception(Error());
        Marshal.FreeHGlobal(ptrLocalCfg);
        
        //3. 设置报警回调函数
        AlarmCallBack = SuperBrainAlarmCallBack;
        CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(AlarmCallBack, IntPtr.Zero);
        CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();
        
        //4. 登录设备
        //设备IP地址或者域名
        byte[] byIP = System.Text.Encoding.Default.GetBytes(Ip);
        struLogInfo.sDeviceAddress = new byte[129];
        byIP.CopyTo(struLogInfo.sDeviceAddress, 0);

        //设备用户名
        byte[] byUserName = System.Text.Encoding.Default.GetBytes(Username);
        struLogInfo.sUserName = new byte[64];
        byUserName.CopyTo(struLogInfo.sUserName, 0);

        //设备密码
        byte[] byPassword = System.Text.Encoding.Default.GetBytes(Password);
        struLogInfo.sPassword = new byte[64];
        byPassword.CopyTo(struLogInfo.sPassword, 0);

        //设备端口
        struLogInfo.wPort = ushort.Parse(Port);//设备服务端口号
        //其他可选参数
        struLogInfo.bUseAsynLogin = false; //是否异步登录：0- 否，1- 是 
        struLogInfo.byLoginMode = 0; //0-Private, 1-ISAPI, 2-自适应
        struLogInfo.byHttps = 0; //0-不适用tls，1-使用tls 2-自适应

        // V40最新接口登录，兼容V30
        CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
        //登录设备 Login the device
        _userId = CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);
        DigitalChannelTotalNumber = DeviceInfo.struDeviceV30.byIPChanNum + 256 * (uint)DeviceInfo.struDeviceV30.byHighDChanNum;
        if (_userId < 0)
            throw new Exception(Error());
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
        if (!CHCNetSDK.NET_DVR_GetDVRConfig(_userId, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
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
                if (!CHCNetSDK.NET_DVR_GetDVRConfig(_userId, CHCNetSDK.NET_DVR_GET_PICCFG_V40, i + (int)IpConfigInfo.dwStartDChan, ptrPicCfg, (UInt32)nSize, ref dwReturn))
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
    private bool SuperBrainAlarmCallBack(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
    {
        _dbSemaphore.Wait(1500);
        //设备支持AI开放平台接入，处理媒体类型是 实时视频流
        if (lCommand != CHCNetSDK.COMM_UPLOAD_AIOP_VIDEO)
            return false;
      

        //回调函数逻辑
        CHCNetSDK.NET_AIOP_VIDEO_HEAD struAIOPVideo = new CHCNetSDK.NET_AIOP_VIDEO_HEAD();
        uint dwSize = (uint)Marshal.SizeOf(struAIOPVideo);
        struAIOPVideo = (CHCNetSDK.NET_AIOP_VIDEO_HEAD)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_AIOP_VIDEO_HEAD));
        
        //报警设备的IP地址
        string strIP = Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');
        //报警设备位于接入超脑的通道号
        int channel = Convert.ToInt16(struAIOPVideo.dwChannel);
        
        //报警时间：年月日时分秒
        string strTimeYear = (struAIOPVideo.struTime.wYear).ToString();
        string strTimeMonth = (struAIOPVideo.struTime.wMonth).ToString("d2");
        string strTimeDay = (struAIOPVideo.struTime.wDay).ToString("d2");
        string strTimeHour = (struAIOPVideo.struTime.wHour).ToString("d2");
        string strTimeMinute = (struAIOPVideo.struTime.wMinute).ToString("d2");
        string strTimeSecond = (struAIOPVideo.struTime.wSecond).ToString("d2");
        string strTime = $"{strTimeYear}-{strTimeMonth}-{strTimeDay} {strTimeHour}:{strTimeMinute}:{strTimeSecond}";

        DateTime alarmTime = DateTime.Parse(strTime);
        string data = Marshal.PtrToStringAnsi(struAIOPVideo.pBufferAIOPData).Replace("\n", "").Replace("\t", "");
        //Console.WriteLine(data+ " " + DateTime.Now.ToString());

        // 分析数据
        //CheckResult result_1 = AnalyzeSuperBrainResponse(data, "6893b26fe4b04517a94ac36dca1fbb44");// 反光衣
        CheckResult result_1 = AnalyzeSuperBrainResponse(data, "dc00279fa260418891c88fd7a4179295");// 反光衣
        //CheckResult result_2 = AnalyzeSuperBrainResponse(data, "d71a6546b0284384a757935bc889abd6");// 帽子
        CheckResult result_2 = AnalyzeSuperBrainResponse(data, "7dfb0893d6e94d0cbe0bf60ef53fccc8");// 帽子

        if (result_1.Result == null || result_2.Result == null)
            return false;
        EnumAlarmType type = EnumAlarmType.均穿戴;
        if (result_1.Result == "yes" && result_2.Result == "no")
            type = EnumAlarmType.未戴安全帽;
        else if (result_1.Result == "no" && result_2.Result == "yes")
            type = EnumAlarmType.未穿反光衣;
        else if (result_1.Result == "no" && result_2.Result == "no")
            type = EnumAlarmType.均未穿戴;
        
        //保存图片
        string filename = $"{strIP}_{channel}_{nameof(type)}.jpg";
        string filePath = Path.Combine(AlarmImageFolder, filename);
        if ((struAIOPVideo.dwPictureSize != 0) && (struAIOPVideo.pBufferPicture != IntPtr.Zero))
        {
            FileStream fsPic = new FileStream(filePath, FileMode.Create);
            int iPicLen = (int)struAIOPVideo.dwPictureSize;
            byte[] byPic = new byte[iPicLen];
            Marshal.Copy(struAIOPVideo.pBufferPicture, byPic, 0, iPicLen);
            fsPic.Write(byPic, 0, iPicLen);
            fsPic.Close();
        }
        
        if (AlarmTraceService is not null)
        //添加告警记录
            AlarmTraceService.AddAlarmTraceAsync(strIP, channel, (int) type, filePath, alarmTime, data);
        _dbSemaphore.Release();
        return true; //回调函数需要有返回，表示正常接收到数据
    }
    
    
    

    private CheckResult AnalyzeSuperBrainResponse(string data, string typeKey)
    {
            if (data.Length == 0)
                return null;
            
                
            // 检测id
            int start = data.IndexOf(typeKey);
            if (start <= 0)
                return null;
            
               
            string b = data.Substring(start + typeKey.Length, data.Length - (start + typeKey.Length));

            // 检测结果
            string strResultStart = "\"classify\":";
            string strResultEnd = "}";
            int ResultStart = b.IndexOf(strResultStart);
            int ResultEnd = b.IndexOf(strResultEnd);
            if (ResultStart > 0 && ResultEnd > 0)
            {
                // 获取数据
                string c = b.Substring(ResultStart + strResultStart.Length, ResultEnd - strResultStart.Length - 1);
                CheckResult result = JsonConvert.DeserializeObject<CheckResult>(c);
                if ((result.AttrType == 0 && result.AttrValue == 0))
                {
                    result.Result = "no";
                    return result;
                }
                else if ((result.AttrType == 0 && result.AttrValue == 1))
                {
                    result.Result = "yes";
                    return result;
                }
                else
                {
                    string d = b.Substring(ResultEnd + 1, b.Length - (ResultEnd + 1));
                    return AnalyzeSuperBrainResponse(d, typeKey);
                }
            }
            return null;
        
    }

    // 设置布防
    public void SetupAlarm()
    {
        CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
        struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
        struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
        struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
        struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测
        if (CHCNetSDK.NET_DVR_SetupAlarmChan_V41(_userId, ref struAlarmParam) < 0)
        {
            Console.WriteLine(Error());                   
        }
        else
        {
            // 布防成功
        }
    }
    
    private string Save_AlarmPic(CHCNetSDK.NET_AIOP_VIDEO_HEAD struAIOPVideo, string strPic)
    {
        // 判断文件夹是否存在
        DirectoryInfo dinfo = new DirectoryInfo(strPic);
        if (!dinfo.Exists)
        {
            dinfo.Create();
        }
        // 保存图片
        strPic = strPic + "/" + Guid.NewGuid().ToString() + ".jpg";
        FileStream fsPic = new FileStream(strPic, FileMode.Create);
        int iPicLen = (int)struAIOPVideo.dwPictureSize;
        byte[] byPic = new byte[iPicLen];
        Marshal.Copy(struAIOPVideo.pBufferPicture, byPic, 0, iPicLen);
        fsPic.Write(byPic, 0, iPicLen);
        fsPic.Close();

        return strPic;
    }
    
   
    
    
    
    
    
    
    private void TrainAlarm(string ChaonaoGuid, string CameraIP, string CameraName, int AlarmType, string AlarmTypeName, string AlarmTime, string PicPath, bool AutoClose)
    {
        //告警弹窗原实现新建了一个winform+PictureBox渲染视频回放
        // string strGuid = Guid.NewGuid().ToString();
        // TrainAlarm tam = new TrainAlarm(strGuid, AlarmTypeName, CameraName, DateTime.Now.ToString(), PicPath, AutoClose);
        // tam.CamearAlarm(); // 告警弹窗

        // // 添加报警记录 数据库入库
        // AlarmRecodeData ta = new AlarmRecodeData();
        // ta.Guid = strGuid;
        // ta.DriveGuid = ChaonaoGuid;
        // ta.DriveIP = CameraIP;
        // ta.DriveName = CameraName;
        // ta.AlarmTime = DateTime.Now;
        // ta.AlarmType = AlarmType;
        // ta.AlarmHandleState = 1;
        // ta.Remark = PicPath.Replace("\\", "/");
        // Opt_AlarmRecode.AddRecord(ta);
        //
        // // 更新角标数量
        // GetAlarmCount();
    }
    
    private void PeopleIn(string ChaonaoGuid, string CameraIP, string CameraName, int AlarmType, string AlarmTypeName, string AlarmTime, string PicPath)
    {
        string strGuid = Guid.NewGuid().ToString();

        // 添加正常人员进入记录，数据库入库
        // AlarmRecodeData ta = new AlarmRecodeData();
        // ta.Guid = strGuid;
        // ta.DriveGuid = ChaonaoGuid;
        // ta.DriveIP = CameraIP;
        // ta.DriveName = CameraName;
        // ta.AlarmTime = DateTime.Now;
        // ta.AlarmType = 2;
        // ta.AlarmHandleState = 1;
        // ta.Remark = PicPath.Replace("\\", "/");
        // Opt_AlarmRecode.AddRecord(ta);
    }
    
    private void AddPeopleWear(string ChaonaoGuid, string CameraIP, string CameraName, int AlarmType, string PicPath)
    {
        // 添加报警记录 数据库入库
        // AlarmRecodeData ta = new AlarmRecodeData();
        // ta.Guid = Guid.NewGuid().ToString(); 
        // ta.DriveGuid = ChaonaoGuid;
        // ta.DriveIP = CameraIP;
        // ta.DriveName = CameraName;
        // ta.AlarmTime = DateTime.Now;
        // ta.AlarmType = AlarmType;
        // ta.AlarmHandleState = 1;
        // ta.Remark = PicPath.Replace("\\", "/");
        // Opt_AlarmRecode.AddPeopleWear(ta);
    }


}

