using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using NLog;

namespace EmailIpAddressChange
{
    partial class Service : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Switch _switch = new Switch();

        private readonly string _interfaceName;

        private AddressChangeListener _addressChangeListener;

        public Service()
        {
            InitializeComponent();
            CanStop = true;
            _interfaceName = ConfigurationManager.AppSettings["InterfaceName"];
        }

        protected override void OnStart(string[] args)
        {
            if (_switch.TurnOn())
            {
                if (null == _interfaceName)
                {
                    _logger.Error("InterfaceName is not specified; aborting");
                    Stop();
                }
                else
                {
                    _addressChangeListener = new AddressChangeListener(_interfaceName);
                    _logger.Info("Started");
                }
            }
        }

        protected override void OnStop()
        {
            if (_switch.TurnOff())
            {
                if (null != _addressChangeListener)
                {
                    _addressChangeListener.Dispose();
                    _addressChangeListener = null;
                }
                _logger.Info("Stopped");
            }
        }
    }
}