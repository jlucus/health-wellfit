using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WellFitPlus.Common.BindingModels.Identity;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Entities.Identity;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebAPI.Models;

namespace WellFitPlus.WebAPI.Controllers.OAuth {
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController {
        private ApplicationUserManager _userManager;

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        public AccountController() {
        }

        public AccountController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat) {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = model.Password,
                Company = model.Company,
                RegistrationDate = DateTime.Now
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) {
                return GetErrorResult(result);
            }

            result = await UserManager.AddToRoleAsync(user.Id, model.Role);

            Company userCompany = null;

            if (!string.IsNullOrEmpty(model.GroupCode)) {
                userCompany = new CompanyRepository().GetByGroupCode(model.GroupCode);
            }

            UserProfile newProfile = new Database.Entities.UserProfile()
            {
                Id = Guid.Parse(user.Id),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CompanyId = userCompany?.Id
            };

            new UserRepository().Add(newProfile);

            Setting userSettings = SettingRepository.GetNewDefaultSettings();
            userSettings.UserID = newProfile.Id;

            new SettingRepository().Add(userSettings);

            return Ok();
        }

        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout() {
            // TODO:    logout doesn't expire the token, as OWIN doesn't keep track of issued tokens. to do so
            //          would require establishing a new table that tracks sessions and deleting the token on 
            //          the logout request. not important for the MVP target of this app
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo() {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null
            };
        }

        [Authorize]
        [Route("GetRole")]
        public UserProfileViewModel GetUserRole(Guid userID) {
            UserProfileViewModel um = new UserProfileViewModel();
            IList<string> srRoles = new List<string>();
            srRoles = UserManager.GetRoles(userID.ToString());
            um.Role = srRoles[0];
            um.UserID = userID;

            return um;
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded) {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [HttpPost]
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded) {
                return GetErrorResult(result);
            }

            return Ok();
        }

        #region Helper Methods
        private IAuthenticationManager Authentication {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result) {
            if (result == null) {
                return InternalServerError();
            }

            if (!result.Succeeded) {
                if (result.Errors != null) {
                    foreach (string error in result.Errors) {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid) {
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
        #endregion

        #region Helper Classes
        private class ExternalLoginData {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims() {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null) {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity) {
                if (identity == null) {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value)) {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer) {
                    return null;
                }

                return new ExternalLoginData {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits) {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0) {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);

                return HttpServerUtility.UrlTokenEncode(data);
            }
        }
        #endregion

        protected override void Dispose(bool disposing) {
            if (disposing && _userManager != null) {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}
