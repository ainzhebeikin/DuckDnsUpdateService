using System;
using System.Configuration;
using System.ServiceProcess;
using NLog;

namespace EmailIpAddressChange
{
    partial class Service : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Switch _switch = new Switch();

        private AddressChangeListener _addressChangeListener;

        private EmailSender _emailSender;

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
                    ProtectCredentials();
                    _emailSender = new EmailSender(StringProtector.UnprotectString(ConfigurationManager.AppSettings["SmtpUsername"]),
                                                   StringProtector.UnprotectString(ConfigurationManager.AppSettings["SmtpPassword"]));
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

        private void ProtectCredentials()
        {
            if (!StringProtector.IsProtected(ConfigurationManager.AppSettings["SmtpUsername"]))
            {
                _logger.Info("Protecting credentials...");

                ConfigurationManager.AppSettings["SmtpUsername"] = StringProtector.ProtectString(ConfigurationManager.AppSettings["SmtpUsername"]);
                ConfigurationManager.AppSettings["SmtpPassword"] = StringProtector.ProtectString(ConfigurationManager.AppSettings["SmtpPassword"]);

                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings.Remove("SmtpUserName");
                configuration.AppSettings.Settings.Add("SmtpUserName", ConfigurationManager.AppSettings["SmtpUsername"]);
                configuration.AppSettings.Settings.Remove("SmtpPassword");
                configuration.AppSettings.Settings.Add("SmtpPassword", ConfigurationManager.AppSettings["SmtpPassword"]);
                configuration.Save();
            }
        }

        private void OnAddressChanged(string newAddress)
        {
            _logger.Info("New address: {0}", newAddress);
            _emailSender.Send(ConfigurationManager.AppSettings["EmailRecipient"], ConfigurationManager.AppSettings["EmailSubject"], newAddress);
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
                if (null != _emailSender)
                {
                    _emailSender.Dispose();
                    _emailSender = null;
                }
                _logger.Info("Stopped");
            }
        }
    }
}