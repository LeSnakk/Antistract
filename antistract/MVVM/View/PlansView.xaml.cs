using antistract.Core;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;

namespace antistract.MVVM.View
{
    public partial class PlansView : UserControl
    {
        private WrapPanel _PlanCreatorWrapPanel;
        public PlansView()
        {
            InitializeComponent();
            
            DisplayPlans();
            _PlanCreatorWrapPanel = PlanCreatorWrapPanel;
        }

        public void LoadPlans()
        {
            //GlobalVariables.PlanNames.AddRange(new string[] { "Plan A", "Plan B", "Plan C" });

            Debug.WriteLine("XXX");
            String path = "Plans/paradeplan_2.xml";
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
            PlanNames.Add(doc.GetElementsByTagName("entryName")[0].InnerText);
            GlobalVariables.PlanNames.AddRange(PlanNames);
            //GlobalVariables.PlanNames.AddRange(new List<String>() { "Plan A", "Plan B", "Plan C" });
        }

        public void DisplayPlans()
        {
            foreach (String planName in GlobalVariables.PlanNames)
            {
                Debug.WriteLine(planName);
                RadioButton radioButton = new RadioButton() { Content = planName };
                PlanOverviewStackPanel.Children.Add((radioButton));
            }
        }

        private void AddPlanButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Adding plan...");
        }

        private void AddElementButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Border PlanCreatorItem in PlanCreatorWrapPanel.Children)
            {
                if (PlanCreatorItem.Visibility == Visibility.Collapsed)
                {
                    PlanCreatorItem.Visibility = Visibility.Visible;
                    return;
                }
            }
            
        }

        private void EntryDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Grid grid = button.Parent as Grid;
            Border border = grid.Parent as Border;
            border.Visibility = Visibility.Collapsed;
        }

        private void SavePlanButton_Click(object sender, RoutedEventArgs e)
        {
            string _entryName = EntryName.Text;

            if (!String.IsNullOrWhiteSpace(_entryName))
            { 
                Debug.WriteLine("PLAN: " + _entryName.ToUpper());

                for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
                {
                    TextBox title = (TextBox)this.FindName("EntryTitle" + i);
                    string _title = title.Text;

                    ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                    string _type = type.Text;

                    TextBox duration = (TextBox)this.FindName("EntryDuration" + i);
                    string _duration = duration.Text;

                    Debug.WriteLine("Title: " + _title + "\nType: " + _type + "\nDuration: " + _duration + "\n");
                }
            }

            /*XDocument xmlDoc = new XDocument(
                new XDeclaration("1.0", Encoding.UTF8.HeaderName, String.Empty),
                new XComment("antistract planDB"),
                new XElement("antistract_plan", 
                    new XElement())*/

        }
    }
}
