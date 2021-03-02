using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WellFitPlus.WebAPI.Models
{
    public class RegisterCompanyUserRequest
    {
        public Guid UserId { get; set; }
        public string GroupCode { get; set; }
    }
}