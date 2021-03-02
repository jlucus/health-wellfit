using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http;
//using System.Web.Http.Controllers;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WellFitPlus.WebPortal.Attributes {
    public class AuthorizeRolesAttribute : AuthorizeAttribute {
        private string _role;

        public AuthorizeRolesAttribute(string role) {
            _role = role;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext) {
            bool authorize = false;
            
            string userRole = (string)httpContext.Session["UserRole"];
            if (userRole == _role) { 
                authorize = true; 
            }
    
            return authorize;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
            filterContext.Result = new HttpUnauthorizedResult();
        }
     
    }
}