using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NLog;

namespace EmailIpAddressChange
{
    internal class AddressChangeListener : IDisposable
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly string _interfaceName;

        private IPAddress _lastAddress;

        public AddressChangeListener(string interfaceName)
        {
            _interfaceName = interfaceName;
            _lastAddress = IPAddress.None;
            LogNetworkInterfaces();
            NetworkChange.NetworkAddressChanged += NetworkChangeOnNetworkAddressChanged;
        }

        public void Dispose()
        {
            NetworkChange.NetworkAddressChanged -= NetworkChangeOnNetworkAddressChanged;
        }

        private void NetworkChangeOnNetworkAddressChanged(object sender, EventArgs eventArgs)
        {
            LogNetworkInterfaces();
        }

        private void LogNetworkInterfaces()
        {
            foreach (var ipAddressInformation in NetworkInterface.GetAllNetworkInterfaces()
                .Where(@interface => string.Equals(@interface.Name, _interfaceName))
                .SelectMany(@interface => @interface.GetIPProperties().UnicastAddresses)
                .Where(ipAddressInformation => !ipAddressInformation.Address.Equals(_lastAddress)))
            {
                _logger.Info(ipAddressInformation.Address);
                _lastAddress = ipAddressInformation.Address;
                break;
            }
        }
    }
}