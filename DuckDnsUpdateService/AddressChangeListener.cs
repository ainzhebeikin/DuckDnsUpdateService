using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace DuckDnsUpdateService
{
    internal delegate void AddressChangedEventHandler(string newAddress);

    internal class AddressChangeListener : IDisposable
    {
        private readonly string _interfaceName;

        private IPAddress _lastAddress;

        public AddressChangeListener(string interfaceName)
        {
            _interfaceName = interfaceName;
            _lastAddress = IPAddress.None;
        }

        public void Start()
        {
            LogNetworkInterfaces();
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        }

        public event AddressChangedEventHandler AddressChanged;

        public void Dispose()
        {
            NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
        }

        private void OnNetworkAddressChanged(object sender, EventArgs eventArgs)
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
                _lastAddress = ipAddressInformation.Address;
                if (null != AddressChanged)
                {
                    AddressChanged(_lastAddress.ToString());
                }
                break;
            }
        }
    }
}