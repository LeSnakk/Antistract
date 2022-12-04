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
        private bool checkBrowser = false;
        private bool checkPrograms = false;

        ThreadStart loadInstalledPrograms = LoadInstalledPrograms;
        TimerWindow timerWindow;

        public ProductivityView()
        {
            InitializeComponent();
            FillPickPlanDropdown();
            bgWorker.DoWork += BgWorker_DoWork;
            FirstStartup();
            LoadBlacklistUserSave();
            //Debug_ClearUserSettings();
        }

        public void Debug_ClearUserSettings()
        {
            Settings.Default.BlacklistedPaths.Clear();
            Settings.Default.BlacklistedProcesses.Clear();
            Settings.Default.BlacklistedPrograms.Clear();
            Settings.Default.BlacklistedDisplayNames.Clear();
        }

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

        public void LoadBlacklistUserSave()
        {
            Debug.WriteLine(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

            //Debug.WriteLine("Current no of stored blacklisted programs: " + Settings.Default.BlacklistedDisplayNames.Count);

            BlacklistedPaths.Clear();
            namesList.Clear();
            DisplayBlacklistedNames.Clear();

            BlacklistedWebsites.Clear();

            BlacklistedPaths = Settings.Default.BlacklistedPaths.Cast<string>().ToList();
            namesList = Settings.Default.BlacklistedProcesses.Cast<string>().ToList();
            namesList.AddRange(Settings.Default.BlacklistedPrograms.Cast<string>().ToList());
            DisplayBlacklistedNames = Settings.Default.BlacklistedDisplayNames.Cast<string>().ToList();

            BlacklistedWebsites = Settings.Default.BlacklistedWebsites.Cast<string>().ToList();

            blacklistList.Items.Clear();
            WebsitesBlacklistList.Items.Clear();

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

        public void GetInstalledPrograms(object sender, RoutedEventArgs e)
        {
            paths.Clear();
            programs.Clear();

            loadInstalledPrograms += () =>
            {
                ShowListCallBack();
            };

            btn_CallLoad.Visibility = Visibility.Hidden;
            LoadingText.Visibility = Visibility.Visible;
            Thread thread = new Thread(loadInstalledPrograms) { IsBackground = true };
            thread.Start();
        }

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
            }
            Debug.WriteLine(programName + ": " + processName);
        }
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


        private void ShowListCallBack()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { ShowTheList(); }));
        }

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

        public static void startChecking()
        {
            StartChecking();
            TimerWindow.TimerOnHoldNO();
        }

        public static void StartChecking()
        {
            bgWorker.RunWorkerAsync();
        }


        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BlacklistedPaths.Clear();
            namesList.Clear();
            DisplayBlacklistedNames.Clear();

            BlacklistedPaths = Settings.Default.BlacklistedPaths.Cast<string>().ToList();
            namesList = Settings.Default.BlacklistedProcesses.Cast<string>().ToList();
            namesList.AddRange(Settings.Default.BlacklistedPrograms.Cast<string>().ToList());
            DisplayBlacklistedNames = Settings.Default.BlacklistedDisplayNames.Cast<string>().ToList();

            while (isChecked())
            {
                //higher = slower = lower CPU usage
                Thread.Sleep(500);

                Process[] processes = namesList.SelectMany(name => Process.GetProcessesByName(name)).ToArray();
                if (processes.Length == 0 && !checkBrowser && checkPrograms)
                {
                    TimerWindow.TimerOnHoldNO();
                    Debug.WriteLine("Notepad is not running");
                    Debug.WriteLine("!1");
                }
                else if (processes.Length == 00 && checkBrowser && GlobalVariables.OnlyPausing)
                {
                    CheckBrowser();
                    Debug.WriteLine("!2");
                }
                else if (processes.Length >= 1 && checkPrograms)
                {
                    Debug.WriteLine("Notepad is running");

                    foreach (Process process in processes)
                    {
                        Debug.WriteLine(process);
                        if (!process.HasExited)
                        {
                            try
                            {
                                {
                                    if (BlacklistedPaths.Any(path => process.MainModule.FileName.Replace("/", "\\").Substring(0, process.MainModule.FileName.Replace("/", "\\").LastIndexOf("\\") + 1).Contains(path)))
                                    {
                                        Debug.WriteLine(process.MainModule.FileName);
                                        Debug.WriteLine("Closing...");
                                        if (!GlobalVariables.OnlyPausing)
                                        {
                                            RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                                            TimerWindow.TimerOnHoldNO();
                                            Debug.WriteLine("!3");
                                            while (!process.HasExited)
                                            {
                                                process.Kill();
                                            }
                                        }
                                        else if (GlobalVariables.OnlyPausing)
                                        {
                                            RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
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

        private void CheckBrowser()
        {
            if (checkBrowser)
            {
                if (GlobalVariables.OnlyPausing)
                {
                    if (ReadGCExData())
                    {
                        RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                        TimerWindow.TimerOnHoldYES();
                        Debug.WriteLine("!2.1");
                        Debug.WriteLine("Chrome forbidden tab open");
                    }
                    else if (!ReadGCExData())
                    {
                        RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                        Debug.WriteLine("!2.2");
                        TimerWindow.TimerOnHoldNO();
                    }
                }
            }
        }

        public bool ReadGCExData()
        {
            try
            {
                using (var fs = new FileStream(GCExLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.ASCII))
                {
                    string temp = sr.ReadToEnd();
                    if (temp.Contains("-cT2A;z=YzW}f4ht/H6epiW2!Md*@,"))
                    {
                        return true;
                    }
                    else if (temp.Contains("8fj*d-*c@cP}+i3f%aB*BD#63amL*i"))
                    {
                        return false;
                    }
                    else
                    {
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

        private void PickPlanDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PickPlanDropdownDefaultText.Visibility = Visibility.Hidden;
            ComboBox pickPlanDropdown = (ComboBox)sender;

            if (GlobalVariables.PlanNames.Count < 1)
            {
                SwitchToPlansView();
            }
            else
            {
                ComboBoxItem selectedPlan = (ComboBoxItem)pickPlanDropdown.SelectedItem;
                CurrentlySelectedPlan.SelectedPlan = selectedPlan.Content.ToString();
                if (GlobalVariables.OnlyPausing != null && CheckMode != "" && Settings.Default.BlacklistedPrograms.Count != 0)
                {
                    Settings.Default.StartEnabled = true;
                    Settings.Default.Save();
                }
            }
        }

        public void SwitchToPlansView()
        {
            mainWndw.MenuButtonPlans.IsChecked = true;
            GoToPlansViewButton.Command.Execute(null);
        }

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

        private void Close_Program_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.OnlyPausing = false;
            CheckMode = "closing";
            if (CurrentlySelectedPlan.SelectedPlan != null && Settings.Default.BlacklistedPrograms.Count != 0)
            {
                Settings.Default.StartEnabled = true;
                Settings.Default.Save();
            }
            ModeText.Text = "Distracting processes will be automatically closed.";
        }

        private void Stop_Timer_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.OnlyPausing = true;
            CheckMode = "pausing";
            SetExtensionCheckModePausing();
            if (CurrentlySelectedPlan.SelectedPlan != null && Settings.Default.BlacklistedPrograms.Count != 0)
            {
                Settings.Default.StartEnabled = true;
                Settings.Default.Save();
            }
            ModeText.Text = "Distracting processes will interrupt your schedule.";
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            timerWindow = null;
            GlobalVariables.timerWindow = null;
            if (!GlobalVariables.TimerRunning)
            {
                //if (!String.IsNullOrWhiteSpace(BrowserWebsites.Text))
                //{
                TransmitToBrowserExtension();
                //}
                Debug.WriteLine("\n**********");
                Debug.WriteLine("Plan: " + CurrentlySelectedPlan.SelectedPlan);
                Debug.WriteLine("Prog: " + checkPrograms);
                Debug.WriteLine("Web.: " + checkBrowser);
                Debug.WriteLine("Mode: " + CheckMode);
                Debug.WriteLine("**********\n");
                timerWindow = new TimerWindow(CurrentlySelectedPlan.SelectedPlan);
                GlobalVariables.timerWindow = timerWindow;
                GlobalVariables.timerWindow.Show();
                GlobalVariables.TimerRunning = true;
                Settings.Default.StartEnabled = false;
                Settings.Default.BlacklistBlocked = true;
                Settings.Default.Save();
                //ToggleStartButton(false);
            }
        }

        private void TransmitToBrowserExtension()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("BrowserExtensions/Chrome/data.xml");

            XmlNodeList checkModeNodeList = doc.GetElementsByTagName("checkMode");
            XmlNode checkMode = checkModeNodeList[0];

            checkMode.InnerText = CheckMode;

            doc.Save("BrowserExtensions/Chrome/data.xml");

            if (!checkBrowser)
            {
                SetExtensionCheckModePausing();
            }
        }

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

        private void ToggleAddToBlacklistButton(bool isDisabled)
        {
            AddToBlacklist.IsEnabled = isDisabled;
        }

        private void ToggleRemoveFromBlacklistButton(bool isDisabled)
        {
            RemoveFromBlacklist.IsEnabled = isDisabled;
        }

        private void AddToBlacklist_Click(object sender, RoutedEventArgs e)
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

        private void RemoveFromBlacklist_Click(object sender, RoutedEventArgs e)
        {
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

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Deselecting)
            {
                string output;
                ListBoxItem lbi = listBox.SelectedItem as ListBoxItem;

                programs.TryGetValue(lbi.Content.ToString(), out output);

                SelectedProgramName = lbi.Content.ToString();
                SelectedProcessName = output;
            }
        }

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

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            ShouldCheckNo();
            GlobalVariables.TimerRunning = false;
            SetExtensionCheckModePausing();
            if (GlobalVariables.OnlyPausing != null && CheckMode != "" && Settings.Default.BlacklistedPrograms.Count != 0)
            {
                Settings.Default.StartEnabled = true;
                Settings.Default.Save();
            }
            Settings.Default.BlacklistBlocked = false;
            Settings.Default.Save();
            GlobalVariables.timerWindow.MainTimer.Stop();
            GlobalVariables.timerWindow.WasteTimer.Stop();
            GlobalVariables.timerWindow.Close();
        }

        private void AddToWebsitesBlacklist_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(BrowserWebsites.Text))
            {
                NoWebsitesBlacklistPlaceholderText.Visibility = Visibility.Hidden;
                WebsitesBlacklistList.IsEnabled = true;

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

                SyncToXML(BrowserWebsites.Text, false, "add");
                AddWebsiteToSaves(BrowserWebsites.Text);

                BrowserWebsites.Clear();
            }
        }

        private void RemoveFromWebsitesBlacklist_Click(object sender, RoutedEventArgs e)
        {

            Settings.Default.BlacklistedWebsites.Remove(SelectedWebsite.ToString());
            Settings.Default.Save();
            SyncToXML(SelectedWebsite, false, "remove");

            ReloadWebsitesBlacklistList();

            SelectedWebsite = "";
            RemoveFromWebsitesBlacklist.IsEnabled = false;
            Deselecting = true;
            WebsitesBlacklistList.UnselectAll();
            Deselecting = false;

            if (WebsitesBlacklistList.Items.Count <= 0)
            {
                NoWebsitesBlacklistPlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private void WebsitesBlacklistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Deselecting)
            {
                ListBoxItem lbi = WebsitesBlacklistList.SelectedItem as ListBoxItem;
                SelectedWebsite = lbi.Content.ToString();
                RemoveFromWebsitesBlacklist.IsEnabled = true;
            }
        }

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

        private void FilterWebsites_Checked(object sender, RoutedEventArgs e)
        {
            GlobalVariables.BrowserClose = FilterWebsites.IsChecked.Value;
            checkBrowser = FilterWebsites.IsChecked.Value;
            FilteringTextMsg();
            CheckBox();
        }

        private void FilterPrograms_Checked(object sender, RoutedEventArgs e)
        {
            checkPrograms = FilterPrograms.IsChecked.Value;
            CheckBox();
            FilteringTextMsg();
        }

        private void FilteringTextMsg()
        {
            if (!checkPrograms && !checkBrowser)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting...";
            }
            else if (checkPrograms && !checkBrowser)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting programs.";
            }
            else if (checkBrowser && !checkPrograms)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting websites.";
            }
            else if (checkPrograms && checkBrowser)
            {
                FilteringText.Text = "To help you focus and increase your productivity, this will keep an eye on your distracting programs and websites.";
            }
        }

        private void CheckBox()
        {
            if (checkPrograms)
            {
                Programs_circle_checked.Visibility = Visibility.Visible;
                Programs_circle.Visibility = Visibility.Hidden;
            }
            else if (!checkPrograms)
            {
                Programs_circle.Visibility = Visibility.Visible;
                Programs_circle_checked.Visibility = Visibility.Hidden;
            }
            if (checkBrowser)
            {
                Websites_circle_checked.Visibility = Visibility.Visible;
                Websites_circle.Visibility = Visibility.Hidden;
            }
            else if (!checkBrowser)
            {
                Websites_circle.Visibility = Visibility.Visible;
                Websites_circle_checked.Visibility = Visibility.Hidden;
            }
        }
    }
}