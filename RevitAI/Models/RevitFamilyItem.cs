using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RevitAI.Models
{
    public class RevitFamily : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<RevitFamilyType> Types { get; set; } = new ObservableCollection<RevitFamilyType>();
    }

    public class RevitFamilyType : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
