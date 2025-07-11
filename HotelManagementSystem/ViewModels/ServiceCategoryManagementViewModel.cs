using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceCategoryManagementViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceCategory> CategoryList { get; set; } = new();
        public ObservableCollection<ServiceCategory> FilteredCategoryList { get; set; } = new();

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterCategories(); }
        }

        public ICommand EditCommand { get; }
        public ICommand AddCommand { get; }

        public ServiceCategoryManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                CategoryList = new ObservableCollection<ServiceCategory>(db.ServiceCategories.ToList());
            }
            FilteredCategoryList = new ObservableCollection<ServiceCategory>(CategoryList);
            EditCommand = new RelayCommand(c => EditCategory(c as ServiceCategory));
            AddCommand = new RelayCommand(_ => AddCategory());
        }

        private void FilterCategories()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = CategoryList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    (c.CategoryName != null && c.CategoryName.ToLower().Contains(keyword))
                );
            }

            FilteredCategoryList.Clear();
            foreach (var c in query) FilteredCategoryList.Add(c);
        }

        private void EditCategory(ServiceCategory? category)
        {
            if (category == null) return;
            var editWindow = new ServiceCategoryEditWindow();
            var vm = new ServiceCategoryEditViewModel(category);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.CategorySaved += updatedCategory =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.ServiceCategories.FirstOrDefault(c => c.CategoryId == updatedCategory.CategoryId);
                    if (refreshed != null)
                    {
                        var idx = CategoryList.IndexOf(category);
                        if (idx >= 0) CategoryList[idx] = refreshed;
                        FilterCategories();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void AddCategory()
        {
            var addWindow = new ServiceCategoryEditWindow();
            var vm = new ServiceCategoryEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.CategorySaved += newCategory =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.ServiceCategories.FirstOrDefault(c => c.CategoryId == newCategory.CategoryId);
                    if (refreshed != null)
                    {
                        CategoryList.Add(refreshed);
                        FilterCategories();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 