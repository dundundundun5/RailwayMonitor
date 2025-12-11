using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HK.Net.Core;
using Newtonsoft.Json;
using RailwayMonitorClient.Interfaces;
using RailwayMonitorClient.Models.Enums;
using Timer = System.Timers.Timer;

namespace RailwayMonitorClient.Sdks;

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
    int second = 60,
    bool recognizeHat = false,
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
    private static int[] PreviousType = [
        -1,
        -1,
        -1,
        -1,
        -1,
        -1,
        -1,
        -1
    ];

    private System.Timers.Timer[] Mytimers { get; set; } =
    [
        new Timer(second * 1000),
        new Timer(second * 1000),
        new Timer(second * 1000),
        new Timer(second * 1000),
        new Timer(second * 1000),
        new Timer(second * 1000),
        new Timer(second * 1000),
        new Timer(second * 1000),
    ]; 
    
    private uint DigitalChannelTotalNumber { get; set; } = 0;
    private string AlarmImageFolder { get; set; } = alarmImageFolder;
    private int _userId = -1;
    private int _iFileNumber = 0;
    private CHCNetSDK.MSGCallBack_V31 AlarmCallBack { get; set; }= null;
    public string Error()
    {
        int code = (int)CHCNetSDK.NET_DVR_GetLastError();
        return $"错误码={code}, 错误描述={ErrorCode.GetDescription(code)}";
    }

    /// 1 初始化
    /// 2 设置回调函数
    /// 3 登录
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Login()
    {
        for(int i = 0; i < Mytimers.Length; i++)
        {
            Mytimers[i].AutoReset = false;
            Mytimers[i].Elapsed += (sender, args) =>
            {
                PreviousType[i] = -1;
            };
            
        }
        
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
    private bool SuperBrainAlarmCallBack(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
    {
       
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
        string strTimeMiliSecond = (struAIOPVideo.struTime.wMilliSec).ToString("d3");
        string strTime = $"{strTimeYear}-{strTimeMonth}-{strTimeDay} {strTimeHour}:{strTimeMinute}:{strTimeSecond}";

        DateTime alarmTime = DateTime.Parse(strTime);
        string data = Marshal.PtrToStringAnsi(struAIOPVideo.pBufferAIOPData).Replace("\n", "").Replace("\t", "");
        //Console.WriteLine(data+ " " + DateTime.Now.ToString());

        // 分析数据
        //CheckResult result_1 = AnalyzeSuperBrainResponse(data, "6893b26fe4b04517a94ac36dca1fbb44");// 反光衣
        CheckResult result_1 = AnalyzeSuperBrainResponse(data, "dc00279fa260418891c88fd7a4179295");// 反光衣
        //CheckResult result_2 = AnalyzeSuperBrainResponse(data, "d71a6546b0284384a757935bc889abd6");// 帽子
        CheckResult result_2 = AnalyzeSuperBrainResponse(data, "7dfb0893d6e94d0cbe0bf60ef53fccc8");// 帽子
        try
        {
            if (result_1 == null || result_2 == null)
                return false;
            if (result_1.Result == null || result_2.Result == null)
                return false;
        }
        catch (Exception e)
        {
            return false;
        }
        EnumAlarmType type = EnumAlarmType.均穿戴;
        if (result_1.Result == "yes" && result_2.Result == "no")
            type = EnumAlarmType.未戴安全帽;
        else if (result_1.Result == "no" && result_2.Result == "yes")
            type = EnumAlarmType.未穿反光衣;
        else if (result_1.Result == "no" && result_2.Result == "no")
            type = EnumAlarmType.均未穿戴;
        if (!recognizeHat)
        {
            if (type is EnumAlarmType.未戴安全帽)
                return true;
            if (type is EnumAlarmType.均未穿戴 or EnumAlarmType.未穿反光衣)
                type = EnumAlarmType.未穿反光衣;
        }
        
        if (type == EnumAlarmType.均穿戴)
            return true;
        
        
        //告警去重
        int idx = channel - 33 >= Mytimers.Length ? 0 : channel - 33;
        if ((int)type == PreviousType[idx])
        {
            Console.WriteLine($"通道{idx}检测到重复告警{type.ToString()}");
            return true;
        }
        else
        {
            PreviousType[idx] = (int)type;
            Mytimers[idx].Stop();
            Mytimers[idx].Start();
        }
        //保存图片
        strTime = $"{strTimeYear}-{strTimeMonth}-{strTimeDay}_{strTimeHour}-{strTimeMinute}-{strTimeSecond}-{strTimeMiliSecond}";
        string filename = $"{strIP}_{channel}_{nameof(type)}_{strTime}.jpg";
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

}

