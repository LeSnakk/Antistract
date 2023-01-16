using antistract.Core;
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
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Threading;
using System.Windows.Media;
using antistract.MVVM.View;
using System.Threading;
using antistract.Properties;

namespace antistract
{
    /// <summary>
    /// Interaction logic for TimerWindow.xaml
    /// </summary>
    public partial class TimerWindow : Window
    {
        CurrentlySelectedPlan CurrentlySelectedPlan = new CurrentlySelectedPlan();
        readonly string path = "Plans/paradeplan.xml";
        XmlNode SelectedPlanNodes;
        public DispatcherTimer MainTimer;
        public DispatcherTimer WasteTimer;
        TimeSpan timeLeft;
        TimeSpan timeWasted;
        TimeSpan weeklyLearnTime;
        TimeSpan totalLearnTime;

        DateTime currentTime;
        DateTime closingTime;

        bool ShouldAddToWeeklyLearnTime;

        static public bool TimerOnHold = false;

        string EventTitle;
        string EventDescription;
        string EventDuration;
        int TotalEvents;
        int TotalPlanTime;
        int CurrentEvent = 1;

        public TimerWindow(string currentlySelectedPlan)
        {
            InitializeComponent();
            currentTime = DateTime.Now;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            SelectedPlanLabel.DataContext = CurrentlySelectedPlan;
            CurrentlySelectedPlan.SelectedPlan = currentlySelectedPlan;
            GetPlan(CurrentlySelectedPlan.SelectedPlan);
            TimerOnHold = false;
            InitializeTimer();
        }

        //Retrieve and store selected plan data
        public void GetPlan(string PlanName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList elements = doc.ChildNodes;

            for (int i = 0; i < elements.Count; i++)
            {
                foreach (XmlNode Event in elements[i].ChildNodes)
                {
                    if (Event["entryName"].InnerText == PlanName)
                    {
                        SelectedPlanNodes = Event;
                        TotalEvents = Event.ChildNodes.Count;

                        for (int j = 1; j < Event.ChildNodes.Count; j++)
                        {
                            TotalPlanTime += Int32.Parse(Event.ChildNodes[j]["duration"].InnerText);
                        }
                    }
                }
            }
            CalculateClosingTime(TotalPlanTime);
        }

        //Controls behaviour of the Timer at given CurrentEvent no. Starting the supervision of programs/websites
        private void InitializeTimer()
        {
            if (CurrentEvent < TotalEvents)
            {
                if (SelectedPlanNodes.ChildNodes[CurrentEvent]["type"].InnerText == "Work")
                {
                    Paint("green");
                    ShouldAddToWeeklyLearnTime = true;

                    //Communicates to ProductivityView to start supervision
                    ProductivityView.ShouldCheckYes();
                    ProductivityView.startChecking();                         
                }
                else if (SelectedPlanNodes.ChildNodes[CurrentEvent]["type"].InnerText == "Break")
                {
                    Paint("blue");
                    ShouldAddToWeeklyLearnTime = false;

                    ProductivityView.ShouldCheckNo();
                }

                EntryTitle.Content = SelectedPlanNodes.ChildNodes[CurrentEvent]["title"].InnerText;
                StartTimer(Int32.Parse(SelectedPlanNodes.ChildNodes[CurrentEvent]["duration"].InnerText));
            }
            else
            {
                //After Plan being completed:
                EndTimer();
            }
        }

        //Reset all parameters and variables after plan has been finished
        public void EndTimer()
        {
            ProductivityView.ShouldCheckNo();
            ProductivityView.SetExtensionCheckModePausing();
            TimerOnHoldNO();
            GlobalVariables.TimerRunning = false;
            Settings.Default.StartEnabled = true;
            Settings.Default.BlacklistBlocked = false;
            Settings.Default.TodayDate = DateTime.Now.Date.ToString();
            int cycles = Settings.Default.TotalLearningCycles;
            cycles++;
            Settings.Default.TotalLearningCycles = cycles;
            Settings.Default.Save();
            this.Close();
        }

