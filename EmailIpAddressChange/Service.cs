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
                _emailSender = new EmailSender(StringProtector.UnprotectString(ConfigurationManager.AppSettings["SmtpUsername"]),
                                               StringProtector.UnprotectString(ConfigurationManager.AppSettings["SmtpPassword"]));
                _addressChangeListener = new AddressChangeListener(ConfigurationManager.AppSettings["InterfaceName"]);
                _addressChangeListener.AddressChanged += OnAddressChanged;
                _addressChangeListener.Start();
                _logger.Info("Started");
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