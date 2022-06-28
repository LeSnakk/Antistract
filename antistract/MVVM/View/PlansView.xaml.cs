using antistract.Core;
using Caliburn.Micro;
using System;
using System.Collections;
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
        readonly string path = "Plans/paradeplan_2.xml";
        public List<Plans> Plans = new List<Plans>();
        private string _currentlySelectedPlan;

        public PlansView()
        {
            InitializeComponent();
            
            DisplayPlans();
            _PlanCreatorWrapPanel = PlanCreatorWrapPanel;
        }

        public string GetCurrentlySelectedPlan()
        {
            return _currentlySelectedPlan;
        }
        public void SetCurrentlySelectedPlan(string planName)
        {
            _currentlySelectedPlan = planName;
        }

        public void LoadPlans()
        {
            GlobalVariables.PlanNames.Clear();

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList elements = doc.ChildNodes;

            for (int i = 0; i < elements.Count; i++)
            {
                foreach (XmlNode Event in elements[i].ChildNodes)
                {
                    Debug.WriteLine(Event["entryName"].InnerText);
                    for (int j = 1; j < Event.ChildNodes.Count; j++)
                    {
                        Debug.WriteLine(Event.ChildNodes[j]["title"].InnerText);
                        Debug.WriteLine(Event.ChildNodes[j]["type"].InnerText);
                        Debug.WriteLine(Event.ChildNodes[j]["duration"].InnerText);
                    }
                    GlobalVariables.PlanNames.Add(Event["entryName"].InnerText);
                }
            }
            Debug.WriteLine("XXX");
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

        public void GVPlanNamesToOCPlanNames()
        {
            Plans plans = new Plans();

            for (int i = 0; i < GlobalVariables.PlanNames.Count; i++)
            {
                EntryNames entryNames = new EntryNames();
                entryNames.entryName = GlobalVariables.PlanNames[i];
                plans.EntryNames.Add(entryNames);
            }
            PlanOverviewStackPanel.DataContext = plans.EntryNames;
        }

        public void DisplayPlans()
        {
            GVPlanNamesToOCPlanNames();
            /*Plans plans = new Plans();

            for (int i = 0; i < 4; i++)
            {
                Entries entries = new Entries();
                entries.entryName = //Weise ich hier den Plans zu oder ziehe ich aus den Plans? Muss der Verweis nicht beim
            }                             //Einlesen der XML stattfinden? Warte nein, die ObservableCollection existiert ja noch nicht
                                            //Muss aus der GlobalVariables die PlanNames nehmen. Brauche in der Plans Class ja dann
                                            //Nur die entryName Variable, oder? GlobalVatiables.PlanNames -> ObservableCollection -> StackPanel
            foreach (String planName in GlobalVariables.PlanNames)
            {
                Debug.WriteLine(planName);
                RadioButton radioButton = new RadioButton() { Content = planName };
                PlanOverviewStackPanel.Children.Add((radioButton));
            }*/
        }

        private void AddPlanButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Adding plan....");
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

            XDocument doc = XDocument.Load(path);
            XElement root = new XElement("entry", new XAttribute("PlanName", _entryName));

            root.Add(new XElement("entryName", _entryName));

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

                    WriteToXMLFile(doc, root, _title, _type, _duration);
                }
            }
            if (root.Elements().Count<XElement>() > 1)
            {
                doc.Element("antistract_plan").Add(root);
                doc.Save(path);
            }
            LoadPlans();
            DisplayPlans();
        }

        public void WriteToXMLFile(XDocument doc, XElement root, string _title, string _type, string _duration)
        {
            if (!string.IsNullOrEmpty(_title))
            {
                root.Add(new XElement("event",
                    new XElement("title", _title),
                    new XElement("type", _type),
                    new XElement("duration", _duration)));
            }
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            XDocument doc = XDocument.Load(path);

            doc.Descendants("entry").Where(p => p.Attribute("PlanName").Value == GetCurrentlySelectedPlan()).FirstOrDefault().Remove();

            /*XElement selectedElement = doc.Descendants()
            .Where(x => (string)x.Attribute("PlanName") == GetCurrentlySelectedPlan()).SingleOrDefault();
            selectedElement.RemoveAll();*/
            doc.Save(path);

            LoadPlans();
            DisplayPlans();
        }

        private void PlanEntryNameList_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = (RadioButton)sender;
            GetPlan(radiobutton.Content.ToString());
            SetCurrentlySelectedPlan(radiobutton.Content.ToString());
        }
    }
}
