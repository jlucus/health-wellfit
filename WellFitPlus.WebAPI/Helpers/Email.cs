using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace WellFitPlus.WebAPI.Utility
{
    public class EmailUtility
    {
        private SmtpClient _smtpClient;
        private NetworkCredential _credential;
        private string _send_from_address;

        public EmailUtility()
        {
            // TODO: Obtaining mail server address and credential 
            string server_address = ConfigurationManager.AppSettings["MailServerAddress"].ToString();
            string server_port = ConfigurationManager.AppSettings["MailServerPort"].ToString();
            string server_SSL = ConfigurationManager.AppSettings["MailServerSSL"].ToString();
            
            int port_no = 0;
            if (int.TryParse(server_port, out port_no))
            {
                _smtpClient = new SmtpClient(server_address, port_no);
            }
            else
            {
                _smtpClient = new SmtpClient(server_address);
            }

            bool bSSL = false;
            if (bool.TryParse(server_SSL, out bSSL))
            {
                _smtpClient.EnableSsl = bSSL;
            }

            string username = ConfigurationManager.AppSettings["MailServerUsername"].ToString();
            string password = ConfigurationManager.AppSettings["MailServerPassword"].ToString();
            _credential = new NetworkCredential(username, password);

            _send_from_address = "no_reply@email.com";
        }

        public bool AsyncSendMessage(string destination, string subject, string body)
        {
            return Send(subject, body, (new List<string> { destination }), new List<string>());
        }

        private bool Send(string title, string content, List<string> addresses, List<string> attachments)
        {
            MailMessage messageToSend = new MailMessage();

            #region Generating Email
            try
            {
                // Set the sender's address
                messageToSend.From = new MailAddress(_send_from_address);

                // Send To addresses
                foreach (string toAddr in addresses)
                {
                    messageToSend.To.Add(new MailAddress(toAddr));
                }

                // Set the subject and message body text
                messageToSend.Subject = title;
                messageToSend.Body = content;
                messageToSend.IsBodyHtml = false;

                // Add attachment(s)
                foreach (string attachment in attachments)
                {
                    messageToSend.Attachments.Add(new Attachment(attachment));
                }


            }
            catch (Exception PreparingEmailErr)
            {
                // TODO: Log error message
                return false;
            }
            #endregion

            #region Sending Email
            try
            {
                SmtpClient client = _smtpClient;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = _credential;
                client.Send(messageToSend);
                return true;
            }
            catch (Exception SendEmailErr)
            {
                // TODO: Log error message
                return false;
            }
            #endregion
        }

    }
}
