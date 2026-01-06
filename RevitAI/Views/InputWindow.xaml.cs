using System.Windows;

namespace RevitAI.Views
{
    public partial class InputWindow : Window
    {
        public string PromptText { get; private set; } = string.Empty;
        public bool IsConfirmed { get; private set; } = false;

        public InputWindow()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            PromptText = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                MessageBox.Show("请输入有效的指令。", "提示");
                return;
            }

            IsConfirmed = true;
            StatusText.Text = "正在处理...";
            GenerateButton.IsEnabled = false;
            
            // 关闭窗口以返回控制权给外部命令
            this.Close();
        }
    }
}
