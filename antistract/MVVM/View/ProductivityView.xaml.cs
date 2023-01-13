using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using antistract.Core;
using antistract.MVVM.ViewModel;
using Microsoft.WindowsAPICodePack.Shell;
using antistract.Properties;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text;
using System.Windows.Media.Animation;

namespace antistract.MVVM.View
{
    public partial class ProductivityView : UserControl
    {
        readonly MainWindow mainWndw = (MainWindow)Application.Current.MainWindow;

        static BackgroundWorker bgWorker = new BackgroundWorker();
        static Dictionary<String, String> programs = new Dictionary<String, String>();
        static Dictionary<String, String> paths = new Dictionary<String, String>();
        public List<RegistryKey> installedPrograms = new List<RegistryKey>();

        CurrentlySelectedPlan CurrentlySelectedPlan = new CurrentlySelectedPlan();

        public static bool ShouldCheck;
        string CheckMode;

        private string SelectedProgramName;
        private string SelectedProcessName;
        private string SelectedProcessPath;
        private string SelectedWebsite;

        private string RemoveSelectedProgramName;
        private string RemoveSelectedProcessName;
        private string RemoveSelectedProcessPath;

        private List<string> BlacklistedPaths = new List<String>();
        private List<string> namesList = new List<String>();
        private List<string> DisplayBlacklistedNames = new List<String>();
        private List<string> BlacklistedWebsites = new List<String>();
        private bool BlackListPlaceholderText = true;
        private bool Deselecting = false;

        private string GCExLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google/Chrome/User Data/Default/Sync Extension Settings/dkcmjcfdomcigfioegnmleiijjoccfea/000003.log");

        ThreadStart loadInstalledPrograms = LoadInstalledPrograms;
        TimerWindow timerWindow;

        public ProductivityView()
        {
            InitializeComponent();
            FillPickPlanDropdown();
            bgWorker.DoWork += BgWorker_DoWork;
            FirstStartup();
            LoadBlacklistUserSave();
            GlobalVariables.CheckBrowser = false;
            GlobalVariables.CheckPrograms = false;
            //Debug_ClearUserSettings();
        }

        public void Debug_ClearUserSettings()
        {
            Settings.Default.BlacklistedPaths.Clear();
            Settings.Default.BlacklistedProcesses.Clear();
            Settings.Default.BlacklistedPrograms.Clear();
            Settings.Default.BlacklistedDisplayNames.Clear();
        }

        //Initialize user settings on first start
        public void FirstStartup()
        {
            CheckMode = "";
            Settings.Default.StartEnabled = false;
            Settings.Default.Save();

            if (Settings.Default.FirstStartup)
            {
                Settings.Default.BlacklistedPrograms = new StringCollection();
                Settings.Default.BlacklistedProcesses = new StringCollection();
                Settings.Default.BlacklistedPaths = new StringCollection();
                Settings.Default.BlacklistedDisplayNames = new StringCollection();
                Settings.Default.BlacklistedWebsites = new StringCollection();
                Settings.Default.BlacklistedWebsites.Add(("www.youtube.com").ToString());
                Settings.Default.BlacklistedWebsites.Add(("netflix.com").ToString());
                Settings.Default.FirstStartup = false;
                Settings.Default.Save();

                Debug.WriteLine("First start");
            }
            else
            {
                Debug.WriteLine("Not first start");
                return;
            }
        }

        //Load saved blacklist data
        public void LoadBlacklistUserSave()
        {
            //Location of settings.settings (user settings)
            Debug.WriteLine(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

            //Reset variables
            BlacklistedPaths.Clear();
            namesList.Clear();
            DisplayBlacklistedNames.Clear();

            BlacklistedWebsites.Clear();

            //Load data
            BlacklistedPaths = Settings.Default.BlacklistedPaths.Cast<string>().ToList();
            namesList = Settings.Default.BlacklistedProcesses.Cast<string>().ToList();
            namesList.AddRange(Settings.Default.BlacklistedPrograms.Cast<string>().ToList());
            DisplayBlacklistedNames = Settings.Default.BlacklistedDisplayNames.Cast<string>().ToList();

            BlacklistedWebsites = Settings.Default.BlacklistedWebsites.Cast<string>().ToList();

            blacklistList.Items.Clear();
            WebsitesBlacklistList.Items.Clear();

            //Create listbox entries for blacklisted items to be displayed on GUI
            foreach (string name in DisplayBlacklistedNames)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = name;
                blacklistList.Items.Add(item);
            }
            if (blacklistList.Items.Count > 0)
            {
                NoBlacklistPlaceholderText.Visibility = Visibility.Hidden;
                blacklistList.IsEnabled = false;
            }
            int temp = 0;

            //Call method to transmit blacklisted websites to Browser extension
            foreach (string name in BlacklistedWebsites)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = name;
                WebsitesBlacklistList.Items.Add(item);
                if (temp < 1)
                {
                    SyncToXML(name, true, "add");
                }
                else
                {
                    SyncToXML(name, false, "add");
                }
                temp++;
            }
            if (WebsitesBlacklistList.Items.Count > 0)
            {
                NoWebsitesBlacklistPlaceholderText.Visibility = Visibility.Hidden;
                WebsitesBlacklistList.IsEnabled = true;
            }
        }

