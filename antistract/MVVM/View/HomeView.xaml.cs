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

namespace antistract.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        readonly MainWindow mainWndw = (MainWindow)Application.Current.MainWindow;
        public HomeView()
        {
            InitializeComponent();
            UpdateTotalWeekStudyHours();
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            mainWndw.MenuButtonProductivity.IsChecked = true;
        }

        public void UpdateTotalWeekStudyHours()
        {
            if ((int)Settings.Default["WeeklyLearnTime"] >= 3600)
            {
                WeekStudyHours.Text = ((int)Settings.Default["WeeklyLearnTime"] / 3600).ToString();
                TimeUnit.Text = "  hours.";
            }
            else
            {
                WeekStudyHours.Text = ((int)Settings.Default["WeeklyLearnTime"] / 60).ToString();
                TimeUnit.Text = "  minutes.";
            }          
        }
    }
}
