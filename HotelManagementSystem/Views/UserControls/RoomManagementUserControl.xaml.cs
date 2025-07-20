using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.Windows;
using HotelManagementSystem.Models;
using System.Windows;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class RoomManagementUserControl : UserControl
    {
        public RoomManagementUserControl(RoomManagementViewModel? mainVm = null)
        {
            InitializeComponent();
            if (mainVm == null)
                mainVm = new RoomManagementViewModel();
            this.DataContext = mainVm;
        }
    }
} 