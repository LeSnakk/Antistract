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


namespace antistract
{
    /// <summary>
    /// Interaction logic for TimerWindow.xaml
    /// </summary>
    public partial class TimerWindow : Window
    {
        CurrentlySelectedPlan CurrentlySelectedPlan = new CurrentlySelectedPlan();
        readonly string path = "Plans/paradeplan_2.xml";
        XmlNode SelectedPlanNodes;
        DispatcherTimer MainTimer;
        TimeSpan timeLeft;


        string EventTitle;
        string EventDescription;
        string EventDuration;
        int TotalEvents;
        int CurrentEvent = 1;

        public TimerWindow(string currentlySelectedPlan)
        {
            InitializeComponent();
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            SelectedPlanLabel.DataContext = CurrentlySelectedPlan;
            CurrentlySelectedPlan.SelectedPlan = currentlySelectedPlan;
            GetPlan(CurrentlySelectedPlan.SelectedPlan);
            InitializeTimer();
        }

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
                        Debug.WriteLine("SELECTED PLAN:\n" + Event["entryName"].InnerText);
                        for (int j = 1; j < Event.ChildNodes.Count; j++)
                        {
                            Debug.WriteLine(Event.ChildNodes[j]["title"].InnerText);
                            Debug.WriteLine(Event.ChildNodes[j]["type"].InnerText);
                            Debug.WriteLine(Event.ChildNodes[j]["duration"].InnerText);
                        }
                    }
                }
            }
        }

        private void InitializeTimer()
        {
            if (CurrentEvent < TotalEvents)
            {
                if (SelectedPlanNodes.ChildNodes[CurrentEvent]["type"].InnerText == "Work")
                {
                    Paint("green");
                }
                else if (SelectedPlanNodes.ChildNodes[CurrentEvent]["type"].InnerText == "Break")
                {
                    Paint("blue");
                }
                EntryTitle.Content = SelectedPlanNodes.ChildNodes[CurrentEvent]["title"].InnerText;
                StartTimer(Int32.Parse(SelectedPlanNodes.ChildNodes[CurrentEvent]["duration"].InnerText));
            }
        }

        private void StartTimer(int Minutes)
        {
            MainTimer = new DispatcherTimer();           
            timeLeft = TimeSpan.FromMinutes(Minutes);

            MainTimer.Tick += dispatcherTimer_Tick;
            MainTimer.Interval = new TimeSpan(0, 0, 1);


            MainTimer.Start();
            Timer.Content = timeLeft.TotalSeconds;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
            Timer.Content = ((int)timeLeft.TotalMinutes)+1;
            TimerSeconds.Content = timeLeft.TotalSeconds;

            if (timeLeft.TotalSeconds <= 0) 
            {
                MainTimer.Stop();
                Timer.Content = ((int)timeLeft.TotalMinutes);
                CurrentEvent++;
                InitializeTimer();
            }
        }

        //Colors:
        //Green: #BF88D498
        //Blue: #BF2CC3CE
        //Red: #BFCE2C2C

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

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Application.Current.Windows[1].DragMove(); //Only wirks in build
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
