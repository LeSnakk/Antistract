using antistract.Core;
using Caliburn.Micro;
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
using System.Xml;

namespace antistract.MVVM.View
{
    /// <summary>
    /// Interaction logic for PlansView.xaml
    /// </summary>
    public partial class PlansView : UserControl
    {
        public BindableCollection<GlobalVariables> PlanNames { get; set; }
        public PlansView()
        {
            InitializeComponent();
            DisplayPlans();
        }

        public void LoadPlans()
        {
            GlobalVariables.PlanNames.AddRange(new string[] { "Plan A", "Plan B", "Plan C" });

            XmlDocument doc = new XmlDocument();

        }

        public void DisplayPlans()
        {
            
            foreach (String planName in GlobalVariables.PlanNames)
            {
                Debug.WriteLine(planName);
                RadioButton radioButton = new RadioButton() { Content = planName };
                PlanOverviewStackPanel.Children.Add(radioButton);
            }
        }

        private void AddPlanButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Adding plan...");
        }

        private void AddPlanElementButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
