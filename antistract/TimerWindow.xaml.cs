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

namespace antistract
{
    /// <summary>
    /// Interaction logic for TimerWindow.xaml
    /// </summary>
    public partial class TimerWindow : Window
    {
        CurrentlySelectedPlan plan;
        public TimerWindow()
        {
            InitializeComponent();
            plan = new CurrentlySelectedPlan();
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
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
            plan.SelectedPlan = "DEMO";
            Debug.WriteLine(plan.SelectedPlan);
        }
    }
}
