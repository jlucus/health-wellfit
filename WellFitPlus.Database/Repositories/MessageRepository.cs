using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using WellFitPlus.Database.Entities;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
namespace WellFitPlus.Database.Repositories {
    public class MessageRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(MessageRepository));

        public string Add(Message message) {
            string err;
            try {

                _context.Messages.Add(message);
                _context.SaveChanges();

            } 
            catch (DbUpdateException sqlex) {
                err = "SQL Exception " + sqlex.Message
                                    + "SQL Inner Exception " + sqlex.InnerException.Message
                                    + "SQL Inner Inner Exception " + sqlex.InnerException.InnerException.Message;

                foreach (var result in sqlex.Entries) {
                    err += " Record save unsuccessful sql inner " + result.GetType().Name;
                }

            } catch (Exception ex) {
                log.Error(ex);
                throw new Exception(ex.Message);
            }
            return "Pass";
        }

        public void Delete(Guid id) {
            try {
                Message message = _context.Messages.Where(v => v.Id == id).FirstOrDefault();

                _context.Messages.Remove(message);
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        public List<Message> GetMessages() {
            List<Message> messageList = new List<Message>();
            try {

                messageList = _context.Messages.ToList();
            } catch (Exception ex) {
                log.Error(ex);
            }
            return messageList;
        }
    }
}
