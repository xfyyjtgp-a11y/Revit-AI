using RevitAI.ViewModels;
using System.Windows;

namespace RevitAI.Views
{
    /// <summary>
    /// RevitFamilyView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitFamilyView : Window
    {
        public RevitFamilyView()
        {
            InitializeComponent();
            DataContext = new RevitFamilyViewModel();
        }
    }
}
