using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Auth;

namespace WellFitPlus.Mobile.Models
{
    public class UserCredentials
    {
        #region Properties

        public string UserName
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null) ? account.Username : null;
            }
        }

        public string Password
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(App.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["Password"] : null;
            }
        }
        #endregion

        #region Initialization

        public UserCredentials()
        {

        }

        #endregion

        #region Functions

        public void SaveCredentials(string userName, string password)
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                Account account = new Account
                {
                    Username = userName
                };
                account.Properties.Add("Password", password);
                AccountStore.Create().Save(account, App.AppName);
            }
        }

        public void DeleteCredentials()
        {
            try
            {
                // Sometimes more than one account is stored (for whatever reason). We need to remove all of them.
                var accountStore = AccountStore.Create();
                var accounts = accountStore.FindAccountsForService(App.AppName).ToList();

                for (int i = 0; i < accounts.Count; i++)
                {
                    if (accounts[i] != null)
                    {
                        accountStore.Delete(accounts[i], App.AppName);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
