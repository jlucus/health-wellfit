using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

using System.Web.Http;

using log4net;
using WellFitPlus.WebAPI.Models;
using WellFitPlus.Common;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using System.Net.Http.Headers;
using Microsoft.AspNet.Identity;
using WellFitPlus.WebAPI.Controllers;
using WellFitPlus.WebAPI.BindingModels;
using WellFitPlus.WebAPI.Helpers;

namespace WellFitPlus.WebAPI.Controllers
{
    [RoutePrefix("api/Message")]
    public class MessageController : ApiController
    {

        private MessageRepository _messageRepo = new MessageRepository();

        #region API Methods
        [Authorize]
        [HttpGet]
        [Route("GetMessages")]
        public List<Message> GetMessages()
        {
            return _messageRepo.GetMessages();

        }


        #endregion
    }
}