        //Each event gets an own timer. It is set here
        public void StartTimer(int Minutes)
        {
            MainTimer = new DispatcherTimer();
            WasteTimer = new DispatcherTimer();
            timeLeft = TimeSpan.FromMinutes(Minutes);
            timeWasted = TimeSpan.FromSeconds(timeWasted.TotalSeconds);

            weeklyLearnTime = TimeSpan.FromSeconds((int)Settings.Default["WeeklyLearnTime"]);
            totalLearnTime = TimeSpan.FromSeconds((int)Settings.Default["TotalLearnTime"]);

            Timer.Content = (int)Minutes;
            timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));

            //Set time interval of the timer (ticks every second)
            MainTimer.Tick += dispatcherTimer_Tick;
            MainTimer.Interval = new TimeSpan(0, 0, 1);
            WasteTimer.Interval = new TimeSpan(0, 0, 1);

            MainTimer.Start();
            WasteTimer.Start();
            
            ClosingTime.Content = closingTime.ToString("HH:mm");
        }

        //Every second this code gets executed once
        //Controls behaviour of timer
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (TimerOnHold == false)
            {
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));

                if (ShouldAddToWeeklyLearnTime)
                {
                    Paint("green");
                    weeklyLearnTime = weeklyLearnTime.Add(TimeSpan.FromSeconds(1));
                    totalLearnTime = totalLearnTime.Add(TimeSpan.FromSeconds(1));
                    Settings.Default["WeeklyLearnTime"] = (int)weeklyLearnTime.TotalSeconds;
                    Settings.Default["TotalLearnTime"] = (int)totalLearnTime.TotalSeconds;
                    Settings.Default.Save();
                    Debug.WriteLine("WeekLearnTime: " + Settings.Default["WeeklyLearnTime"]);
                    Debug.WriteLine("TotalLearnTime: " + Settings.Default["TotalLearnTime"]);
                }
            }
            else if (TimerOnHold == true)
            {
                ShowWastedTimeArea();
                Paint("red");
                timeWasted = timeWasted.Add(TimeSpan.FromSeconds(1));   
                closingTime = closingTime.Add(TimeSpan.FromSeconds(1));
                Settings.Default.DistractedToday = DateTime.Now.Date.ToString();
                Settings.Default.Save();
                Debug.WriteLine("Timer has been stopped");
            }
            
            Timer.Content = ((int)timeLeft.TotalMinutes)+1;
            TimerSeconds.Content = timeLeft.TotalSeconds;
            
            TimerWasted.Content = timeWasted.ToString(@"hh\:mm\:ss");
            ClosingTime.Content = closingTime.ToString("HH:mm");

            //If the event has been finished, a new timer gets called for the next event
            if (timeLeft.TotalSeconds <= 0) 
            {
                MainTimer.Stop();
                Timer.Content = ((int)timeLeft.TotalMinutes);
                CurrentEvent++;
                InitializeTimer();
            }
        }

        //Change color of the TimerWindow
        private void Paint(string color)
        {
            Color colour;
            if (color == "green")
            {
                colour = (Color)ColorConverter.ConvertFromString("#BF88D498");
            }
            else if (color == "blue")
            {
                colour = (Color)ColorConverter.ConvertFromString("#BF2CC3CE");
            }
            else if (color == "red")
            {
                colour = (Color)ColorConverter.ConvertFromString("#BFCE2C2C");
            }
            TimerBorder.Background = new SolidColorBrush(colour);
        }

        private void CalculateClosingTime(int TotalMinutes)
        {
            closingTime = currentTime.AddMinutes(TotalMinutes);
        }

        //Make window movable
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Application.Current.Windows[1].DragMove(); //Only wirks in build
            }
        }

        private void ShowWastedTimeArea()
        {
            TimerWastedTabTextBefore.Visibility = Visibility.Visible;
            TimerWasted.Visibility = Visibility.Visible;
            TimerWastedTabTextAfter.Visibility = Visibility.Visible;
        }
        private void HideWastedTimeArea()
        {
            TimerWastedTabTextBefore.Visibility = Visibility.Hidden;
            TimerWasted.Visibility = Visibility.Hidden;
            TimerWastedTabTextAfter.Visibility = Visibility.Hidden;
        }

        //Gets called from ProductivityView to change the behaviour of the timer
        public static void TimerOnHoldYES()
        {
            TimerOnHold = true;
        }
        public static void TimerOnHoldNO()
        {
            TimerOnHold = false;
        }
    }
}
