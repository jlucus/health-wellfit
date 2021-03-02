using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using WellFitPlus.WebAPI.Models;
using WellFitPlus.Common;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;

namespace WellFitPlus.WebAPI.Controllers
{
    [RoutePrefix("api/setting")]
    public class SettingController : ApiController
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(VideoController));
        SettingRepository _settingRepo = new SettingRepository();

        [HttpPost]
        [Route("Add")]
        public bool Add(SettingView settingView)
        {
            try
            {
                Setting setting = new Setting();

                UpdateModel(ref setting, settingView);

                _settingRepo.Add(setting);

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
            return true;
        }

        [HttpPost]
        [Route("AddOrUpdate")]
        public bool AddOrUpdate(SettingView settingView)
        {
            try
            {
                bool result = false;

                // Get Setting
                Setting setting = _settingRepo.GetSettings(settingView.UserID);
                if (setting != null)
                {
                    // Edit Setting
                    this.Edit(settingView);
                    result = true;
                }
                else
                {
                    result = this.Add(settingView);
                }

                return result;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        [HttpPost]
        [Route("Edit")]
        public void Edit(SettingView settingView)
        {
            try
            {
                Setting setting = _settingRepo.GetSettings(settingView.UserID);
                if (setting != null)
                {

                    UpdateModel(ref setting, settingView);
                    _settingRepo.Edit(setting);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        [HttpPost]
        [Route("Get")]
        public SettingView GetSetting(SettingView settingView)
        {
            Setting setting = new Setting();

            try
            {
                setting = _settingRepo.GetSettings(settingView.UserID);

                settingView.CacheSize = setting.CacheSize;
                settingView.UserID = setting.UserID;
                settingView.Mute = setting.Mute;
                settingView.Reminders = settingView.Reminders;
                settingView.UserID = setting.UserID;
                settingView.VideoDelayTime = setting.VideoDelayTime;
                settingView.WellFitEmails = setting.WellFitEmails;
                settingView.WiFiDownloadOnly = setting.WiFiDownloadOnly;
                settingView.RolloverDate = setting.RolloverDate;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return settingView;
        }

        private void UpdateModel(ref Setting settingModel, SettingView settingView)
        {

            settingModel.CacheSize = settingView.CacheSize;
            settingModel.Id = settingView.UserID;
            settingModel.Mute = settingView.Mute;
            settingModel.Reminders = settingView.Reminders;
            settingModel.UserID = settingView.UserID;
            settingModel.VideoDelayTime = settingView.VideoDelayTime;
            settingModel.WellFitEmails = settingView.WellFitEmails;
            settingModel.WiFiDownloadOnly = settingView.WiFiDownloadOnly;
            settingModel.RolloverDate = settingView.RolloverDate;
        }
    }
}
