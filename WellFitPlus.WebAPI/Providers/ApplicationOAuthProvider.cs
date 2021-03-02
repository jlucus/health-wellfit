using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using WellFitPlus.WebAPI.Models;
using WellFitPlus.Database.Entities.Identity;
using WellFitPlus.Database.Repositories;

namespace WellFitPlus.WebAPI.Providers {
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId) {
            if (publicClientId == null) {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context) {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null) {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager, CookieAuthenticationDefaults.AuthenticationType);

            string companyName = string.Empty;
            var userProfile = new UserRepository().Get(Guid.Parse(user.Id));

            if (userProfile != null && userProfile.Company != null) {
                companyName = userProfile.Company.Name;
            }

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity,
                new AuthenticationProperties(new Dictionary<string, string>
                {
                    { "userName", user.UserName },
                    { "guid", user.Id.ToString() },
                    { "registrationDate", user.RegistrationDate.ToString() },
                    { "companyName", companyName }
                }));

            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context) {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary) {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context) {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null) {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context) {
            if (context.ClientId == _publicClientId) {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri) {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }
    }
}