using System.Windows;
using Autodesk.Revit.UI;
using RevitAI.ViewModels;

namespace RevitAI.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ExternalEvent externalEvent, AIRequestHandler requestHandler, string apiKey)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(externalEvent, requestHandler, apiKey);
        }
    }
}
