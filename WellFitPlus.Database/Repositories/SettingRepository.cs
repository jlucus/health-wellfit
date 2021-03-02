using log4net;
using System;
using System.Linq;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {
    public class SettingRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(SettingRepository));

        public static Setting GetNewDefaultSettings() {
            return new Setting() {
                Mute = false,
                WiFiDownloadOnly = true,
                CacheSize = 250,
                VideoDelayTime = 10,
                Reminders = true,
                WellFitEmails = false,
                RolloverDate = DateTime.Now
            };
        }

        public Guid Add(Setting setting) {
            try {

                _context.Settings.Add(setting);
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
                return Guid.Empty;
            }
            return setting.Id;
        }

        public void Edit(Setting setting) {

            try {

                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        public Setting GetSettings(Guid userID) {
            Setting setting = new Setting();

            try {
                setting = _context.Settings.Where(s => s.UserID == userID).FirstOrDefault();
            } catch (Exception ex) {
                log.Error(ex);
            }
            return setting;
        }

    }
}
