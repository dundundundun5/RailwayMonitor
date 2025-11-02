using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RailwayAlarmBackend.Sdks;
using Color = System.Windows.Media.Color;
using ToolTip = System.Windows.Controls.ToolTip;
using WinForms = System.Windows.Forms;
using WinInput = System.Windows.Input;

namespace RailwayMonitorClient.Views
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

        // 容器数组
        private Border[] _containers;

        // PictureBox数组
        private WinForms.PictureBox[] _pictureBoxes;


        public Camera Camera { get; set; }

        public LiveView()
        {
            InitializeComponent();
            InitializeArrays();
            SetupEventHandlers();
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
            _containers[0] = Container00; _pictureBoxes[0] = PictureBox00;
            _containers[1] = Container01; _pictureBoxes[1] = PictureBox01;
            _containers[2] = Container02; _pictureBoxes[2] = PictureBox02;
            _containers[3] = Container03; _pictureBoxes[3] = PictureBox03;
            _containers[4] = Container10; _pictureBoxes[4] = PictureBox10;
            _containers[5] = Container11; _pictureBoxes[5] = PictureBox11;
            _containers[6] = Container12; _pictureBoxes[6] = PictureBox12;
            _containers[7] = Container13; _pictureBoxes[7] = PictureBox13;
            _containers[8] = Container20; _pictureBoxes[8] = PictureBox20;
            _containers[9] = Container21; _pictureBoxes[9] = PictureBox21;
            _containers[10] = Container22; _pictureBoxes[10] = PictureBox22;
            _containers[11] = Container23; _pictureBoxes[11] = PictureBox23;

            // 初始化句柄和别名
            for (int i = 0; i < TOTAL_CELLS; i++)
            {
                _pictureBoxHandles[i] = _pictureBoxes[i].Handle;
                _aliases[i] = $"摄像头 {i + 1}";

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
        /// 设置事件处理器
        /// </summary>
        private void SetupEventHandlers()
        {
            for (int i = 0; i < TOTAL_CELLS; i++)
            {
                int index = i; // 捕获当前索引
                _containers[i].MouseEnter += (s, e) => OnContainerMouseEnter(s, e, index);
                _containers[i].MouseLeave += OnContainerMouseLeave;
            }
        }


        /// <summary>
        /// 容器鼠标进入事件
        /// </summary>
        private void OnContainerMouseEnter(object sender, WinInput.MouseEventArgs e, int index)
        {
            var container = sender as Border;
            if (container != null)
            {
                container.Background = new SolidColorBrush(Color.FromRgb(74, 85, 104));
            }
        }

        /// <summary>
        /// 容器鼠标离开事件
        /// </summary>
        private void OnContainerMouseLeave(object sender, WinInput.MouseEventArgs e)
        {
            var container = sender as Border;
            if (container != null )
            {
                container.Background = new SolidColorBrush(Color.FromRgb(26, 32, 44));
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
        /// 设置指定索引的别名
        /// </summary>
        public void SetAlias(int index, string alias)
        {
            if (index >= 0 && index < TOTAL_CELLS)
            {
                _aliases[index] = alias;

                // 更新ToolTip
                var container = _containers[index];
                var toolTip = container.ToolTip as ToolTip;
                if (toolTip != null)
                {
                    toolTip.Content = alias;
                }
            }
        }

        /// <summary>
        /// 获取指定索引的别名
        /// </summary>
        public string GetAlias(int index)
        {
            if (index >= 0 && index < TOTAL_CELLS)
                return _aliases[index];
            return string.Empty;
        }

        /// <summary>
        /// 获取所有别名数组
        /// </summary>
        public string[] GetAliases()
        {
            return _aliases.ToArray();
        }

        /// <summary>
        /// 设置所有别名
        /// </summary>
        public void SetAliases(string[] aliases)
        {
            if (aliases.Length == TOTAL_CELLS)
            {
                for (int i = 0; i < TOTAL_CELLS; i++)
                {
                    SetAlias(i, aliases[i]);
                }
            }
        }

        /// <summary>
        /// 获取视频面板的句柄（兼容旧版本）
        /// </summary>
        public IntPtr GetVideoHandle()
        {
            return PictureBox00.Handle;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            // 清理资源逻辑
        }
    }
}