using System.Runtime.InteropServices;
using HK.Net.Core;
using RailwayAlarmBackend.Models.Enums;
namespace RailwayAlarmBackend.Services;
public class RecorderService : IDisposable
{
    private Int32 m_lUserID = -1;
    private Int32 m_lPlayHandle = -1;
    private Int32 m_lDownHandle = -1;
    private bool m_bPause = false;
    private bool m_bReverse = false;
    private bool m_bSound = false;
    /// <summary>
    /// 第iSelIndex个通道，从0开始算第一个
    /// </summary>
    private long iSelIndex = 0; 
    private uint dwAChanTotalNum = 0;
    private uint dwDChanTotalNum = 0;
    public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
    public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
    public CHCNetSDK.NET_DVR_GET_STREAM_UNION m_unionGetStream;
    public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
    private string DVRIPAddress { get; } //设备IP地址或者域名
    private Int16 DVRPortNumber { get; }//设备服务端口号
    private string DVRUserName { get; }//设备登录用户名
    private string DVRPassword { get; }//设备登录密码
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96, ArraySubType = UnmanagedType.U4)]
    private int[] iChannelNum;

    public RecorderService(string ip, string port = "8000", string username = "admin", string password = "11111111a")
    {
        DVRIPAddress = ip;
        DVRPortNumber = short.Parse(port);
        DVRUserName = username;
        DVRPassword = password;
    }

    public void Login()
    {
        bool initializing = CHCNetSDK.NET_DVR_Init();
        if (initializing == false)
        {
            Console.WriteLine(Error());
            return;
        }
        iChannelNum = new int[96];
        if (m_lUserID < 0)
        {
            
            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);

            dwAChanTotalNum = (uint)DeviceInfo.byChanNum;
            dwDChanTotalNum = (uint)DeviceInfo.byIPChanNum + 256 * (uint)DeviceInfo.byHighDChanNum;
            if (dwDChanTotalNum > 0)
                InfoIPChannel();
        }
    }
    public void InfoIPChannel()
    {
         uint dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40);

        IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
        Marshal.StructureToPtr(m_struIpParaCfgV40, ptrIpParaCfgV40, false);

        uint dwReturn = 0;
        int iGroupNo = 0; //该Demo仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取
        if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
        {
            Console.WriteLine(Error());
        }
        else
        {
            // succ
            m_struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));
           
            for (int i = 0; i < dwAChanTotalNum; i++)
            {
                
                iChannelNum[i] = i + (int)DeviceInfo.byStartChan;                     
            }
            
            byte byStreamType;
            uint iDChanNum = 64;

            if (dwDChanTotalNum < 64)
            {
                iDChanNum = dwDChanTotalNum; //如果设备IP通道小于64路，按实际路数获取
            }

            for (int i = 0; i < iDChanNum; i++)
            {
                iChannelNum[i + dwAChanTotalNum] = i + (int)m_struIpParaCfgV40.dwStartDChan;

                byStreamType = m_struIpParaCfgV40.struStreamMode[i].byGetStreamType;
                m_unionGetStream = m_struIpParaCfgV40.struStreamMode[i].uGetStream;

                switch (byStreamType)
                {
                    //目前NVR仅支持0- 直接从设备取流一种方式
                    case 0:
                        dwSize = (uint)Marshal.SizeOf(m_unionGetStream);
                        IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                        Marshal.StructureToPtr(m_unionGetStream, ptrChanInfo, false);
                        m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));

                        //列出IP通道
                        Marshal.FreeHGlobal(ptrChanInfo);
                        break;

                    default:
                        break;
                }
            }
        }
        Marshal.FreeHGlobal(ptrIpParaCfgV40);
    }
    public void recorderDataCallBack(int handle, uint type, IntPtr buffer, uint size, uint user)
    {
        if (size > 0)
        {
            byte[] data = new byte[size];
            Marshal.Copy(buffer, data, 0, (Int32)size);
            string str = "stream.ps";
            FileStream fs = new FileStream(str, FileMode.Create);
            int l = (int)size;
            fs.Write(data, 0, l);
            fs.Close();
        }
    }
    public void StartPlayback(IntPtr handle, DateTime start, DateTime end)
    {
        var playDataCallBack = new CHCNetSDK.PLAYDATACALLBACK(recorderDataCallBack);
        CHCNetSDK.NET_DVR_SetPlayDataCallBack((int)handle, playDataCallBack, (uint)m_lUserID);
        
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
        struVodPara.struIDInfo.dwChannel = (uint)iChannelNum[(int)iSelIndex]; //通道号 Channel number  
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
        m_lPlayHandle = CHCNetSDK.NET_DVR_PlayBackByTime_V40(m_lUserID, ref struVodPara);
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
    public void StartDownload(DateTime start, DateTime end, string rootPath)
    {
        if (m_lDownHandle >= 0)
        {
            Console.WriteLine(Error());//正在下载，请先停止下载
            return;
        }

        CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
        struDownPara.dwChannel = (uint)iChannelNum[(int)iSelIndex]; //通道号 Channel number  

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
        sVideoFileName = "D:\\Downtest_Channel"+struDownPara.dwChannel+".mp4";

        //按时间下载 Download by time
        m_lDownHandle = CHCNetSDK.NET_DVR_GetFileByTime_V40(m_lUserID, sVideoFileName, ref struDownPara);
        if (m_lDownHandle < 0)
        {
            Console.WriteLine(Error());
            return;
        }

        uint iOutValue = 0;
        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lDownHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
        {
            Console.WriteLine(Error());
            return;
        }

        // timerDownload.Interval = 1000;
        // timerDownload.Enabled = true; 用于更新UI线程中下载进度条的定时器
        // btnStopDownload.Enabled = true;停止下载按钮
    }
    public void StopDownload()
    {
        if(m_lDownHandle<0)
        {
            return;            
        }

        if (!CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle))
        {
           
            Console.WriteLine(Error());
            return;
        }

        // timerDownload.Stop(); 

        Console.WriteLine("The downloading has been stopped succesfully!");
        m_lDownHandle = -1;
        // DownloadProgressBar.Value = 0;
        // btnStopDownload.Enabled = true;
    }
    public void Pause()
    {
        uint iOutValue = 0;

        if (!m_bPause)
        {
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYPAUSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                Console.WriteLine(Error());
                return;
            }
            m_bPause = true;
            // btnPause.Text = ">";
            // labelPause.Text = "播放";
        }
        else
        {
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYRESTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                Console.WriteLine(Error());
                return;
            }
            m_bPause = false;
            // btnPause.Text = "||";
            // labelPause.Text = "暂停";
        }
    }
    public void Reverse()
    {
        uint iOutValue = 0;
        if (!m_bReverse)
        {
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAY_REVERSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                Console.WriteLine(Error());
                return;
            }
            m_bReverse = true;
            // btnReverse.Text = "Forward";
            // labelReverse.Text = "切换为正放";
        }
        else
        {
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAY_FORWARD, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                Console.WriteLine(Error());
                return;
            }
            m_bReverse = false;
            // btnReverse.Text = "Reverse";
            // labelReverse.Text = "切换为倒放";       
        }

    }
    public void OpenSound()
    {
        uint iOutValue = 0;
        if (!m_bSound)
        {
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSTARTAUDIO, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                Console.WriteLine(Error());
                return;
            }
            m_bSound = true;
            // btnSound.Text = "Stop";
            // labelSound.Text = "关闭声音";
        }
        else
        {
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSTOPAUDIO, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                Console.WriteLine(Error());
                return;
            }
            m_bSound = false;
            // btnSound.Text = "Sound";
            // labelSound.Text = "打开声音";
        }
    }
    public void FastPlay()
    {
        uint iOutValue = 0;

        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYFAST, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
        {
            Console.WriteLine(Error());
            return;
        }
    }
    public void SlowPlay()
    {
        uint iOutValue = 0;

        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSLOW, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
        {
            Console.WriteLine(Error());
            return;
        }
    }
    private void FramePlay()
    {
        uint iOutValue = 0;

        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYFRAME, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
        {
            Console.WriteLine(Error());
            return;
        }
    }
    private void NormalPlay()
    {
        uint iOutValue = 0;
        if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYNORMAL, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
        {
            Console.WriteLine(Error());
            return;
        }
    }
    private void CaptureImage(object sender, EventArgs e)
    {
        if (m_lPlayHandle < 0)
        {
            Console.WriteLine("Please start playback firstly!"); //BMP抓图需要先打开预览
            return;
        }

        string sBmpPicFileName;
        //图片保存路径和文件名 the path and file name to save
        sBmpPicFileName = "test.bmp";

        //BMP抓图 Capture a BMP picture
        if (!CHCNetSDK.NET_DVR_PlayBackCaptureFile(m_lPlayHandle, sBmpPicFileName))
        {
            Console.WriteLine(Error());
            return;
        }
        else
        {
            Console.WriteLine($"文件保存为{sBmpPicFileName}");
        }
        return;
    }
    public string Error()
    {
        int code = (int) CHCNetSDK.NET_DVR_GetLastError();
        return $"错误码={code}, 错误描述={EnumErrorCode.GetDescription(code)}";
    }

    public void Dispose()
    {
        try
        {
            // 停止播放
            if (m_lPlayHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle);
                m_lPlayHandle = -1;
            }

            // 停止下载
            if (m_lDownHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle);
                m_lDownHandle = -1;
            }

            // 注销登录
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            // 清理SDK
            CHCNetSDK.NET_DVR_Cleanup();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"资源释放异常: {ex.Message}");
        }
    }

    ~RecorderService()
    {
        Dispose();
    }
}