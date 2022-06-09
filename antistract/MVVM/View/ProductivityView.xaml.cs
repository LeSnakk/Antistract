using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.ComponentModel;
using antistract.Core;
using antistract.MVVM.ViewModel;
using System.Windows.Controls.Primitives;

namespace antistract.MVVM.View
{
    /// <summary>
    /// Interaction logic for ProductivityView.xaml
    /// </summary>
    public partial class ProductivityView : UserControl
    {
        readonly MainWindow mainWndw = (MainWindow)Application.Current.MainWindow;

        BackgroundWorker bgWorker = new BackgroundWorker();
        Dictionary<String, String> programs = new Dictionary<String, String>();
        public List<RegistryKey> installedPrograms = new List<RegistryKey>();

        public ProductivityView()
        {
            InitializeComponent();
            FillPickPlanDropdown();
            bgWorker.DoWork += BgWorker_DoWork;
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
            tb.Text = "   You selected " + lbi.Content.ToString() + ".";

            ListBoxItem item = new ListBoxItem();
            item.Content = "hallo";
            l1.Items.Add(item);

        }

        public void GetInstalledPrograms(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();

            //var installedPrograms = new List<RegistryKey>();

            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"; //@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {

                            var displayName = sk.GetValue("DisplayName");
 
                            if (displayName != null)
                            {
                                //installedPrograms.Add(sk);
                                //listBox.Items.Add(sk.GetValue("DisplayName"));

                                foreach(String s in sk.GetValueNames())
                                {
                                    string d;
                                    //Debug.WriteLine(sk.GetValue("DisplayName - ") + s + ": " + (sk.GetValue(s)));
                                    if (sk.GetValueNames().Contains("DisplayIcon"))
                                    {
                                        d = "DisplayIcon";
                                    }
                                    else
                                    {
                                        d = "DisplayName";
                                    }
                                    programs.Add(sk.GetValue("DisplayName").ToString(), sk.GetValue(d).ToString());
                                }
                                Debug.WriteLine("\n\n");
                            }

                            //Debug.WriteLine("Length List: " + installedPrograms.Count + "\nLength Displayed: " + listBox.Items.Count);

                        }
                        catch (Exception ex)
                        { }
                    }
                }

                

                //label1.Text += " (" + lstDisplayHardware.Items.Count.ToString() + ")";
            }

            MakeTheList();

        }

        private void MakeTheList()
        {
            foreach (string s in programs.Keys)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = s;
                listBox.Items.Add(item);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string output;
            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);

            programs.TryGetValue(lbi.Content.ToString(), out output);

            tb.Text = output;

        }

        private void startChecking(object sender, RoutedEventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            
            while (isChecked() == false)
            {
                var names = new[] { "systemsettings", "winrar", "steam" };
                Process[] processes = names.SelectMany(name => Process.GetProcessesByName(name)).ToArray();
                if (processes.Length == 0)
                {
                    Debug.WriteLine("Notepad is not running");
                }
                else if (processes.Length >= 1)
                {
                    Debug.WriteLine("Notepad is running");
                    foreach (Process process in processes)
                    {
                        Debug.WriteLine("Closing...");
                        process.Kill();
                        
                    }
                }
            }
            Debug.WriteLine("Checking stopped");
        }

        public bool isChecked()
        {
            bool temp = false;
            this.Dispatcher.Invoke(() =>
            {
                temp = this.isCheckingg.IsChecked.Value;
            });
           return temp;
        }

        private void PickPlanDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PickPlanDropdownDefaultText.Visibility = Visibility.Hidden;
            PickPlanDropdown.Width = Double.NaN;

            if (GlobalVariables.PlanNames.Count < 1)
            {
                SwitchToPlansView();
            }
        }

        public void SwitchToPlansView()
        {
            mainWndw.MenuButtonPlans.IsChecked = true;
            GoToPlansViewButton.Command.Execute(null);
        }
    }


}
