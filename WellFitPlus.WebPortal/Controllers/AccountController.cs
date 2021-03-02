using System;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Configuration;

//using System.Web;
//using System.Web.Http;

using WellFitPlus.WebPortal.Views;
using WellFitPlus.WebPortal.Models;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;


namespace WellFitPlus.WebPortal.Controllers {

    public class AccountController : Controller {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl) {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) {
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", model.Email),
                    new KeyValuePair<string, string>("password", model.Password)
                });

            using (var client = new HttpClient()) {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServerURI"].ToString());
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                var authPath = @"/API/token";
                var response = await client.PostAsync(authPath, content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode) {
                    var token = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    AuthToken auth = JsonConvert.DeserializeObject<AuthToken>(token);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

                    authPath = @"/API/api/Account/GetRole?userID=" + auth.UserID;

                    response = await client.GetAsync(authPath).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode) {
                        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var userRole = JsonConvert.DeserializeObject(json) as Newtonsoft.Json.Linq.JObject;

                        Session["UserRole"] = userRole["role"].ToString();
                    }

                    return RedirectToLocal(returnUrl);
                } else {
                    return View("Failure");
                }
            };
        }

        public ActionResult Logout() {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ForgotPassword
        public ActionResult ForgotPassword() {
            return View();
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation() {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code) {
            return code == null ? View("Error") : View();
        }

        private ActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

    }
}
