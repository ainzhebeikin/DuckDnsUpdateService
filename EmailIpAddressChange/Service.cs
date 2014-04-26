using System;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using NLog;

namespace EmailIpAddressChange
{
    partial class Service : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Switch _switch = new Switch();

        public Service()
        {
            InitializeComponent();
            CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            if (_switch.TurnOn())
            {
                _logger.Info("Started");
                LogNetworkInterfaces();
                NetworkChange.NetworkAddressChanged += NetworkChangeOnNetworkAddressChanged;
            }
        }

        protected override void OnStop()
        {
            if (_switch.TurnOff())
            {
                NetworkChange.NetworkAddressChanged -= NetworkChangeOnNetworkAddressChanged;
                _logger.Info("Stopped");
            }
        }

        private void NetworkChangeOnNetworkAddressChanged(object sender, EventArgs eventArgs)
        {
            LogNetworkInterfaces();
        }

        private void LogNetworkInterfaces()
        {
            foreach (var @interface in NetworkInterface.GetAllNetworkInterfaces())
            {
                _logger.Info("Id = {0}, Name = {1}, Type = {2}, Status = {3}, Speed = {4}",
                             @interface.Id,
                             @interface.Name,
                             @interface.NetworkInterfaceType,
                             @interface.OperationalStatus,
                             @interface.Speed);
                foreach (var address in @interface.GetIPProperties().UnicastAddresses)
                {
                    _logger.Info("Unicast address family = {0}, address = {1}",
                                 address.Address.AddressFamily,
                                 address.Address);
                }
            }
        }
    }
}