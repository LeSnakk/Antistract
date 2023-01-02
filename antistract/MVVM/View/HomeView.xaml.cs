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

        int CurrentWeek;
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
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            CalendarWeekRule weekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
            System.Globalization.Calendar cal = cultureInfo.Calendar;
            int week = cal.GetWeekOfYear(dt, weekRule, firstDay);
            CurrentWeek = week;

            if (Settings.Default.Week == 0)
            {
                Settings.Default.Week = week;
                Settings.Default.Save();
            } 
            else if (Settings.Default.Week != week)
            {
                Settings.Default.LastWeek = Settings.Default.Week;
                Settings.Default.WeeklyLearnTime = 0;
                Settings.Default.Week = week;
                Settings.Default.CheckedForWeekCombo = false;
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
                min15_txt1.Visibility = Visibility.Visible;
                min15_txt2.Visibility = Visibility.Visible;
                min15_txt3.Text = "Can you make it to 1 hour?";
            }
            //1 Hour
            if ((int)Settings.Default["WeeklyLearnTime"] >= 3600)
            {
                h1ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h1.png", UriKind.Relative));
                h1ThisWeek.Opacity = 1;
                h1_txt1.Visibility = Visibility.Visible;
                h1_txt2.Visibility = Visibility.Visible;
            }
            //10 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 36000)
            {
                h10ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h10.png", UriKind.Relative));
                h10ThisWeek.Opacity = 1;
                h10_txt1.Visibility = Visibility.Visible;
                h10_txt2.Visibility = Visibility.Visible;
            }
            //20 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 72000)
            {
                h20ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h20.png", UriKind.Relative));
                h20ThisWeek.Opacity = 1;
                h20_txt1.Visibility = Visibility.Visible;
                h20_txt2.Visibility = Visibility.Visible;
            }
            //30 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 108000)
            {
                h30ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h30.png", UriKind.Relative));
                h30ThisWeek.Opacity = 1;
                h30_txt1.Visibility = Visibility.Visible;
                h30_txt2.Visibility = Visibility.Visible;
            }
            //40 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 144000)
            {
                h40ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h40.png", UriKind.Relative));
                h40ThisWeek.Opacity = 1;
                h40_txt1.Visibility = Visibility.Visible;
                h40_txt2.Visibility = Visibility.Visible;
            }
            //50 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 180000)
            {
                h50ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h50.png", UriKind.Relative));
                h50ThisWeek.Opacity = 1;
                h50_txt1.Visibility = Visibility.Visible;
                h50_txt2.Visibility = Visibility.Visible;
            }
            //100 Hours
            if ((int)Settings.Default["WeeklyLearnTime"] >= 360000)
            {
                h100ThisWeek.Source = new BitmapImage(new Uri(@"/Images/Achievements/h100.png", UriKind.Relative));
                h100ThisWeek.Opacity = 1;
                h100_txt1.Visibility = Visibility.Visible;
                h100_txt2.Visibility = Visibility.Visible;
            }
            //Been productive TODAY?
            if (Settings.Default.TodayDate == DateTime.Now.Date.ToString())
            {
                ProductiveToday.Source = new BitmapImage(new Uri(@"/Images/Achievements/productiveToday.png", UriKind.Relative));
                ProductiveToday.Opacity = 1;
            }
            //No Distractions
            if ((Settings.Default.TodayDate == DateTime.Now.Date.ToString() && Settings.Default.DistractedToday != DateTime.Now.Date.ToString()))
            {
                NoDistraction.Source = new BitmapImage(new Uri(@"/Images/Achievements/noDistraction.png", UriKind.Relative));
                NoDistraction.Opacity = 1;
            }
            //Week Combos
            if (Settings.Default.WeeklyLearnTime != 0 && isFollowingWeek())
            {
                WeekCombo.Source = new BitmapImage(new Uri(@"/Images/Achievements/weekCombo.png", UriKind.Relative));
                int combo = Settings.Default.WeekCombo;
                if (Settings.Default.CheckedForWeekCombo == false)
                {
                    combo++;
                    Settings.Default.WeekCombo = combo;
                    Settings.Default.CheckedForWeekCombo = true;
                    Settings.Default.Save();
                }
                WeekCombo.Opacity = 1;
                WeekComboNumber.Text = combo.ToString();         
                WeekComboToolTip.Text = "You've been productive every week for the past " + combo.ToString() + " weeks. Let's see how far you can push it!";
            } 
            else if (!isFollowingWeek())
            {
                Settings.Default.WeekCombo = 0;
                Settings.Default.Save();
            }
            if (Settings.Default.TotalLearningCycles > 0)
            {
                CompletedCycles.Source = new BitmapImage(new Uri(@"/Images/Achievements/completedLearningCycles.png", UriKind.Relative));
                CompletedCycles.Opacity = 1;
                CompletedCyclesNumber.Text = Settings.Default.TotalLearningCycles.ToString();
                CompletedCyclesToolTip.Text = "Nice! You have successfully completed " + Settings.Default.TotalLearningCycles.ToString() + " learning cycles so far!";
            }

        }

        public bool isFollowingWeek()
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            DateTime dt = DateTime.Now;
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            CalendarWeekRule weekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
            System.Globalization.Calendar cal = cultureInfo.Calendar;
            int week = cal.GetWeekOfYear(dt, weekRule, firstDay);

            DateTime dt2 = dt.AddDays(-7);
            int week2 = cal.GetWeekOfYear(dt2, weekRule, firstDay);
            Debug.WriteLine("Week2: " + week2);

            if (week2 == Settings.Default.LastWeek)
            {
                Debug.WriteLine("TRUE");
                return true;
            }
            else
            {
                Debug.WriteLine("FALSE");
                return false;
            }
        }
    }
}
