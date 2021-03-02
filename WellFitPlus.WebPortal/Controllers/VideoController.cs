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
using System.Configuration;
using WellFitPlus.WebPortal.Attributes;
//using System.Web;
//using System.Web.Http;

using WellFitPlus.WebPortal.Views;

namespace WellFitPlus.WebPortal.Controllers {

    //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
    [AuthorizeRoles("admin")]
    public class VideoController : Controller {

        const string VIDEO_DIRECTORY = "~/Content/Video";

        // POST: /video/MainPage
        public ActionResult MainPage() {

            List<VideoViewModel> videoList = new List<VideoViewModel>();
            List<Video> videos = new List<Video>();
            
            try
            {
                VideoRepository _videoRepo = new VideoRepository();
        
                videos = _videoRepo.GetVideos()
                    .Where(video => video.Deleted == false).ToList();

                foreach (Video vid in videos)
                {
                    VideoViewModel vidView = new VideoViewModel();

                    vidView.Active = vid.Active;
                    vidView.DateModified = vid.DateModified;
                    vidView.DateUploaded = vid.DateUploaded;
                    vidView.Description = vid.Description;
                    vidView.Path = vid.Path;
                    vidView.Tags = vid.Tags;
                    vidView.Title = vid.Title;
                    vidView.Type = vid.Type.ToString();
                    vidView.ID = vid.Id;

                    videoList.Add(vidView);
                }
                videoList = videoList.OrderBy(v => v.Title).ToList();
            }
            catch (Exception ex)
            {
                //log.Error(ex);
            }
            
            return View(videoList);
        }
        
        [HttpPost]
        public ActionResult LoadVideo(HttpPostedFileBase fileToUpload) {

            // Verify that the user selected a file
            if (fileToUpload != null && fileToUpload.ContentLength > 0) {
                // extract only the filename
                var fileName = Path.GetFileName(fileToUpload.FileName);

                // Get the file path to local server
                var path = Path.Combine(Server.MapPath(VIDEO_DIRECTORY));
                string abspath = ConfigurationManager.AppSettings["videoURL"].ToString() + "/" + fileName;

                VideoRepository _videoRepo = new VideoRepository();

                bool videoRecordExists = false;
                if (System.IO.File.Exists(path + "/" + fileName))
                {
                    // We need to check to make sure that the video record exists. If it does then
                    // Save the video in case it needs to be replaced and set the Deleted flag to false.

                    // TODO: There is a situation where the user could upload a different video with the same name
                    //       as a previous video record.
                    // A better solution to duplicate uploads should be found other than just skipping the upload
                    // and reusing the previous video for a video record.
                    
                    var video = _videoRepo.GetVideos().Where(v => v.Title == fileName).FirstOrDefault();
                    if (video != null) {
                        video.Deleted = false;
                        video.Active = true;
                        _videoRepo.Edit(video);
                        videoRecordExists = true;
                    }

                    // Save the video file.
                    fileToUpload.SaveAs(path + "\\" + fileName);
                }

                if (videoRecordExists == false) { 
                     // Add the new video record

                    fileToUpload.SaveAs(path + "\\" + fileName);

                    Video newVideo = new Video();

                    newVideo.Active = true;
                    newVideo.Description = "Enter a Description";
                    newVideo.DateModified = DateTime.Now;
                    newVideo.DateUploaded = DateTime.Now;
                    newVideo.Path = abspath;
                    newVideo.Title = fileToUpload.FileName;

                    _videoRepo.Add(newVideo);
                }
            }
            
            return RedirectToAction("MainPage");
        }
        
        [HttpPost]
        public JsonResult DeleteVideo(VideoViewModel video)
        {
            try
            {
                VideoRepository _videoRepo = new VideoRepository();
                Video _video = _videoRepo.GetVideo(video.ID);
                _video.Deleted = true;

                var videoShortName = _video.Path.Split('/').Last();
                var path = Path.Combine(Server.MapPath(VIDEO_DIRECTORY), videoShortName);

                if (System.IO.File.Exists(path))
                {
                    // We can't delete video records due to analytics. Previous activities use old video records for statistical
                    // purposes.
                    _videoRepo.Edit(_video);
                    
                }
                
                return Json(new { IsSuccess = true, Message = "Video Deleted" });
            }
            catch(Exception ex)
            {
                return Json(new { IsSuccess = false, Message = "Video was not Deleted. Detail: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ToggleVideoActive(VideoViewModel video) {

            VideoRepository _videoRepo = new VideoRepository();

            Video vid = _videoRepo.GetVideo(video.ID);
            vid.Active = !vid.Active;

            _videoRepo.Edit(vid);

            // redirect back to the index action to show the form once again
            return RedirectToAction("MainPage");
        }
    }
}
