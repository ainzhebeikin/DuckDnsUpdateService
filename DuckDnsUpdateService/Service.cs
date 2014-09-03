using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using NLog;

namespace DuckDnsUpdateService
{
    partial class Service : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Switch _switch = new Switch();

        private AddressChangeListener _addressChangeListener;

        public Service()
        {
            InitializeComponent();
            CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            if (_switch.TurnOn())
            {
                try
                {
                    ProtectConfiguration("Token");
                    _addressChangeListener = new AddressChangeListener(ConfigurationManager.AppSettings["InterfaceName"]);
                    _addressChangeListener.AddressChanged += OnAddressChanged;
                    _addressChangeListener.Start();
                    _logger.Info("Started");
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Failed to start", e);
                    Stop();
                }
            }
        }

        private void ProtectConfiguration(params string[] keys)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var changed = keys.Any(key =>
            {
                if (ConfigurationManager.AppSettings[key].IsProtected())
                {
                    return false;
                }

                ConfigurationManager.AppSettings[key] = ConfigurationManager.AppSettings[key].Protect();
                configuration.AppSettings.Settings.Remove(key);
                configuration.AppSettings.Settings.Add(key, ConfigurationManager.AppSettings[key]);
                return true;
            });
            if (changed)
            {
                _logger.Info("Protecting configuration...");
                configuration.Save();
            }
        }

        private void OnAddressChanged(string newAddress)
        {
            _logger.Info("New address: {0}", newAddress);
            var success = DuckDnsUpdater.Update(ConfigurationManager.AppSettings["Domain"], ConfigurationManager.AppSettings["Token"].Unprotect(), newAddress);
            if (success)
            {
                _logger.Info("DNS update successful");
            }
            else
            {
                _logger.Warn("DNS update failed");
            }
        }

        protected override void OnStop()
        {
            if (_switch.TurnOff())
            {
                if (null != _addressChangeListener)
                {
                    _addressChangeListener.AddressChanged -= OnAddressChanged;
                    _addressChangeListener.Dispose();
                    _addressChangeListener = null;
                }
                _logger.Info("Stopped");
            }
        }
    }
}