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
            foreach (var @interface in NetworkInterface.GetAllNetworkInterfaces())
            {
                _logger.Info("Id = {0}, Name = {1}, Type = {2}, Status = {3}, Speed = {4}",
                             @interface.Id,
                             @interface.Name,
                             @interface.NetworkInterfaceType,
                             @interface.OperationalStatus,
                             @interface.Speed);
            }
        }
    }
}