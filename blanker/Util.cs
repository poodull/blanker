using log4net;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace blanker
{
    class Util
    {
        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<string> ShowNetworkInterfaces()
        {
            List<string> ret = new List<string>();

            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            _log.DebugFormat("Interface information for {0}.{1}     ",
                    computerProperties.HostName, computerProperties.DomainName);

            if (nics == null || nics.Length < 1)
            {
                _log.Error("  No network interfaces found.");
                return null;
            }

            _log.DebugFormat("  Number of interfaces .................... : {0}", nics.Length);

            foreach (NetworkInterface adapter in nics)
            {
                _log.Debug(adapter.Description);
                _log.Debug(String.Empty.PadLeft(adapter.Description.Length, '='));
                _log.DebugFormat("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                StringBuilder mac = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    // Display the physical address in hexadecimal.
                    mac.AppendFormat("{0}", bytes[i].ToString("X2"));
                    // Insert a hyphen after each byte, unless we are at the end of the
                    // address.
                    if (i != bytes.Length - 1)
                    {
                        mac.Append(":");
                    }
                }
                if (!string.IsNullOrWhiteSpace(mac.ToString()))
                {
                    ret.Add(mac.ToString().ToLower().Trim());
                }
                _log.DebugFormat("  Physical address ........................ : {0}", mac);
            }
            return ret;
        }
    }
}
