using System.Windows;
using RailwayMonitorClient.Models.Entities;
using RailwayMonitorClient.Sdks;
using RailwayMonitorClient.Models.Enums;

namespace RailwayMonitorClient.Views
{
    /// <summary>
    /// 全屏监控窗口
    /// </summary>
    public partial class FullScreenWindow : Window
    {
        private Camera _camera;
        private Device _device;
        public FullScreenWindow(Device device)
        {
            InitializeComponent();
            _device = device;
            _camera = new Camera
            (
                cameraIpAddress: device.Ip,
                port: (ushort)device.Port,
                realPlayHandle: FullScreenPictureBox.Handle
            );
            StartCameraPreview();
        }

      
        public void StartCameraPreview()
        {
            var actualChannel = _device.Channel;
                // 创建新的Camera实例
            if (_device.Type != (int)EnumDeviceType.摄像机)
            {
                // 录像机和超脑的通道号需要+32，第一个数字通道是33
                actualChannel = _device.Channel + 32;
            }
            // 登录并启动预览
            _camera.Login();
            _camera.StartPreview(channel: actualChannel, streamType: EnumStreamType.主码流, linkMode: EnumLinkMode.RTSP);
           
        }
        

        /// <summary>
        /// 窗口关闭时清理资源
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // 清理摄像头资源
            if (_camera != null)
            {
                
                _camera.Dispose();
                _camera = null;
            }
        }

        private void FullScreenPictureBoxDoubleClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            Close();
        }
    }
}