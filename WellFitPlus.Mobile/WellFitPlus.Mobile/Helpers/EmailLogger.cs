
using System;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

using Xamarin.Forms;
using WellFitPlus.Mobile.Abstractions;

namespace WellFitPlus.Mobile
{

	/// <summary>
	/// This class is used to debug hard to find errors. Things like production 
	/// </summary>
	public class EmailLogger
	{


		private static readonly string PREFS_LOG_KEY = "Email logger key";
		private static EmailLogger _instance;

		public static EmailLogger Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new EmailLogger();
				}

				return _instance;
			}
		}

		public static void SendSingleLogToEmail(string TAG, string log, string email = "rbarber+wfp@asgrp.com")
		{
			string platform = "";
#if __IOS__
			platform = "iOS";
#elif __ANDROID
			platform = "Android";
#endif

			string subject = "Well Fit Single Log";
			string body = TAG + ": " + platform + " " + log;


			//MailMessage mail = new MailMessage();
			//SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
			//mail.From = new MailAddress("rbarber.asg@gmail.com");
			//mail.To.Add(email);
			//mail.Subject = subject;
			//mail.Body = body;
			//SmtpServer.Port = 465;
			//SmtpServer.Credentials = new System.Net.NetworkCredential("rbarber.asg@gmail.com", "b710230b");
			//SmtpServer.EnableSsl = true;
			//ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
			//{
			//	return true;
			//};

			//SmtpServer.Send(mail);
			Xamarin.Forms.Device.OpenUri(new Uri("mailto:" + email + "?subject=" + subject + "&body=" + body));
		}

		private string _log = "";

		private EmailLogger()
		{

			//_log = GetSavedLog();

			if (_log == "")
			{
				_log += DateTime.Now.ToString() + "__;";
			}
		}

		public void AddLog(string log)
		{
			_log += log + "__;";

			//SaveLog(_log);
		}

		private void SaveLog(String log)
		{
			DependencyService.Get<IPreferences>().SetString(PREFS_LOG_KEY, log);
		}

		private String GetSavedLog()
		{
			String log = DependencyService.Get<IPreferences>().GetString(PREFS_LOG_KEY);

			return log == null ? "" : log;
		}

		public void ClearLogs()
		{
			_log = "";
			SaveLog(_log);
		}

		public void SendLogsToEmail(string emailAddress)
		{

			string[] data = _log.Split(new string[] { "__;" }, StringSplitOptions.None);
			string body = "";

			foreach (String log in data)
			{
				body += log + ",\n\r";
			}

			Xamarin.Forms.Device.OpenUri(new Uri("mailto:" + emailAddress + "?subject=Logs&body=" + body));

			ClearLogs();
		}



	}

}