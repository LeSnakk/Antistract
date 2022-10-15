using antistract.MVVM.View;
using antistract.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace antistract
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializePlans();
            LoadUsername();
            Settings.Default.BlacklistBlocked = false;
            Settings.Default.Save();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
        if (e.ChangedButton == MouseButton.Left)
        {
            Application.Current.MainWindow.DragMove();
        }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("closing...");
            ProductivityView.SetExtensionCheckModePausing();
            Application.Current.Shutdown();
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            this.MenuButtonProductivity.IsChecked = true;
        }

        private void MenuButtonOverview_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RemoveGetStartedButton(object sender, RoutedEventArgs e)
        {
        }

        private void InitializePlans()
        {
            PlansView plansView = new PlansView();
            plansView.LoadPlans();
        }

        private void LoadUsername()
        {
            if (Settings.Default["Username"].ToString() == "")
            {
                Settings.Default["Username"] = "New User";
                Settings.Default.Save();
            }
            UsernameDisplay.Text = Settings.Default["Username"].ToString();
            UsernameDisplay.Visibility = Visibility.Visible;
            
            UsernameDisplayTextBox.Text = Settings.Default["Username"].ToString();
            UsernameDisplayTextBox.Visibility = Visibility.Hidden;
            SaveUsername.Visibility = Visibility.Hidden;
        }

        private void UsernameDisplay_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UsernameDisplay.Visibility = Visibility.Hidden;
            UsernameDisplayTextBox.Visibility = Visibility.Visible;
            SaveUsername.Visibility=Visibility.Visible;
        }

        private void SaveUsername_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default["Username"] = UsernameDisplayTextBox.Text;
            Settings.Default.Save();
            LoadUsername();
        }
    }
}