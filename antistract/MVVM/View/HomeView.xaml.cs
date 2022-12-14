using antistract.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
            ManageAchievements();
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            mainWndw.MenuButtonProductivity.IsChecked = true;
        }

        public void UpdateTotalWeekStudyHours()
        {
            GetCurrentWeek();

            if ((int)Settings.Default["WeeklyLearnTime"] >= 3600)
            {
                WeekStudyHours.Text = ((int)Settings.Default["WeeklyLearnTime"] / 3600).ToString();
                TimeUnit.Text = "  hours.";
            }
            else if ((int)Settings.Default["WeeklyLearnTime"] < 60)
            {
                WeekStudyHours.Text = ((int)Settings.Default["WeeklyLearnTime"] / 60).ToString();
                RewardText.Text = "Are your ready?";
            }
            else if ((int)Settings.Default["WeeklyLearnTime"] < 120)
            {
                WeekStudyHours.Text = ((int)Settings.Default["WeeklyLearnTime"] / 60).ToString();
                TimeUnit.Text = "  minute.";
            }
            else
            {
                WeekStudyHours.Text = ((int)Settings.Default["WeeklyLearnTime"] / 60).ToString();
                TimeUnit.Text = "  minutes.";
            }          
        }

        public void GetCurrentWeek()
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;

            DateTime dt = DateTime.Now;
            DateTime dt2 = DateTime.Parse("02.01.2023 18:59:08");
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            CalendarWeekRule weekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
            System.Globalization.Calendar cal = cultureInfo.Calendar;
            int week = cal.GetWeekOfYear(dt, weekRule, firstDay);
            Debug.WriteLine(cal.GetWeekOfYear(dt2, weekRule, firstDay));

            if (Settings.Default.Week == 0)
            {
                Settings.Default.Week = week;
                Settings.Default.Save();
            } 
            else if (Settings.Default.Week != week)
            {
                Settings.Default.WeeklyLearnTime = 0;
                Settings.Default.Week = week;
                Settings.Default.Save();
            }
        }

        public void ManageAchievements()
        {
            //15 Minutes
            if ((int) Settings.Default["WeeklyLearnTime"] >= 900)
            {
                min15ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/min15.png", UriKind.Relative));
                min15ThisWeek.Opacity = 1;
            }
            //1 Hour
            if ((int)Settings.Default["WeeklyLearnTime"] >= 3600)
            {
                h1ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h1.png", UriKind.Relative));
                h1ThisWeek.Opacity = 1;
            }
            //10 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 36000)
            {
                h10ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h10.png", UriKind.Relative));
                h10ThisWeek.Opacity = 1;
            }
            //20 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 72000)
            {
                h20ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h20.png", UriKind.Relative));
                h20ThisWeek.Opacity = 1;
            }
            //30 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 108000)
            {
                h30ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h30.png", UriKind.Relative));
                h30ThisWeek.Opacity = 1;
            }
            //40 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 144000)
            {
                h40ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h40.png", UriKind.Relative));
                h40ThisWeek.Opacity = 1;
            }
            //50 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 180000)
            {
                h50ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h50.png", UriKind.Relative));
                h50ThisWeek.Opacity = 1;
            }
            //100 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 360000)
            {
                h100ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h100.png", UriKind.Relative));
                h100ThisWeek.Opacity = 1;
            }
            //Been productive TODAY?
            if (Settings.Default.TodayDate == DateTime.Now.Date.ToString())
            {
                ProductiveToday.Source = new BitmapImage(new Uri(@"/Images/Achievements/productiveToday.png", UriKind.Relative));
                ProductiveToday.Opacity = 1;
            }

        }
    }
}
