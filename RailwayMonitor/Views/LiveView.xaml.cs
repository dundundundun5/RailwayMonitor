using System.Windows;
using System.Windows.Controls;
using RailwayMonitor.Models.Entities;
using RailwayMonitor.Sdks;
using ToolTip = System.Windows.Controls.ToolTip;
using WinForms = System.Windows.Forms;

namespace RailwayMonitor.Views
{
    /// <summary>
    /// LiveView控件 - 3行4列PictureBox，支持别名显示
    /// </summary>
    public partial class LiveView
    {
        private const int ROWS = 3;
        private const int COLS = 4;
        private const int TOTAL_CELLS = ROWS * COLS;

        // PictureBox句柄数组
        private IntPtr[] _pictureBoxHandles;

        // 别名数组
        private string[] _aliases;

        private List<Device> _devices;
        // 容器数组
        private Border[] _containers;

        // PictureBox数组
        private WinForms.PictureBox[] _pictureBoxes;


        public Camera Camera { get; set; }

        public LiveView(List<Device> devices)
        {
            InitializeComponent();
            InitializeArrays();
            _devices = devices;
        }

        /// <summary>
        /// 初始化数组
        /// </summary>
        private void InitializeArrays()
        {
            _pictureBoxHandles = new IntPtr[TOTAL_CELLS];
            _aliases = new string[TOTAL_CELLS];
            _containers = new Border[TOTAL_CELLS];
            _pictureBoxes = new PictureBox[TOTAL_CELLS];

            // 初始化容器和PictureBox引用
            _containers[0] = Container00; 
            _pictureBoxes[0] = PictureBox00;
            PictureBox00.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 0);
            
            _containers[1] = Container01; 
            _pictureBoxes[1] = PictureBox01;
            PictureBox01.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 1);
            
            _containers[2] = Container02; 
            _pictureBoxes[2] = PictureBox02;
            PictureBox02.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 2);
            
            _containers[3] = Container03; 
            _pictureBoxes[3] = PictureBox03;
            PictureBox03.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 3);
            
            _containers[4] = Container10; 
            _pictureBoxes[4] = PictureBox10;
            PictureBox10.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 4);
            
            _containers[5] = Container11; 
            _pictureBoxes[5] = PictureBox11;
            PictureBox11.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 5);
            
            _containers[6] = Container12; 
            _pictureBoxes[6] = PictureBox12;
            PictureBox12.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 6);
            
            _containers[7] = Container13; 
            _pictureBoxes[7] = PictureBox13;
            PictureBox13.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 7);
            
            _containers[8] = Container20; 
            _pictureBoxes[8] = PictureBox20;
            PictureBox20.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 8);
            
            _containers[9] = Container21; 
            _pictureBoxes[9] = PictureBox21;
            PictureBox21.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 9);
            
            _containers[10] = Container22; 
            _pictureBoxes[10] = PictureBox22;
            PictureBox22.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 10);
            
            _containers[11] = Container23; 
            _pictureBoxes[11] = PictureBox23;
            PictureBox23.MouseDoubleClick += (s, e) => PictureBoxClick(s, e, 11);

            // 初始化句柄和别名
            for (int i = 0; i < TOTAL_CELLS; i++)
            {
                _pictureBoxHandles[i] = _pictureBoxes[i].Handle;
                _aliases[i] = $"监控窗口 {i + 1}";
                // 设置ToolTip
                var toolTip = new ToolTip
                {
                    Content = _aliases[i],
                    Style = (Style)Resources["AliasToolTipStyle"]
                };
                _containers[i].ToolTip = toolTip;
            }
        }
        


        /// <summary>
        /// 获取指定索引的PictureBox句柄
        /// </summary>
        public IntPtr GetPictureBoxHandle(int index)
        {
            if (index >= 0 && index < TOTAL_CELLS)
                return _pictureBoxHandles[index];
            return IntPtr.Zero;
        }

        /// <summary>
        /// 获取所有PictureBox句柄数组
        /// </summary>
        public IntPtr[] GetPictureBoxHandles()
        {
            return _pictureBoxHandles.ToArray();
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            // 清理资源逻辑
        }
        
        private void PictureBoxClick(object? sender, MouseEventArgs e, int index)
        {
            if (index >= _devices.Count)
                return;
            Device selectedDevice = _devices[index];
            var fullScreen = new FullScreenWindow(selectedDevice);
            fullScreen.Show();
            Console.WriteLine($"Picturebox {index} double click");
        }
    }
}