using System.Text;
using HK.Net.Core;
using RailwayMonitorClient.Models.Enums;

namespace RailwayMonitorClient.Sdks;
/// <summary>
/// 相机接口
/// InitializeSdk(程序启动) => Login => Dispose => CleanUpSdk(程序结束)
/// </summary>
public class Camera : IDisposable
{
    private CHCNetSDK.NET_DVR_USER_LOGIN_INFO LoginInfo { get; set; }
    private CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo { get; set; }
    public EnumDeviceType DeviceType { get; set; }
    public int UserId { get; set; } = -1;
    private IntPtr RealPlayHandle { get; set; }= -1;
    private Int32 m_lPort = -1;
    public string Password { get; set; }
    public string UserName;
    public string CameraIpAddress;
    public ushort Port;
    private uint iLastErr = 0;
    private string str;
    private CHCNetSDK.REALDATACALLBACK RealData = null;
    
    public Camera(string cameraIpAddress, string userName = "admin", string password = "11111111a" , ushort port = 8000, IntPtr realPlayHandle = -1, EnumDeviceType deviceType = EnumDeviceType.摄像机)
    {
        CameraIpAddress = cameraIpAddress;
        RealPlayHandle = realPlayHandle;
        DeviceType = deviceType;
        UserName = userName;
        Password = password;
        Port = port;
    }
    ~Camera()
    {
        Dispose();
    }
    public void Dispose()
    {
        CHCNetSDK.NET_DVR_Logout(UserId);
        UserId = -1;
        RealPlayHandle = -1;
        CHCNetSDK.NET_DVR_Cleanup();
    }
    public string RtspAddress()
    {
        return $"rtsp://{UserName}:{Password}@{CameraIpAddress}:554/h264/ch1/main/av_stream";
    }
    public void Login()
    {
        CHCNetSDK.NET_DVR_Init();
        CHCNetSDK.NET_DVR_SetConnectTime(2000, 1);//设置超时时间
        CHCNetSDK.NET_DVR_SetReconnect(10000, 1);//设置重连时
        byte[] bytesUserName = Encoding.Default.GetBytes(UserName);
        byte[] bytesPassword = Encoding.Default.GetBytes(Password);
        byte[] bytesDeviceAddress = Encoding.Default.GetBytes(CameraIpAddress);

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
        LoginInfo = loginInfo;
        
        var deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
        DeviceInfo = deviceInfo;
        UserId = CHCNetSDK.NET_DVR_Login_V40(ref loginInfo, ref deviceInfo);
        
        if (UserId == -1)
        {
            throw new Exception(Error());
        }

    }
    
   

    
    
    public void StartPreview(int channel = 1, EnumStreamType streamType = EnumStreamType.主码流, EnumLinkMode linkMode = EnumLinkMode.TCP, EnumBlockMode blockMode = EnumBlockMode.阻塞取流)
    {
        if (RealPlayHandle != -1)
        {
            
            CHCNetSDK.NET_DVR_StopRealPlay((int)RealPlayHandle);
        }

        CHCNetSDK.NET_DVR_PREVIEWINFO previewInfo = new()
        {
            hPlayWnd = RealPlayHandle,
            lChannel = (int)channel,//预览的设备通道 the device channel number
            dwStreamType = (uint) streamType,//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
            dwLinkMode = (uint) linkMode, //连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
            bBlocked = blockMode == EnumBlockMode.阻塞取流, //0- 非阻塞取流，1- 阻塞取流
            dwDisplayBufNum = 1,
            byProtoType = 0,// 应用层取流协议 0私有协议 1RTSP协议
            byPreviewMode = 0
            
        };


        // 强制I帧
        //CCHCNetSDK.NET_DVR_MakeKeyFrame(UserId, _deviceInfo.struDeviceV30.byStartChan);

        // 默认PS流封装

        var dataCallBack = new CHCNetSDK.REALDATACALLBACK(((handle, type, buffer, size, user) =>
        {
            // if (size > 0)
            // {
            //     byte[] data = new byte[size];
            //     Marshal.Copy(buffer, data, 0, (Int32)size);
            //     string str = "stream.ps";
            //     FileStream fs = new FileStream(str, FileMode.Create);
            //     int l = (int)size;
            //     fs.Write(data, 0, l);
            //     fs.Close();
            // }
        }));
        RealPlayHandle = CHCNetSDK.NET_DVR_RealPlay_V40(UserId, ref previewInfo, dataCallBack, new IntPtr());
        if (RealPlayHandle < 0)
        {
            throw new Exception(Error());
        };
    }
    /// <summary>
    /// 停止视频取流
    /// </summary>
    public void StopPreview()
    {
        CHCNetSDK.NET_DVR_StopRealPlay((int)RealPlayHandle);
        RealPlayHandle = -1;
    }

    /// <summary>
    /// 直接抓取图片,无需启动实时数据流获取，阻塞线程
    /// </summary>
    public byte[] DirectlyCaptureJpegImage()
    {
        CHCNetSDK.NET_DVR_JPEGPARA outJpegParam = new();
        byte[] buffer = new byte[1024 * 1024 * 50];// 50M
        uint size = 0;
        bool ok = CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(UserId, DeviceInfo.struDeviceV30.byStartChan, ref outJpegParam, buffer,
            (uint)buffer.Length, ref size);
        if (!ok) throw new Exception(Error());
        byte[] ret = new byte[size];
        Array.Copy(buffer, ret, size);
        return ret;
    }

    /// <summary>
    /// 获取海康SDK的错误码
    /// </summary>
    public uint GetLastErrorCode()
    {
        
        return CHCNetSDK.NET_DVR_GetLastError();
    }

    public bool Reboot()
    {
        return CHCNetSDK.NET_DVR_RebootDVR(UserId);
    }
    
    public string Error()
    {
        int code = (int) CHCNetSDK.NET_DVR_GetLastError();
        return $"错误码={code}, 错误描述={ErrorCode.GetDescription(code)}";
    }
}
