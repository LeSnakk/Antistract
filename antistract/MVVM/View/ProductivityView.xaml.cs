using Microsoft.Win32;
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

namespace antistract.MVVM.View
{
    /// <summary>
    /// Interaction logic for ProductivityView.xaml
    /// </summary>
    public partial class ProductivityView : UserControl
    {
        public ProductivityView()
        {
            InitializeComponent();
        }

        void PrintText(object sender, SelectionChangedEventArgs args)
        {
            ListBoxItem lbi = ((sender as ListBox).SelectedItem as ListBoxItem);
            tb.Text = "   You selected " + lbi.Content.ToString() + ".";
        }

        public void GetInstalledPrograms(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();

            var installedPrograms = new List<RegistryKey>();

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
            Debug.WriteLine(lbi.Content.ToString());

        }
    }


}
