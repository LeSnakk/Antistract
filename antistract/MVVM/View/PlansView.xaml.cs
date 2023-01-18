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
        readonly string path = "Plans/paradeplan.xml";
        public List<Plans> Plans = new List<Plans>();
        private string _currentlySelectedPlan = "";
        private bool _isEdited = false;
        private bool saveEnabled = false;

        CurrentlySelectedPlan CurrentlySelectedPlan = new CurrentlySelectedPlan();

        public PlansView()
        {
            InitializeComponent();
            DisplayPlans();
        }

        public bool isEdited()
        {
            return _isEdited;
        }
        public void isEdited(bool isEdited)
        {
            _isEdited = isEdited;
        }

        //Check if the input contains only numbers (for "duration" parameter in plan events)
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Read plans from global variables
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

        //Load plans from XML database into global variables class
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
                    GlobalVariables.PlanNames.Add(Event["entryName"].InnerText);
                }
            }
        }

        public void DisplayPlans()
        {
            GVPlanNamesToOCPlanNames();
        }

        //Load selected plan from XML document and display it on PLansView
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

        //Button on plansview which saves the plan
        private void SavePlanButton_Click(object sender, RoutedEventArgs e)
        {
            saveEnabled = false;
            //Check for plan name duplicate
            if ((!GlobalVariables.PlanNames.Contains(EntryName.Text) || isEdited()) && EntryName.Text != "")
            {
                SavePlanButton_Click();
                if (saveEnabled)
                {                    
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
                
            } else if (GlobalVariables.PlanNames.Contains(EntryName.Text))
            {
                InfoField.Visibility = Visibility.Visible;
                InfoField.Content = "Please pick a plan name that does not exist yet.";
            }
        }

        //Save plan to XML file
        private void SavePlanButton_Click() {
            //If it's an edited plan
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

                //Loop through each event
                for (int i = 0; i < PlanCreatorWrapPanel.Children.Count - 2; i++)
                {
                    TextBox title = (TextBox)this.FindName("EntryTitle" + i);
                    ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                    TextBox duration = (TextBox)this.FindName("EntryDuration" + i);

                    //If event is completely empty, ignore it
                    if (String.IsNullOrEmpty(title.Text) && String.IsNullOrEmpty(type.Text) && String.IsNullOrEmpty(duration.Text))
                    {
                        continue;
                    }
                    else
                    {
                        //Invalid title name input
                        if (String.IsNullOrWhiteSpace(title.Text))
                        {
                            InvalidInput("text");
                            saveEnabled = false;
                            return;
                        }
                        else
                        {
                            saveEnabled = true;
                        }
                        //Invalid type input
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
                        //Invalid duration input
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

                //Loop through XML
                for (int l = 0; l < elements.Count; l++)
                {
                    foreach (XmlNode Event in elements[l].ChildNodes)
                    {
                        //Find and remove edited plan
                        if (Event["entryName"].InnerText == CurrentlySelectedPlan.SelectedPlan)
                        {
                            int i = 1;
                            while (i < Event.ChildNodes.Count)
                            {
                                Event.RemoveChild(Event.ChildNodes[i]);
                            }
                        }
                    }
                }
                Save(null, doc1, path);

                //Save "new" edited plan to XML
                XDocument doc = XDocument.Load(path);
                XElement root = doc.Descendants("entry").FirstOrDefault(p => p.Attribute("PlanName").Value == CurrentlySelectedPlan.SelectedPlan);

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
                        continue;
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(title.Text))
                        {
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
                    }
                }
                Save(doc, null, path);
                LoadPlans();
                DisplayPlans();
            }

            //If it's a new plan
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
                        string _title;
                        ComboBox type = (ComboBox)this.FindName("EntryType" + i);
                        string _type;
                        TextBox duration = (TextBox)this.FindName("EntryDuration" + i);
                        string _duration;

                        if (String.IsNullOrEmpty(title.Text) && String.IsNullOrEmpty(type.Text) && String.IsNullOrEmpty(duration.Text))
                        {
                            continue;
                        }
                        else
                        {
                            if (String.IsNullOrWhiteSpace(title.Text))
                            {
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
            //Re-load plans
            LoadPlans();
            DisplayPlans();
        }
    
        //Handle input error messages
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

        //Write data collected from save method to XML file
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

        //Wipe PlansView event elements
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

        //Keep selected plan ticked (visually)
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

        //Save modified XML file after change
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

        //Toggle "add" button visibility
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

        //Toggle individual event item accessibility
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
        //Toggle all event item accessibility
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

        //If user wants to add a new plan
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

        //If user wants to add another event item
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

        //If user wants to delete specific event item
        private void EventDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Grid grid = button.Parent as Grid;
            Border border = grid.Parent as Border;
            int eventToDeleteIndex = PlanCreatorWrapPanel.Children.IndexOf(border);
            int currentlyEnabledEntrySlots = CurrentlyEnabledEntrySlots();

            //Copy data from following event item into previous event item and delete last 
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
                    title2.Clear();
                    type2.SelectedIndex = -1;
                    duration2.Clear();
                    PlanCreatorWrapPanel.Children[currentlyEnabledEntrySlots - 1].Visibility = Visibility.Collapsed;
                } 
                else if (i == currentlyEnabledEntrySlots-1 && last)
                {
                    title.Clear();
                    type.SelectedIndex = -1;
                    duration.Clear();
                    PlanCreatorWrapPanel.Children[currentlyEnabledEntrySlots-1].Visibility = Visibility.Collapsed;
                }
            }
        }

        //Obtain number of used event items
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

        //If user clicks on a plan in navbar
        private void PlanEntryNameList_Click(object sender, RoutedEventArgs e)  //Plan List
        {
            RadioButton radiobutton = (RadioButton)sender;
            CurrentlySelectedPlan.SelectedPlan = radiobutton.Content.ToString();
            ShowSelectedPlan(CurrentlySelectedPlan.SelectedPlan);
            ToggleDeleteButton(true);
            ToggleEditButton(true);
            SavePlan.IsEnabled = false;
            EntryName.Text = CurrentlySelectedPlan.SelectedPlan;
            EntryName.IsEnabled = false;
            DisableInvalidInputText();
        }

        //If user wants to delete a plan. Plan is removed from XML file
        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            XDocument doc = XDocument.Load(path);
            doc.Descendants("entry").Where(p => p.Attribute("PlanName").Value == CurrentlySelectedPlan.SelectedPlan).FirstOrDefault().Remove();
            Save(doc, null, path);

            ResetPlanCreatorItems();
            ToggleAddButton(false);
            LoadPlans();
            DisplayPlans();
            ToggleDeleteButton(false);
            ToggleEditButton(false);
            EntryName.Clear();
            DisableInvalidInputText();
        }

        //If user wants to edit a plan
        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            EditButtonClick();
            ToggleRemoveButton(true);
            ToggleEditButton(false);
            SavePlan.IsEnabled = true;
        }
        //Event item space is set to "edit" mode
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
        //Event items get the option to be removed
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

        //Enable/Disable UI buttons
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