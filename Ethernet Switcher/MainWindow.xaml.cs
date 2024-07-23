using System;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace Ethernet_Switcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadNetworkAdapters(false);
            LoadComputerName();
        }

        private void LoadNetworkAdapters(bool onlyEnabled)
        {
            try
            {
                cbAdapters.Items.Clear();
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if (onlyEnabled)
                    {
                        // 仅添加启用的网络适配器
                        if ((bool)mo["IPEnabled"])
                        {
                            cbAdapters.Items.Add(mo["Description"]);
                        }
                    }
                    else
                    {
                        // 添加所有网络适配器
                        cbAdapters.Items.Add(mo["Description"]);
                    }
                }

                if (cbAdapters.Items.Count > 0)
                {
                    cbAdapters.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading network adapters: " + ex.Message);
            }
        }

        private void LoadComputerName()
        {
            try
            {
                // 获取计算机名
                string computerName = Environment.MachineName;

                // 设置TextBox控件的Text属性以显示计算机名
                C_Name.Text = computerName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading computer name: " + ex.Message);
            }
        }

        private void chkAdapterswork(object sender, RoutedEventArgs e)
        {
            LoadNetworkAdapters(true);
        }

        private void chkAdaptersnotwork(object sender, RoutedEventArgs e)
        {
            LoadNetworkAdapters(false);
        }


        private void chkManual_Checked(object sender, RoutedEventArgs e)
        {
            txtIPAddress.IsEnabled = true;
            txtSubnetMask.IsEnabled = true;
            txtGateway.IsEnabled = true;
        }

        private void chkManual_Unchecked(object sender, RoutedEventArgs e)
        {
            txtIPAddress.IsEnabled = false;
            txtSubnetMask.IsEnabled = false;
            txtGateway.IsEnabled = false;
        }

        private void btnChangeIP_Click(object sender, RoutedEventArgs e)
        {
            string selectedAdapter = cbAdapters.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedAdapter))
            {
                MessageBox.Show("Please select an adapter.");
                return;
            }

            try
            {
                if (chkManual.IsChecked == true)
                {
                    string ipAddress = txtIPAddress.Text;
                    string subnetMask = txtSubnetMask.Text;
                    string gateway = txtGateway.Text;

                    if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(subnetMask) || string.IsNullOrEmpty(gateway))
                    {
                        MessageBox.Show("Please enter valid IP address, subnet mask, and gateway.");
                        return;
                    }

                    SetIP(selectedAdapter, ipAddress, subnetMask, gateway);
                }
                else
                {
                    SetDHCP(selectedAdapter);
                }

                MessageBox.Show("Network configuration has been successfully changed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing network configuration: " + ex.Message);
            }
        }

        private void SetIP(string adapterDescription, string ipAddress, string subnetMask, string gateway)
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    // Only change the specified network adapter
                    if ((bool)mo["IPEnabled"] && mo["Description"].ToString() == adapterDescription)
                    {
                        ManagementBaseObject newIP = mo.GetMethodParameters("EnableStatic");
                        ManagementBaseObject newGateway = mo.GetMethodParameters("SetGateways");

                        newIP["IPAddress"] = new string[] { ipAddress };
                        newIP["SubnetMask"] = new string[] { subnetMask };
                        newGateway["DefaultIPGateway"] = new string[] { gateway };
                        newGateway["GatewayCostMetric"] = new int[] { 1 };

                        mo.InvokeMethod("EnableStatic", newIP, null);
                        mo.InvokeMethod("SetGateways", newGateway, null);

                        MessageBox.Show($"The IP address of network adapter '{adapterDescription}' has been changed to {ipAddress}, subnet mask to {subnetMask}, and default gateway to {gateway}.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting IP address: " + ex.Message);
            }
        }

        private void SetDHCP(string adapterDescription)
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    // Only change the specified network adapter
                    if ((bool)mo["IPEnabled"] && mo["Description"].ToString() == adapterDescription)
                    {
                        mo.InvokeMethod("EnableDHCP", null);

                        MessageBox.Show($"The network adapter '{adapterDescription}' has been switched to automatic IP address acquisition (DHCP).");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting DHCP: " + ex.Message);
            }
        }
    }
}
