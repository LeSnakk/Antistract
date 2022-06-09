using antistract.MVVM.View;
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

        private void InitializePlans()
        {
            PlansView plansView = new PlansView();
            plansView.LoadPlans();
        }
        
    }
}