using antistract.Core;
using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using antistract.Properties;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;

namespace antistract.MVVM.View
{
    public partial class PlansView : UserControl
    {
        private WrapPanel _PlanCreatorWrapPanel;
        readonly string path = "Plans/paradeplan_2.xml";
        public List<Plans> Plans = new List<Plans>();
        private string _currentlySelectedPlan = "";
        private bool _isEdited = false;
        private bool saveEnabled = false;

        CurrentlySelectedPlan CurrentlySelectedPlan = new CurrentlySelectedPlan();

        public PlansView()
        {
            InitializeComponent();

            DisplayPlans();
            _PlanCreatorWrapPanel = PlanCreatorWrapPanel;
        }

        public bool isEdited()
        {
            return _isEdited;
        }
        public void isEdited(bool isEdited)
        {
            _isEdited = isEdited;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
            ToggleRemoveButton(false);

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
                            TextBox title = (TextBox)this.FindName("EntryTitle" + (j - 1));
                            title.Text = Event.ChildNodes[j]["title"].InnerText;

                            ComboBox type = (ComboBox)this.FindName("EntryType" + (j - 1));
                            type.Text = Event.ChildNodes[j]["type"].InnerText;

                            TextBox duration = (TextBox)this.FindName("EntryDuration" + (j - 1));
                            duration.Text = Event.ChildNodes[j]["duration"].InnerText;

                            if (PlanCreatorWrapPanel.Children[j - 1].Visibility == Visibility.Collapsed)
                            {
                                PlanCreatorWrapPanel.Children[j - 1].Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            DisableInvalidInputText();
        }

        private void SavePlanButton_Click(object sender, RoutedEventArgs e)
        {
            saveEnabled = false;
            if ((!GlobalVariables.PlanNames.Contains(EntryName.Text) || isEdited()) && EntryName.Text != "")
            {
                Debug.WriteLine("WENT!!!");
                SavePlanButton_Click();
                if (saveEnabled)
                {
                    //check for bool if is valid or so
                    /*for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
                    {
                        //TogglePlanCreatorItem(i, false);
                        
                    }*/
                    
                    ToggleAddButton(false);
                    ToggleRemoveButton(false);

                    if (isEdited())
                    {
                        TickPlan(CurrentlySelectedPlan.SelectedPlan);
                    }
                    else
                    {
                        TickPlan(EntryName.Text);
                    }
                    CurrentlySelectedPlan.SelectedPlan = EntryName.Text;
                    isEdited(false);
                    EntryName.Clear();
                    EntryName.Text = CurrentlySelectedPlan.SelectedPlan;
                    EntryName.IsEnabled = false;
                    SavePlan.IsEnabled = false;
                    ShowSelectedPlan(CurrentlySelectedPlan.SelectedPlan);
                }
            } else if (String.IsNullOrWhiteSpace(EntryName.Text))
            {
                InfoField.Visibility = Visibility.Visible;
                InfoField.Content = "Please fill in a plan name.";
                
            }
        }

        private void SavePlanButton_Click() {
            if (isEdited())
            {
                if (EntryName.Text != CurrentlySelectedPlan.SelectedPlan && !GlobalVariables.PlanNames.Contains(EntryName.Text))
                {
                    isEdited(false);
                    SavePlanButton_Click();
                }

                Debug.WriteLine("\n\nWENT INTO EDIT MODE\n\n");

                XmlDocument doc1 = new XmlDocument();
                doc1.Load(path);
                XmlNodeList elements = doc1.ChildNodes;

                for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 2; i++)
                {
                    TextBox title = (TextBox)this.FindName("EntryTitle" + i);
                    ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                    TextBox duration = (TextBox)this.FindName("EntryDuration" + i);

                    if (String.IsNullOrEmpty(title.Text) && String.IsNullOrEmpty(type.Text) && String.IsNullOrEmpty(duration.Text))
                    {
                        //I GUESS NO – maybe fill variables with empty content?
                        continue;
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(title.Text))
                        {
                            Debug.WriteLine("Invalid input TEXT0 " + i);
                            InvalidInput("text");
                            saveEnabled = false;
                            return;
                        }
                        else
                        {
                            Debug.WriteLine(i + " " + title.Text + "totallength= " + PlanCreatorWrapPanel.Children.Count);
                            saveEnabled = true;
                        }

                        if (String.IsNullOrWhiteSpace(type.Text))
                        {
                            InvalidInput("type");
                            saveEnabled = false;
                            return;
                        }
                        else
                        {
                            saveEnabled = true;
                        }

                        if (String.IsNullOrWhiteSpace(duration.Text))
                        {
                            InvalidInput("duration");
                            saveEnabled = false;
                            return;
                        }
                        else
                        {
                            saveEnabled = true;
                        }
                    }
                }

                for (int l = 0; l < elements.Count; l++)    //All elements in XML
                {
                    foreach (XmlNode Event in elements[l].ChildNodes)
                    {
                        if (Event["entryName"].InnerText == CurrentlySelectedPlan.SelectedPlan)     //Find right entry by Plan Name
                        {
                            Debug.WriteLine("\nEvent before delete: \n" + Event.ChildNodes.Count + "\n");
                            int i = 1;
                            while (i < Event.ChildNodes.Count)
                            {
                                Event.RemoveChild(Event.ChildNodes[i]);
                            }
                            Debug.WriteLine("\nEvent after delete: \n" + Event.ChildNodes.Count + "\n");
                        }
                    }
                }
                Save(null, doc1, path);

                //doc1.Save(path);

                XDocument doc = XDocument.Load(path);
                XElement root = doc.Descendants("entry").FirstOrDefault(p => p.Attribute("PlanName").Value == CurrentlySelectedPlan.SelectedPlan);
                Debug.WriteLine("\nROOT BEFORE REPLACEMENT\n" + root);

                for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
                {
                    TextBox title = (TextBox)this.FindName("EntryTitle" + i);
                    string _title;
                    ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                    string _type;
                    TextBox duration = (TextBox)this.FindName("EntryDuration" + i);
                    string _duration;

                    if (String.IsNullOrEmpty(title.Text) && String.IsNullOrEmpty(type.Text) && String.IsNullOrEmpty(duration.Text))
                    {
                        //I GUESS NO – maybe fill variables with empty content?
                        continue;
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(title.Text))
                        {
                            Debug.WriteLine("Invalid input TEXT1");
                            InvalidInput("text");
                            saveEnabled = false;
                            return;
                        } else
                        {
                            _title = title.Text;
                            saveEnabled = true;
                        }

                        if (String.IsNullOrWhiteSpace(type.Text))
                        {
                            InvalidInput("type");
                            saveEnabled = false;
                            return;
                        } else
                        {
                            _type = type.Text;
                            saveEnabled = true;
                        }

                        if (String.IsNullOrWhiteSpace(duration.Text))
                        {
                            InvalidInput("duration");
                            saveEnabled = false;
                            return;
                        } else
                        {
                            _duration = duration.Text;
                            saveEnabled = true;
                        }

                        WriteToXMLFile(doc, root, _title, _type, _duration);
                        Debug.WriteLine("\nROOT AFTER REPLACEMENT DURCHGANG " + i + "\n" + root);
                    }
                }

                

