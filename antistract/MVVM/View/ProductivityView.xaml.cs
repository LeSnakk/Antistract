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

namespace antistract.MVVM.View
{
    /// <summary>
    /// Interaction logic for ProductivityView.xaml
    /// </summary>
    public partial class ProductivityView : UserControl
    {
        BackgroundWorker bgWorker = new BackgroundWorker();
        public List<RegistryKey> installedPrograms = new List<RegistryKey>();

        public ProductivityView()
        {
            InitializeComponent();
            bgWorker.DoWork += BgWorker_DoWork;
        }

        

        void PrintText(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            tb.Text = "   You selected " + lbi.Content.ToString() + ".";
        }

        public void GetInstalledPrograms(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();

            //var installedPrograms = new List<RegistryKey>();

            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {

                            var displayName = sk.GetValue("DisplayName");
                            var size = sk.GetValue("EstimatedSize");

                            Debug.WriteLine(displayName);

                            ListBoxItem item;
                            if (displayName != null)
                            {
                                installedPrograms.Add(sk);

                                //Debug.WriteLine(sk.GetValue("DisplayName"));
                                listBox.Items.Add(sk.GetValue("DisplayName"));
                            }

                            //Debug.WriteLine("Length List: " + installedPrograms.Count + "\nLength Displayed: " + listBox.Items.Count);

                        }
                        catch (Exception ex)
                        { }
                    }
                }

                

                //label1.Text += " (" + lstDisplayHardware.Items.Count.ToString() + ")";
            }


        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            

        }

        private void startChecking(object sender, RoutedEventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            
            while (isChecked() == false)
            {
                Process[] processes = Process.GetProcessesByName("Taskmgr");
                if (processes.Length == 0)
                {
                    Debug.WriteLine("TaskManager is not running");
                }
                else if (processes.Length >= 1)
                {
                    Debug.WriteLine("TaskManager is running");
                }
            }
            return;
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

    }


}