        //Manage loading the installed programs
        public void GetInstalledPrograms(object sender, RoutedEventArgs e)
        {
            paths.Clear();
            programs.Clear();

            //Callback function to display entries in listbox
            loadInstalledPrograms += () =>
            {
                ShowListCallBack();
            };

            btn_CallLoad.Visibility = Visibility.Hidden;
            LoadingText.Visibility = Visibility.Visible;

            //Loading programs on new thread for the UI to not become unresponsive
            Thread thread = new Thread(loadInstalledPrograms) { IsBackground = true };
            thread.Start();
        }

        //Search for installed programs. Most installed programs are registered in the AppsFolder
        public static void LoadInstalledPrograms()
        {
            var FOLDERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
            ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);

            foreach (var app in (IKnownFolder)appsFolder)
            {
                //regular installed programs
                if (app.Properties.System.Link.TargetParsingPath.Value != null)
                {
                    AddToInstalledProgramsList(app.Name, app.Properties.System.Link.TargetParsingPath.Value, "reg");
                }
            }
        }

        //If a program is selected and added by the user, it's added to blacklist 
        public static void AddToInstalledProgramsList(string programName, string programPath, string programType)
        {
            string processName = "";
            if (programType == "reg")
            {

                programPath = programPath.Replace("/", "\\");
                processName = programPath.Split("\\").Last();

                if (!programs.ContainsKey(programName))
                {
                    programs.Add(programName, processName);
                    paths.Add(programName, programPath.Substring(0, programPath.LastIndexOf("\\") + 1));
                }
                else
                {
                    AddDuplicateEntry(programName, processName, 1);
                    AddDuplicateEntryPath(programName, programPath, 1);
                }
            }
            else if (programType == "win")
            {
                //...
                //Microsoft store apps are weird. Please don't use Microsoft store.
            }
            Debug.WriteLine(programName + ": " + processName);
        }

        //If certain programs have the same name, a number is added to it's displayed name and path
        public static void AddDuplicateEntry(string programName, string processName, int i)
        {
            if (programs.ContainsKey(programName + " (" + i + ")"))
            {
                AddDuplicateEntry(programName, processName, ++i);
            }
            else
            {
                programs.Add(programName + " (" + i + ")", processName);
            }
        }
        public static void AddDuplicateEntryPath(string programName, string programPath, int i)
        {
            if (programs.ContainsKey(programName + " (" + i + ")"))
            {
                AddDuplicateEntry(programName, programPath.Substring(0, programPath.LastIndexOf("\\") + 1), ++i);
            }
            else
            {
                paths.Add(programName + " (" + i + ")", programPath.Substring(0, programPath.LastIndexOf("\\") + 1));
            }
        }

        //To display the installed programs on the UI, a Dispatcher is needed to update the UI from the non-UI thread.
        //It's running synchronously on the program-loading-thread
        private void ShowListCallBack()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { ShowTheList(); }));
        }

        //Create listbox items for each installed program
        private void ShowTheList()
        {
            listBox.Items.Clear();
            foreach (string s in programs.Keys)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = s;
                listBox.Items.Add(item);
            }

            listBox.Items.SortDescriptions
                .Add(new System.ComponentModel.SortDescription(
                    "Content", System.ComponentModel.ListSortDirection.Ascending));

            LoadingText.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Visible;

            ToggleAddToBlacklistButton(true);
            ToggleRemoveFromBlacklistButton(true);
            blacklistList.IsEnabled = true;
        }

        //Begin the timer
        public static void startChecking()
        {
            StartChecking();
            TimerWindow.TimerOnHoldNO();
        }
        public static void StartChecking()
        {
            bgWorker.RunWorkerAsync();
        }

        //Logic for supervising running programs and communicating to browser extension
        //This is a background worker. It is used to check every n seconds which programs are running. Running on another thread.
        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BlacklistedPaths.Clear();
            namesList.Clear();
            DisplayBlacklistedNames.Clear();
            Process[] processes;

            bool checkBrowser = GlobalVariables.CheckBrowser;
            bool checkPrograms = GlobalVariables.CheckPrograms;

            Debug.WriteLine("in bgworker: " + checkBrowser);
            Debug.WriteLine("in bgworker: " + checkPrograms);
            Debug.WriteLine("ischecked: " + isChecked());

            BlacklistedPaths = Settings.Default.BlacklistedPaths.Cast<string>().ToList();
            namesList = Settings.Default.BlacklistedProcesses.Cast<string>().ToList();
            namesList.AddRange(Settings.Default.BlacklistedPrograms.Cast<string>().ToList());
            DisplayBlacklistedNames = Settings.Default.BlacklistedDisplayNames.Cast<string>().ToList();

            //If isChecked is true the bgworker checks for running programs. Can be false eg when Plan entry is "break"
            while (isChecked())
            {
                //Checking every n ms
                //higher = slower = lower CPU usage
                Thread.Sleep(500);

                //Check if blacklisted program name can be found in active processes. If so, add process to array
                processes = namesList.SelectMany(name => Process.GetProcessesByName(name)).ToArray();

                Debug.WriteLine("Globalvariables ohlypausing: " + GlobalVariables.OnlyPausing);
                Debug.WriteLine("Globalvariables checkbrowser: " + GlobalVariables.CheckBrowser);
                Debug.WriteLine(processes.Length);
                //When browser should not be checked but programs
                if (processes.Length == 0 && !checkBrowser && checkPrograms)
                {
                    TimerWindow.TimerOnHoldNO();
                    Debug.WriteLine("Notepad is not running");
                    Debug.WriteLine("!1");
                }
                //When programs should not be checked but browser
                else if (!checkPrograms && GlobalVariables.CheckBrowser && GlobalVariables.OnlyPausing)
                {
                    Debug.WriteLine("bool is: " + checkBrowser);
                    CheckBrowser();
                    Debug.WriteLine("!2");
                }
                else if (processes.Length == 0 && GlobalVariables.CheckBrowser && GlobalVariables.OnlyPausing)
                {
                    Debug.WriteLine("bool is: " + checkBrowser);
                    CheckBrowser();
                    Debug.WriteLine("!2");
                }
                //If array of blacklisted but running program processes is > 0 and programs should be checked
                else if (processes.Length >= 1 && checkPrograms)
                {
                    Debug.WriteLine("Unallowed process is running");

                    foreach (Process process in processes)
                    {
                        Debug.WriteLine(process);
                        if (!process.HasExited)
                        {
                            try
                            {
                                {   //Check if the path of the process is also blacklisted to make sure no other program with the same name at another location is falsely suspected
                                    if (BlacklistedPaths.Any(path => process.MainModule.FileName.Replace("/", "\\").Substring(0, process.MainModule.FileName.Replace("/", "\\").LastIndexOf("\\") + 1).Contains(path)))
                                    {
                                        Debug.WriteLine(process.MainModule.FileName);
                                        Debug.WriteLine("Closing...");
                                        //If checkMode = Closing
                                        if (!GlobalVariables.OnlyPausing)
                                        {
                                            RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                                            //Not pausing timer because...
                                            TimerWindow.TimerOnHoldNO();
                                            Debug.WriteLine("!3");
                                            //...the process gets killed
                                            while (!process.HasExited)
                                            {
                                                process.Kill();
                                            }
                                        }
                                        //If checkMode = Pausing
                                        else if (GlobalVariables.OnlyPausing)
                                        {
                                            RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                                            //Pausing the timer as long as program is running
                                            TimerWindow.TimerOnHoldYES();
                                            Debug.WriteLine(process);
                                            Debug.WriteLine("!4");
                                        }
                                    }
                                    else
                                    {
                                        foreach (string path in BlacklistedPaths)
                                        {
                                            if ((process.MainModule.FileName.Replace("/", "\\")).Contains(path))
                                            {
                                                Debug.WriteLine(process.MainModule.FileName);
                                                Debug.WriteLine("v2Closing...");
                                                if (!GlobalVariables.OnlyPausing)
                                                {
                                                    RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                                                    TimerWindow.TimerOnHoldNO();
                                                    Debug.WriteLine("!5");
                                                    process.Kill();
                                                }
                                                else if (GlobalVariables.OnlyPausing)
                                                {
                                                    RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                                                    TimerWindow.TimerOnHoldYES();
                                                    Debug.WriteLine("!6");
                                                    Debug.WriteLine(process);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Win32Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }
                        }
                    }
                }

            }
            Debug.WriteLine("Checking stopped");
        }

        //Actions if a blacklisted website is opened
        private void CheckBrowser()
        {
            if (GlobalVariables.CheckBrowser)
            {
                if (GlobalVariables.OnlyPausing)
                {
                    //If checkMode is pausing and a blacklisted website is opened, the timer is paused
                    if (ReadGCExData())
                    {
                        RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                        TimerWindow.TimerOnHoldYES();
                        Debug.WriteLine("!2.1");
                        Debug.WriteLine(GlobalVariables.CheckBrowser);
                        Debug.WriteLine("Chrome forbidden tab open");
                    }
                    //If checkMode is pausing and blacklisted tab is opened, the timer is no longer paused
                    else if (!ReadGCExData())
                    {
                        RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                        Debug.WriteLine("!2.2");
                        TimerWindow.TimerOnHoldNO();
                    }
                }
            }
        }

        //Bool that reads the Google Chrome extension data and reports if a tab is opened accordingly
        public bool ReadGCExData()
        {
            try
            {
                //Reading Google Chrome extension data file
                using (var fs = new FileStream(GCExLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.ASCII))
                {
                    string temp = sr.ReadToEnd();
                    //String that indicates that a blacklisted website has been opened
                    if (temp.Contains("-cT2A;z=YzW}f4ht/H6epiW2!Md*@,"))
                    {
                        Debug.WriteLine("true");
                        Process[] chrome = Process.GetProcessesByName("chrome");
                        if (chrome.Length > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //String that indicates that no blacklisted website has been opened
                    else if (temp.Contains("8fj*d-*c@cP}+i3f%aB*BD#63amL*i"))
                    {
                        Debug.WriteLine("false1");
                        return false;
                    }
                    //Fallback: If error occurs, it's not blocking the software
                    else
                    {
                        Debug.WriteLine("false2");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return default;
            }
        }

        public static bool isChecked()
        {
            return ShouldCheck;
        }

        //Create an entry in the dropdown for each saved plan
        private void FillPickPlanDropdown()
        {
            PickPlanDropdown.Items.Clear();
            if (GlobalVariables.PlanNames.Count > 0)
            {
                foreach (String name in GlobalVariables.PlanNames)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = name;
                    PickPlanDropdown.Items.Add(item);
                    item = null;
                }
            }
            else
            //If no plan is available, a default message is shown which redirects to the PlansView when selected
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = "<Please create a plan first>";
                PickPlanDropdown.Items.Add(item);
            }
        }

        void PrintText(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);

            ListBoxItem item = new ListBoxItem();
            item.Content = "hallo";
        }

        //If a plan is selected/the selection changed, the selected plan gets stored
        private void PickPlanDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PickPlanDropdownDefaultText.Visibility = Visibility.Hidden;
            ComboBox pickPlanDropdown = (ComboBox)sender;

            //Redirecting to PlansView
            if (GlobalVariables.PlanNames.Count < 1)
            {
                SwitchToPlansView();
            }
            else
            {
                ComboBoxItem selectedPlan = (ComboBoxItem)pickPlanDropdown.SelectedItem;
                CurrentlySelectedPlan.SelectedPlan = selectedPlan.Content.ToString();
                //Manages if the Start button should be enabled based on if the other required parameters are given (checkmode, blacklist)
                if (GlobalVariables.OnlyPausing != null && CheckMode != "" && CareAboutPrograms())
                {
                    Settings.Default.StartEnabled = true;
                    Settings.Default.Save();
                }
            }
        }

        //Method doing exactly what it sounds like
        public void SwitchToPlansView()
        {
            mainWndw.MenuButtonPlans.IsChecked = true;
            GoToPlansViewButton.Command.Execute(null);
        }

        //Handles if the browser should be supervised
        public static void ShouldCheckYes()
        {
            ShouldCheck = true;
            if (!GlobalVariables.OnlyPausing && GlobalVariables.BrowserClose)
            {
                SetExtensionCheckModeClosing();
            }
            else
            {
                SetExtensionCheckModePausing();
            }
        }
        public static void ShouldCheckNo()
        {
            ShouldCheck = false;
            SetExtensionCheckModePausing();
        }

        //If checkMode is set to close the responsible variables are set accordingly
        private void Close_Program_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.OnlyPausing = false;
            CheckMode = "closing";
            //Manages if the Start button should be enabled based on if the other required parameters are given
            if (CurrentlySelectedPlan.SelectedPlan != null && CareAboutPrograms())
            {
                Settings.Default.StartEnabled = true;
                Settings.Default.Save();
            }
            ModeText.Text = "Distracting processes will be automatically closed.";
        }

        //If checkMode is set to pausing the responsible variables are set accordingly
        private void Stop_Timer_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.OnlyPausing = true;
            CheckMode = "pausing";
            SetExtensionCheckModePausing();
            //Manages if the Start button should be enabled based on if the other required parameters are given
            if (CurrentlySelectedPlan.SelectedPlan != null && CareAboutPrograms())
            {
                Settings.Default.StartEnabled = true;
                Settings.Default.Save();
            }
            ModeText.Text = "Distracting processes will interrupt your schedule.";
        }

        //Start button is clicked
        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            //Check if all required parameters are given
            if (!(FilterWebsites.IsChecked.Value == false && FilterPrograms.IsChecked.Value == false) && CareAboutPrograms())
            {
                //Reset Timer Window in case an other plan had been started prior
                timerWindow = null;
                GlobalVariables.timerWindow = null;
                if (!GlobalVariables.TimerRunning)
                {
                    //Call method to transmit the blacklist and checkMode information to browser extension
                    TransmitToBrowserExtension();

                    GlobalVariables.CheckBrowser = FilterWebsites.IsChecked.Value;
                    GlobalVariables.CheckPrograms = FilterPrograms.IsChecked.Value;

                    //Developer information: What exactly is the timer having an eye on now?
                    Debug.WriteLine("\n**********");
                    Debug.WriteLine("Plan: " + CurrentlySelectedPlan.SelectedPlan);
                    Debug.WriteLine("Prog: " + GlobalVariables.CheckPrograms);
                    Debug.WriteLine("Web.: " + GlobalVariables.CheckBrowser + " || " + FilterWebsites.IsChecked.Value);
                    Debug.WriteLine("Mode: " + CheckMode);
                    Debug.WriteLine("**********\n");

                    //Instanciate Timerwindow with the currently selected plan
                    timerWindow = new TimerWindow(CurrentlySelectedPlan.SelectedPlan);
                    GlobalVariables.timerWindow = timerWindow;
                    GlobalVariables.timerWindow.Show();
                    GlobalVariables.TimerRunning = true;
                    Settings.Default.StartEnabled = false;
                    Settings.Default.BlacklistBlocked = true;
                    Settings.Default.Save();
                    Debug.WriteLine(TimerWindow.TimerOnHold);
                }
            }
        }

        //Bool returns if the running programs should be supervised
        private bool CareAboutPrograms()
        {
            if (GlobalVariables.CheckPrograms || FilterPrograms.IsChecked.Value == true)
            {
                if (Settings.Default.BlacklistedPrograms.Count != 0)
                {
                    return true;
                } else
                {
                    return false;
                }
            } else
            {
                return true;
            }
        }

        //Transmit checkMode data to browser extension. Communication via XML file
        private void TransmitToBrowserExtension()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("BrowserExtensions/Chrome/data.xml");

            XmlNodeList checkModeNodeList = doc.GetElementsByTagName("checkMode");
            XmlNode checkMode = checkModeNodeList[0];

            checkMode.InnerText = CheckMode;

            doc.Save("BrowserExtensions/Chrome/data.xml");

            if (!GlobalVariables.CheckBrowser)
            {
                SetExtensionCheckModePausing();
            }
        }

        //Transmit blacklisted websites data to browser extension. Communication via XML file
        //Takes parameters: website link, should database be cleared first, should said website link be added or removed from database
        public static void SyncToXML(string websiteName, bool clearFirst, string addOrRemove)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("BrowserExtensions/Chrome/data.xml");

            XmlNodeList websitesNodeList = doc.GetElementsByTagName("websites");
            XmlNode websitesRoot = websitesNodeList[0];

            switch (addOrRemove)
            {
                case "add":

                    if (clearFirst)
                    {
                        websitesRoot.RemoveAll();
                    }

                    XmlElement newWebsite = doc.CreateElement("website");

                    newWebsite.InnerText = websiteName;

                    websitesRoot.AppendChild(newWebsite);
                    break;

                case "remove":
                    foreach (XmlNode website in websitesRoot.ChildNodes)
                    {
                        if (website.InnerText == websiteName)
                        {
                            Debug.WriteLine(website.InnerText);
                            websitesRoot.RemoveChild(website);
                            continue;
                        }
                    }
                    break;
            }
            doc.Save("BrowserExtensions/Chrome/data.xml");
        }

        //Storing website in user settings
        public static void AddWebsiteToSaves(string websiteName)
        {
            Settings.Default.BlacklistedWebsites.Add(websiteName.ToString());
            Settings.Default.Save();
        }

        public static void SetExtensionCheckModePausing()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("BrowserExtensions/Chrome/data.xml");

            XmlNodeList checkModeNodeList = doc.GetElementsByTagName("checkMode");
            XmlNode checkMode = checkModeNodeList[0];

            checkMode.InnerText = "pausing";
            doc.Save("BrowserExtensions/Chrome/data.xml");
        }
        public static void SetExtensionCheckModeClosing()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("BrowserExtensions/Chrome/data.xml");

            XmlNodeList checkModeNodeList = doc.GetElementsByTagName("checkMode");
            XmlNode checkMode = checkModeNodeList[0];

            checkMode.InnerText = "closing";
            doc.Save("BrowserExtensions/Chrome/data.xml");
        }

        //Manage availability of filter UI
        private void ToggleAddToBlacklistButton(bool isDisabled)
        {
            AddToBlacklist.IsEnabled = isDisabled;
        }
        private void ToggleRemoveFromBlacklistButton(bool isDisabled)
        {
            RemoveFromBlacklist.IsEnabled = isDisabled;
        }

        //Add selected program to the program blacklist
        private void AddToBlacklist_Click(object sender, RoutedEventArgs e)
        {
            ProgramNotSupportedMessage.Visibility = Visibility.Hidden;
            try
            {
                if (SelectedProgramName != null)
                {
                    NoBlacklistPlaceholderText.Visibility = Visibility.Hidden;
                    blacklistList.IsEnabled = true;
                    if (SelectedProcessName.Contains("."))
                    {
                        SelectedProcessName = SelectedProcessName.Substring(0, SelectedProcessName.LastIndexOf("."));
                        SelectedProcessPath = paths[SelectedProgramName];
                    }
                    if (namesList.Count < 1)
                    {
                        blacklistList.Items.Clear();
                    }

                    if (namesList.Contains(SelectedProgramName) && namesList.Contains(SelectedProcessName) && BlacklistedPaths.Contains(SelectedProcessPath))
                    {
                        return;
                    }
                    //Store program name, process name and path in blacklist
                    else
                    {
                        namesList.Add(SelectedProcessName);
                        namesList.Add(SelectedProgramName);
                        BlacklistedPaths.Add(SelectedProcessPath);

                        DisplayBlacklistedNames.Add(SelectedProgramName);

                        Debug.WriteLine(SelectedProcessPath);
                        ListBoxItem item = new ListBoxItem();
                        item.Content = SelectedProgramName; // + " (" + SelectedProcessName + ")";
                        blacklistList.Items.Add(item);


                        Settings.Default.BlacklistedPrograms.Add(SelectedProgramName.ToString());
                        Settings.Default.BlacklistedProcesses.Add(SelectedProcessName.ToString());
                        Settings.Default.BlacklistedPaths.Add(SelectedProcessPath.ToString());
                        Settings.Default.BlacklistedDisplayNames.Add(SelectedProgramName.ToString());

                        Settings.Default.Save();

                        List<string> test = Settings.Default.BlacklistedPrograms.Cast<string>().ToList();
                        foreach (string name in test)
                        {
                            Debug.WriteLine("In user Settings:" + name);
                        }
                        Debug.WriteLine(Settings.Default.BlacklistedPrograms.Count);
                    }
                }
                SelectedProgramName = null;
                SelectedProcessName = null;
                SelectedProcessPath = null;
                Deselecting = true;
                listBox.UnselectAll();
                Deselecting = false;
            }
            //Some programs (eg certain Microsoft store apps or system Apps are not supported due to Windows policy)
            catch (Exception ex)
            {
                ProgramNotSupportedMessage.Visibility = Visibility.Visible;
            }
        }

        //Remove selected program (name, process, path) from blacklist
        private void RemoveFromBlacklist_Click(object sender, RoutedEventArgs e)
        {
            ProgramNotSupportedMessage.Visibility = Visibility.Hidden;
            if (RemoveSelectedProgramName != null)
            {
                namesList.Remove(RemoveSelectedProgramName);
                namesList.Remove(RemoveSelectedProcessName);
                BlacklistedPaths.Remove(RemoveSelectedProcessPath);
                DisplayBlacklistedNames.Remove(RemoveSelectedProgramName);

                Settings.Default.BlacklistedPrograms.Remove(RemoveSelectedProgramName.ToString());
                Settings.Default.BlacklistedProcesses.Remove(RemoveSelectedProcessName.ToString());
                Settings.Default.BlacklistedPaths.Remove(RemoveSelectedProcessPath.ToString());
                Settings.Default.BlacklistedDisplayNames.Remove(RemoveSelectedProgramName.ToString());
                Settings.Default.Save();


                blacklistList.Items.Clear();

                foreach (string name in DisplayBlacklistedNames)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = name;
                    blacklistList.Items.Add(item);
                }
            }

            if (blacklistList.Items.Count <= 0)
            {
                NoBlacklistPlaceholderText.Visibility = Visibility.Visible;
                blacklistList.IsEnabled = false;
            }

            foreach (string name in namesList)
            {
                Debug.WriteLine(name);
            }
            RemoveSelectedProgramName = "";
            RemoveSelectedProcessName = "";
            RemoveSelectedProcessPath = "";
        }

        //Obtain currently selected program listbox entry on selection change
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Not when listbox selection changes from codebehind (eg after removing program entry from listbox)
            if (!Deselecting)
            {
                string output;
                ListBoxItem lbi = listBox.SelectedItem as ListBoxItem;

                programs.TryGetValue(lbi.Content.ToString(), out output);

                SelectedProgramName = lbi.Content.ToString();
                SelectedProcessName = output;
            }
        }

        //Obtain currently selected already blacklisted program listbox entry on selection change
        private void blacklistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string output;

            if (blacklistList.Items.Count > 0)
            {
                ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);

                RemoveSelectedProgramName = lbi.Content.ToString();

                programs.TryGetValue(RemoveSelectedProgramName, out output);
                if (output.Contains("."))
                {
                    output = output.Substring(0, output.LastIndexOf("."));
                }
                RemoveSelectedProcessName = output;

                paths.TryGetValue(RemoveSelectedProgramName, out output);
                RemoveSelectedProcessPath = output;

                Debug.WriteLine("\n\n");
                Debug.WriteLine(RemoveSelectedProgramName);
                Debug.WriteLine(RemoveSelectedProcessName);
                Debug.WriteLine(RemoveSelectedProcessPath);
            }
        }

        public void ToggleStartButton(bool visibility)
        {
            if (visibility)
            {
                StartTimer.IsEnabled = true;
            }
            else
            {
                StartTimer.IsEnabled = false;
            }
        }

        //Timer stop button
        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            ShouldCheckNo();
            GlobalVariables.TimerRunning = false;
            SetExtensionCheckModePausing();
            if (GlobalVariables.OnlyPausing != null && CheckMode != "" && CareAboutPrograms())
            {
                Settings.Default.StartEnabled = true;
                Settings.Default.Save();
            }
            Settings.Default.BlacklistBlocked = false;
            Settings.Default.Save();
            GlobalVariables.timerWindow.MainTimer.Stop();
            GlobalVariables.timerWindow.WasteTimer.Stop();
            GlobalVariables.timerWindow.Close();
            TimerWindow.TimerOnHoldNO();
            TimerWindow.TimerOnHold = false;
            Debug.WriteLine(TimerWindow.TimerOnHold);
        }

        //Obtain typed in website string and store in blacklist
        private void AddToWebsitesBlacklist_Click(object sender, RoutedEventArgs e)
        {
            //Check if entry is valid
            if (!String.IsNullOrWhiteSpace(BrowserWebsites.Text))
            {
                NoWebsitesBlacklistPlaceholderText.Visibility = Visibility.Hidden;
                WebsitesBlacklistList.IsEnabled = true;

                //Prevent double entries
                foreach (ListBoxItem entry in WebsitesBlacklistList.Items)
                {
                    if (entry.Content.ToString() == BrowserWebsites.Text.ToString())
                    {
                        InfoField.Visibility = Visibility.Visible;
                        return;
                    }
                }

                InfoField.Visibility = Visibility.Hidden;

                ListBoxItem item = new ListBoxItem();
                item.Content = BrowserWebsites.Text; // + " (" + SelectedProcessName + ")";
                WebsitesBlacklistList.Items.Add(item);

                //Call to transmit blacklisted website to browser extension
                SyncToXML(BrowserWebsites.Text, false, "add");
                AddWebsiteToSaves(BrowserWebsites.Text);

                BrowserWebsites.Clear();
            }
        }

        //Remove selected website entry from blacklist
        private void RemoveFromWebsitesBlacklist_Click(object sender, RoutedEventArgs e)
        {

            Settings.Default.BlacklistedWebsites.Remove(SelectedWebsite.ToString());
            Settings.Default.Save();
            SyncToXML(SelectedWebsite, false, "remove");

            ReloadWebsitesBlacklistList();

            SelectedWebsite = "";
            RemoveFromWebsitesBlacklist.IsEnabled = false;

            //Deselect listbox because item is removed
            Deselecting = true;
            WebsitesBlacklistList.UnselectAll();
            Deselecting = false;

            if (WebsitesBlacklistList.Items.Count <= 0)
            {
                NoWebsitesBlacklistPlaceholderText.Visibility = Visibility.Visible;
            }
        }

        //Obtain currently selected already blacklisted website listbox entry on selection change
        private void WebsitesBlacklistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Not when listbox selection changes from codebehind (eg after removing website entry from listbox)
            if (!Deselecting)
            {
                ListBoxItem lbi = WebsitesBlacklistList.SelectedItem as ListBoxItem;
                SelectedWebsite = lbi.Content.ToString();
                RemoveFromWebsitesBlacklist.IsEnabled = true;
            }
        }

        //Reload website blacklist after entry removal
        private void ReloadWebsitesBlacklistList()
        {
            Deselecting = true;
            BlacklistedWebsites = Settings.Default.BlacklistedWebsites.Cast<string>().ToList();
            WebsitesBlacklistList.Items.Clear();
            foreach (string name in BlacklistedWebsites)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = name;
                WebsitesBlacklistList.Items.Add(item);
            }
            Deselecting = false;
        }

        //Handle behaviour when pen icon next to programs/websites tab is clicked
        private void EditWebsites_Click(object sender, RoutedEventArgs e)
        {
            AnimateUp(ProgramsBlacklistBorder);
            ProgramsBlacklistBorder.Visibility = Visibility.Hidden;
            WebsitesBlacklistBorder.Visibility = Visibility.Visible;
            AnimateDown(WebsitesBlacklistBorder);
        }
        private void EditPrograms_Click(object sender, RoutedEventArgs e)
        {
            AnimateUp(WebsitesBlacklistBorder);
            WebsitesBlacklistBorder.Visibility = Visibility.Hidden;
            ProgramsBlacklistBorder.Visibility = Visibility.Visible;
            AnimateDown(ProgramsBlacklistBorder);
        }

        //Animations for programs and websites blacklist area appearance
            //Margins are changed with easing function to achieve smooth animation
        private void AnimateDown(Border border)
        {
            ThicknessAnimation animateMargin = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            animateMargin.EasingFunction = cubicEase;
            border.BeginAnimation(MarginProperty, animateMargin);
            AnimateDown(SubBlacklistArea);
        }
        private void AnimateUp(Border border)
        {
            ThicknessAnimation animateMargin = new ThicknessAnimation(new Thickness(0, -175, 0, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            animateMargin.EasingFunction = cubicEase;
            border.BeginAnimation(MarginProperty, animateMargin);
            AnimateUp(SubBlacklistArea);
        }
        private void AnimateDown(StackPanel stackpanel)
        {
            ThicknessAnimation animateMargin = new ThicknessAnimation(new Thickness(0, -10, 0, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            animateMargin.EasingFunction = cubicEase;
            stackpanel.BeginAnimation(MarginProperty, animateMargin);
        }
        private void AnimateUp(StackPanel stackpanel)
        {
            ThicknessAnimation animateMargin = new ThicknessAnimation(new Thickness(0, -120, 0, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            animateMargin.EasingFunction = cubicEase;
            stackpanel.BeginAnimation(MarginProperty, animateMargin);
        }

        //Handle if programs and/or websites are selected to be supervised
        private void FilterWebsites_Checked(object sender, RoutedEventArgs e)
        {
            GlobalVariables.BrowserClose = FilterWebsites.IsChecked.Value;
            GlobalVariables.CheckBrowser = FilterWebsites.IsChecked.Value;
            FilteringTextMsg();
            CheckBox();
        }
        private void FilterPrograms_Checked(object sender, RoutedEventArgs e)
        {
            GlobalVariables.CheckPrograms = FilterPrograms.IsChecked.Value;
            CheckBox();
            FilteringTextMsg();
        }

        //Change UI information accordingly
        private void FilteringTextMsg()
        {
            if (!GlobalVariables.CheckPrograms && !GlobalVariables.CheckBrowser)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting...";
            }
            else if (GlobalVariables.CheckPrograms && !GlobalVariables.CheckBrowser)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting programs.";
            }
            else if (GlobalVariables.CheckBrowser && !GlobalVariables.CheckPrograms)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting websites.";
            }
            else if (GlobalVariables.CheckPrograms && GlobalVariables.CheckBrowser)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting programs and websites.";
            }
        }

        //UI checkbox next to programs and websites tabs
        private void CheckBox()
        {
            if (GlobalVariables.CheckPrograms)
            {
                Programs_circle_checked.Visibility = Visibility.Visible;
                Programs_circle.Visibility = Visibility.Hidden;
            }
            else if (!GlobalVariables.CheckPrograms)
            {
                Programs_circle.Visibility = Visibility.Visible;
                Programs_circle_checked.Visibility = Visibility.Hidden;
            }
            if (GlobalVariables.CheckBrowser)
            {
                Websites_circle_checked.Visibility = Visibility.Visible;
                Websites_circle.Visibility = Visibility.Hidden;
            }
            else if (!GlobalVariables.CheckBrowser)
            {
                Websites_circle.Visibility = Visibility.Visible;
                Websites_circle_checked.Visibility = Visibility.Hidden;
            }
        }
    }
}