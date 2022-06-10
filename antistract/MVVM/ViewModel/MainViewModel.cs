using antistract.Core;
using antistract.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antistract.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        //public static MainViewModel Instance { get; } = new MainViewModel();

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand ProductivityViewCommand { get; set; }
        public RelayCommand PlansViewCommand { get; set; }
        public RelayCommand ToDoViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public ProductivityViewModel ProductivityVM { get; set; }
        public PlansViewModel PlansVM { get; set; }
        public ToDoViewModel ToDoVM { get; set; }
        public SettingsViewModel SettingsVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            ProductivityVM = new ProductivityViewModel();
            PlansVM = new PlansViewModel();
            ToDoVM = new ToDoViewModel();
            SettingsVM = new SettingsViewModel();

            CurrentView = PlansVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            ProductivityViewCommand = new RelayCommand(o =>
            {
                CurrentView = ProductivityVM;
            });

            PlansViewCommand = new RelayCommand(o =>
            {
                CurrentView = PlansVM;
            });

            ToDoViewCommand = new RelayCommand(o =>
            {
                CurrentView = ToDoVM;
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVM;
            });
        }
    }
}
