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
        private string _currentlySelectedPlan = "";
        private bool _isEdited = false;

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

        public bool isEdited()
        {
            return _isEdited;
        }
        public void isEdited(bool isEdited)
        {
            _isEdited = isEdited;
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

        public void ShowSelectedPlan(string PlanName)
        {
            ResetPlanCreatorItems();
            ToggleAddButton(false);

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
                            TextBox title = (TextBox)this.FindName("EntryTitle" + (j-1));
                            title.Text = Event.ChildNodes[j]["title"].InnerText;

                            ComboBox type = (ComboBox)this.FindName("EntryType" + (j-1));
                            type.Text = Event.ChildNodes[j]["type"].InnerText;

                            TextBox duration = (TextBox)this.FindName("EntryDuration" + (j-1));
                            duration.Text = Event.ChildNodes[j]["duration"].InnerText;

                            if (PlanCreatorWrapPanel.Children[j-1].Visibility == Visibility.Collapsed)
                            {
                                PlanCreatorWrapPanel.Children[j-1].Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }

        private void SavePlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEdited())
            {
                XmlDocument doc1 = new XmlDocument();
                doc1.Load(path);
                XmlNodeList elements = doc1.ChildNodes;

                for (int i = 0; i < elements.Count; i++)
                {
                    foreach (XmlNode Event in elements[i].ChildNodes)
                    {
                        if (Event["entryName"].InnerText == GetCurrentlySelectedPlan())
                        {
                            for (int j = 1; j < Event.ChildNodes.Count; j++)
                            {
                                TextBox title = (TextBox)this.FindName("EntryTitle" + (j - 1));
                                Event.ChildNodes[j]["title"].InnerText = title.Text;

                                ComboBox type = (ComboBox)this.FindName("EntryType" + (j - 1));
                                Event.ChildNodes[j]["type"].InnerText = type.Text;

                                TextBox duration = (TextBox)this.FindName("EntryDuration" + (j - 1));
                                Event.ChildNodes[j]["duration"].InnerText = duration.Text;

                            }
                        }
                    }
                }
                doc1.Save(path);

            }
            else
            {

                string _entryName = EntryName.Text;

                XDocument doc = XDocument.Load(path);
                XElement root = new XElement("entry", new XAttribute("PlanName", _entryName));

                root.Add(new XElement("entryName", _entryName));

                if (!String.IsNullOrWhiteSpace(_entryName))
                {
                    for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
                    {
                        TextBox title = (TextBox)this.FindName("EntryTitle" + i);
                        string _title = title.Text;

                        ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                        string _type = type.Text;

                        TextBox duration = (TextBox)this.FindName("EntryDuration" + i);
                        string _duration = duration.Text;

                        WriteToXMLFile(doc, root, _title, _type, _duration);
                    }
                }
                if (root.Elements().Count<XElement>() > 1)
                {
                    doc.Element("antistract_plan").Add(root);
                    doc.Save(path);
                }
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

        public void ResetPlanCreatorItems()
        {
            for (int i = 1; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
            {
                TextBox title = (TextBox)this.FindName("EntryTitle" + (i));
                title.Clear();

                ComboBox type = (ComboBox)this.FindName("EntryType" + (i));
                type.SelectedIndex = -1;

                TextBox duration = (TextBox)this.FindName("EntryDuration" + (i));
                duration.Clear();
            }
            for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
            {
                TogglePlanCreatorItem(i, false);
                PlanCreatorWrapPanel.Children[i].Visibility = Visibility.Collapsed;
            }
        }

        public void ToggleAddButton(bool visibility)
        {
            if (visibility)
            {
                AddElementButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddElementButton.Visibility = Visibility.Hidden;
            }
            
        }

        public void TogglePlanCreatorItem(int item, bool toggle)
        {
            TextBox title = (TextBox)this.FindName("EntryTitle" + (item));
            title.IsEnabled = toggle;

            ComboBox type = (ComboBox)this.FindName("EntryType" + (item));
            type.IsEnabled = toggle;

            TextBox duration = (TextBox)this.FindName("EntryDuration" + (item));
            duration.IsEnabled = toggle;
        }

        private void AddPlanButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleAddButton(true);
        }

        private void AddElementButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Object PlanCreatorItem in PlanCreatorWrapPanel.Children)
            {
                if (PlanCreatorItem is Border)
                {
                    Border temp = PlanCreatorItem as Border;
                    if (temp.Visibility == Visibility.Collapsed)
                    {
                        temp.Visibility = Visibility.Visible;
                        return;
                    }
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

        private void PlanEntryNameList_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = (RadioButton)sender;
            GetPlan(radiobutton.Content.ToString());
            SetCurrentlySelectedPlan(radiobutton.Content.ToString());
            ShowSelectedPlan(GetCurrentlySelectedPlan());
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            XDocument doc = XDocument.Load(path);
            doc.Descendants("entry").Where(p => p.Attribute("PlanName").Value == GetCurrentlySelectedPlan()).FirstOrDefault().Remove();
            doc.Save(path);

            LoadPlans();
            DisplayPlans();
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
            {
                TogglePlanCreatorItem(i, true);
            }
            ToggleAddButton(true);
            isEdited(true);
        } 
    }
}
