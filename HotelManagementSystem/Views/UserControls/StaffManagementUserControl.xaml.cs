using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.Windows;
using HotelManagementSystem.Models;
using System.Windows;
using System.Linq;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class StaffManagementUserControl : UserControl
    {
        public StaffManagementUserControl()
        {
            InitializeComponent();
            var vm = new StaffManagementViewModel();
            this.DataContext = vm;
            if (vm is StaffManagementViewModel mainVm)
            {
                //mainVm.AddStaffRequested += () => OpenAddStaffWindow(mainVm);
                //mainVm.EditStaffRequested += (staff) => OpenEditStaffWindow(mainVm, staff);
            }
        }

        private void OpenAddStaffWindow(StaffManagementViewModel mainVm)
        {
            var vm = new StaffAddEditViewModel(false);
            var win = new StaffAddEditWindow(vm);
            vm.StaffSaved += _ => mainVm.LoadStaff();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void OpenEditStaffWindow(StaffManagementViewModel mainVm, StaffManagementViewModel.StaffDisplay staff)
        {
            Staff dbStaff;
            using (var db = new HotelManagementDbContext())
            {
                dbStaff = db.Staff.FirstOrDefault(s => s.StaffId == staff.StaffId);
            }
            if (dbStaff == null) return;
            var vm = new StaffAddEditViewModel(true, dbStaff);
            var win = new StaffAddEditWindow(vm);
            vm.StaffSaved += _ => mainVm.LoadStaff();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }
    }
} 