                //doc.Save(path);
                Save(doc, null, path);
                LoadPlans();
                DisplayPlans();
            }
            //THIS IS ONLY CALLED IF NO EDIT ( = NEW ENTRY)
            else
            {
                Debug.WriteLine("\n\nWENT INTO NO EDIT\n\n");

                string _entryName = EntryName.Text;
                XDocument doc = XDocument.Load(path);
                XElement root = new XElement("entry", new XAttribute("PlanName", _entryName));

                root.Add(new XElement("entryName", _entryName));

                if (!String.IsNullOrWhiteSpace(_entryName))
                {
                    for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
                    {
                        TextBox title = (TextBox)this.FindName("EntryTitle" + i);
                        string _title;
                        ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                        string _type;
                        TextBox duration = (TextBox)this.FindName("EntryDuration" + i);
                        string _duration;

                        if (String.IsNullOrEmpty(title.Text) && String.IsNullOrEmpty(type.Text) && String.IsNullOrEmpty(duration.Text))
                        {
                            //maybe fill variables with empty content?
                            continue;
                        }
                        else
                        {
                            if (String.IsNullOrWhiteSpace(title.Text))
                            {
                                Debug.WriteLine("Invalid input TEXT2");
                                InvalidInput("text");
                                saveEnabled = false;
                                return;
                            }
                            else
                            {
                                _title = title.Text;
                                saveEnabled = true;
                            }

                            if (String.IsNullOrWhiteSpace(type.Text))
                            {
                                InvalidInput("type");
                                saveEnabled = false;
                                return;
                            }
                            else
                            {
                                _type = type.Text;
                                saveEnabled = true;
                            }

                            if (String.IsNullOrWhiteSpace(duration.Text))
                            {
                                InvalidInput("duration");
                                saveEnabled = false;
                                return;
                            }
                            else
                            {
                                _duration = duration.Text;
                                saveEnabled = true;
                            }

                            WriteToXMLFile(doc, root, _title, _type, _duration);
                        }
                    }
                }
                if (root.Elements().Count<XElement>() > 1)
                {
                    doc.Element("antistract_plan").Add(root);
                    Save(doc, null, path);
                }
            }
            LoadPlans();
            DisplayPlans();
        }
    
        private void InvalidInput(string type)
        {
            if (String.IsNullOrWhiteSpace(EntryName.Text))
            {
                InfoField.Content = "Please fill in a plan name.";
            } else
            {
                InfoField.Content = "Please fill in all fields.";
            }
            InfoField.Visibility = Visibility.Visible;
        }
        private void DisableInvalidInputText()
        {
            InfoField.Visibility = Visibility.Hidden;
            InfoField.Content = "Please fill in all fields.";
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

        public void ReplaceInXMLFile(XDocument doc, XElement root, string _title, string _type, string _duration)
        {
            XElement toReplace = doc.Descendants("entry").FirstOrDefault(el => el.Attribute("PlanName")?.Value == CurrentlySelectedPlan.SelectedPlan);

            if (!string.IsNullOrEmpty(_title))
            {
                toReplace.ReplaceNodes(new XElement("event",
                    new XElement("title", _title),
                    new XElement("type", _type),
                    new XElement("duration", _duration)));
            }
        }

        public void ResetPlanCreatorItems()
        {
            for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)   //Changed: i from 1 to 0, and -1 to -2
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
            DisableInvalidInputText();
        }

        private void TickPlan(string PlanName)
        {
            EntryNames planName = new EntryNames();
            for (int i = 0; i < PlanOverviewStackPanel.Items.Count; i++)
            {
                planName = (EntryNames)PlanOverviewStackPanel.Items[i];

                if (planName.entryName == PlanName)
                {
                    planName.isChecked = true;
                }
            }
            ToggleDeleteButton(true);
            ToggleEditButton(true);
            DisableInvalidInputText();
        }

        private void Save(XDocument document, XmlDocument xmldoc, string path)
        {
            if (document == null)
            {
                xmldoc.Save(path);
            }
            else if (xmldoc == null)
            {
                document.Save(path);
            }
            DisableInvalidInputText();
        }

        public void ToggleAddButton(bool visibility)
        {
            if (visibility)
            {
                AddElementButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddElementButton.Visibility = Visibility.Collapsed;
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

            if(title.Text == "" && type.Text == "" && duration.Text == "")
            {
                Grid grid = title.Parent as Grid;
                Border border = grid.Parent as Border;
                border.Visibility = Visibility.Collapsed;
            }
        }
        public void TogglePlanCreatorItems(bool toggle)
        {
            for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
            {
                TextBox title = (TextBox)this.FindName("EntryTitle" + (i));
                title.IsEnabled = toggle;

                ComboBox type = (ComboBox)this.FindName("EntryType" + (i));
                type.IsEnabled = toggle;

                TextBox duration = (TextBox)this.FindName("EntryDuration" + (i));
                duration.IsEnabled = toggle;
            }
        }

        private void AddPlanButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPlansViewButton.Command.Execute(null);
            SavePlan.IsEnabled = true;
            
            LoadPlans();
            DisplayPlans();
            CurrentlySelectedPlan.SelectedPlan = "";
            EntryName.Clear();
            EntryName.Text = "New Plan";
            EntryName.IsEnabled = true;
            isEdited(false);
            ResetPlanCreatorItems();
            ToggleAddButton(true);
            TogglePlanCreatorItems(true);
            ToggleRemoveButton(true);

            ToggleDeleteButton(false);
            ToggleEditButton(false);
            DisableInvalidInputText();
            
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

        private void EventDelete_Click(object sender, RoutedEventArgs e)    //Event List
        {
            Button button = (Button)sender;
            Grid grid = button.Parent as Grid;
            Border border = grid.Parent as Border;
            int eventToDeleteIndex = PlanCreatorWrapPanel.Children.IndexOf(border);
            int currentlyEnabledEntrySlots = CurrentlyEnabledEntrySlots();
            Debug.WriteLine("This was event no " + eventToDeleteIndex + "\nTotal active events: " + currentlyEnabledEntrySlots);

            for (int i = eventToDeleteIndex; i < currentlyEnabledEntrySlots; i++)
            {
                bool last = true;
                TextBox title = (TextBox)this.FindName("EntryTitle" + (i));
                TextBox title2 = (TextBox)this.FindName("EntryTitle" + (i+1));
                if (title2 != null)
                {
                    last = false;
                    title.Text = title2.Text;
                }

                ComboBox type = (ComboBox)this.FindName("EntryType" + (i));
                ComboBox type2 = (ComboBox)this.FindName("EntryType" + (i+1));
                if (type2 != null)
                {
                    type.Text = type2.Text;
                }

                TextBox duration = (TextBox)this.FindName("EntryDuration" + (i));
                TextBox duration2 = (TextBox)this.FindName("EntryDuration" + (i+1));
                if (duration2 != null)
                {
                    duration.Text = duration2.Text;
                }

                if (i == currentlyEnabledEntrySlots-1 && !last)
                {
                    Debug.WriteLine("NOT LAST - clearing entry slot: " + i);
                    title2.Clear();
                    type2.SelectedIndex = -1;
                    duration2.Clear();
                    PlanCreatorWrapPanel.Children[currentlyEnabledEntrySlots - 1].Visibility = Visibility.Collapsed;
                } 
                else if (i == currentlyEnabledEntrySlots-1 && last)
                {
                    Debug.WriteLine("IS LAST - clearing entry slot: " + i);
                    title.Clear();
                    type.SelectedIndex = -1;
                    duration.Clear();
                    PlanCreatorWrapPanel.Children[currentlyEnabledEntrySlots-1].Visibility = Visibility.Collapsed;
                }
            }

/*
            foreach (Object element in grid.Children)
            {
                if (element is TextBox)
                {
                    var textBox = (TextBox)element;
                    textBox.Clear();
                }
                else if (element is ComboBox)
                {
                    var comboBox = (ComboBox)element;
                    comboBox.SelectedIndex = -1;
                }
            }
            border.Visibility = Visibility.Collapsed;

            SavePlanButton_Click(); //Nicht alles saven hier
            LoadPlans();
            DisplayPlans();
            ShowSelectedPlan(CurrentlySelectedPlan.SelectedPlan);
            EditButtonClick();
            ToggleRemoveButton(true);
            TickPlan(CurrentlySelectedPlan.SelectedPlan);
*/
        }

        private int CurrentlyEnabledEntrySlots()
        {
            int slots = 0;
            foreach (Object PlanCreatorItem in PlanCreatorWrapPanel.Children)
            {
                if (PlanCreatorItem is Border)
                {
                    Border temp = PlanCreatorItem as Border;
                    if (temp.Visibility == Visibility.Visible)
                    {
                        slots++;
                    }
                }
            }
            return slots;
        }

        private void PlanEntryNameList_Click(object sender, RoutedEventArgs e)  //Plan List
        {
            RadioButton radiobutton = (RadioButton)sender;
            GetPlan(radiobutton.Content.ToString());
            CurrentlySelectedPlan.SelectedPlan = radiobutton.Content.ToString();
            ShowSelectedPlan(CurrentlySelectedPlan.SelectedPlan);
            ToggleDeleteButton(true);
            ToggleEditButton(true);
            SavePlan.IsEnabled = false;
            EntryName.Text = CurrentlySelectedPlan.SelectedPlan;
            EntryName.IsEnabled = false;
            DisableInvalidInputText();
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            XDocument doc = XDocument.Load(path);
            doc.Descendants("entry").Where(p => p.Attribute("PlanName").Value == CurrentlySelectedPlan.SelectedPlan).FirstOrDefault().Remove();
            Save(doc, null, path);
            //doc.Save(path);

            ResetPlanCreatorItems();
            ToggleAddButton(false);
            LoadPlans();
            DisplayPlans();
            ToggleDeleteButton(false);
            ToggleEditButton(false);
            EntryName.Clear();
            DisableInvalidInputText();
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            EditButtonClick();
            ToggleRemoveButton(true);
            ToggleEditButton(false);
            SavePlan.IsEnabled = true;
        }
        private void EditButtonClick()
        {
            EntryName.Text = CurrentlySelectedPlan.SelectedPlan;
            EntryName.IsEnabled = true;
            for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 1; i++)
            {
                TogglePlanCreatorItem(i, true);
            }
            ToggleAddButton(true);
            isEdited(true);
        }
        private void ToggleRemoveButton(bool visibility)
        {
            foreach (Object PlanCreatorItem in PlanCreatorWrapPanel.Children)
            {
                if (PlanCreatorItem is Border)
                {
                    Border temp = PlanCreatorItem as Border;
                    Grid grid = temp.Child as Grid;

                    foreach (Object element in grid.Children)
                    {
                        if (element is Button)
                        {
                            var button = (Button)element;
                            
                            if (visibility)
                            {
                                button.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                button.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        private void ShowTimerWindow_Click(object sender, RoutedEventArgs e)
        {
            TimerWindow timerWindow = new TimerWindow(CurrentlySelectedPlan.SelectedPlan);
            timerWindow.Show();
            Debug.WriteLine(CurrentlySelectedPlan.SelectedPlan);
        }

        public void ToggleDeleteButton(bool visibility)
        {
            if (visibility)
            {
                DeleteButton.IsEnabled = true;
            }
            else
            {
                DeleteButton.IsEnabled = false;
            }
        }
        public void ToggleEditButton(bool visibility)
        {
            if (visibility)
            {
                EditButton.IsEnabled = true;
            }
            else
            {
                EditButton.IsEnabled = false;
            }
        }
    }
}