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
    {        public PlansView()
        {
            InitializeComponent();
            DisplayPlans();
        }

        public void LoadPlans()
        {
            //GlobalVariables.PlanNames.AddRange(new string[] { "Plan A", "Plan B", "Plan C" });

            Debug.WriteLine("XXX");
            String path = "Plans/paradeplan.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList elements = doc.ChildNodes;
            for (int i = 0; i < elements.Count; i++)
            {
                foreach (XmlNode xmlNode in elements[i].ChildNodes)
                {
                    Debug.WriteLine(xmlNode.Name + ": " + xmlNode.InnerText);
                }
            }
            Debug.WriteLine("XXX");

            List<String> PlanNames = new List<String>();
            PlanNames.Add(doc.GetElementsByTagName("Name")[0].InnerText);
            GlobalVariables.PlanNames.AddRange(PlanNames);
            //GlobalVariables.PlanNames.AddRange(new List<String>() { "Plan A", "Plan B", "Plan C" });
        }

        public void DisplayPlans()
        {
            foreach (String planName in GlobalVariables.PlanNames)
            {
                Debug.WriteLine(planName);
                RadioButton radioButton = new RadioButton() { Content = planName };
                PlanNames.Items.Add(radioButton);
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
