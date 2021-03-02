using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Web.Hosting;
using WellFitPlus.Database;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebPortal.Models;

using WellFitPlus.WebPortal.Views;
using WellFitPlus.WebPortal.Attributes;

namespace WellFitPlus.WebPortal.Controllers {
    [AuthorizeRoles("admin")]
    public class MessageController : Controller {

        // POST: /video/MainPage
        public ActionResult MessageMainPage() {

            List<MessageViewModel> messageList = new List<MessageViewModel>();
            List<Message> messages = new List<Message>();
            try
            {
                MessageRepository _messageRepo = new MessageRepository();

                messages = _messageRepo.GetMessages();

                foreach (Message message in messages)
                {
                    MessageViewModel messageView = new MessageViewModel();
                    messageView.Description = message.Description;
                    messageView.ID = message.Id;

                    messageList.Add(messageView);
                }

            }
            catch (Exception ex)
            {
                //log.Error(ex);
            }

            return View(messageList);
        }

        [HttpPost]
        public ActionResult AddMessage(MessageViewModel messageView) {

            try {

                MessageRepository _messageRepo = new MessageRepository();

                Message message = new Message();

                message.Description = messageView.Description;
                TempData["Message"] = _messageRepo.Add(message);

            } catch (Exception ex) {
                TempData["Message"] = ex.Message;
                throw new Exception(ex.Message);
            }
            // redirect back to the index action to show the form once again
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("MessageMainPage", "Message");
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public ActionResult DeleteMessage(MessageViewModel message) {

            MessageRepository _messageRepo = new MessageRepository();

            _messageRepo.Delete(message.ID);

            // redirect back to the index action to show the form once again
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("MessageMainPage", "Message");
            return Json(new { Url = redirectUrl });
        }

    }
}
