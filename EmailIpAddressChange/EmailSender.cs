using System;
using System.Net;
using System.Net.Mail;
using NLog;

namespace EmailIpAddressChange
{
    internal class EmailSender : IDisposable
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly SmtpClient _smtpClient;

        public EmailSender(string username, string password)
        {
            _smtpClient = new SmtpClient
            {
                Credentials = new NetworkCredential(username, password)
            };
        }

        public void Send(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                Subject = subject,
                Body = body
            };
            message.To.Add(to);

            try
            {
                _smtpClient.SendAsync(message, null);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Failed to send email", e);
            }
        }

        public void Dispose()
        {
            _smtpClient.SendAsyncCancel();
            _smtpClient.Dispose();
        }
    }
}