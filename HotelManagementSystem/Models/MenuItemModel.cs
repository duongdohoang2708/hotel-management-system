using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HotelManagementSystem.Models
{
    public class MenuItemModel
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }
        public ObservableCollection<MenuItemModel> Children { get; set; }
    }
}

