using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WellFitPlus.WebAPI.Models
{
    public class DeregisterUserRequest
    {
        public Guid UserId { get; set; }
    }
}