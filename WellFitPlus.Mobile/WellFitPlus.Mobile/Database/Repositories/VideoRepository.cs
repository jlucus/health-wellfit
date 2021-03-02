using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WellFitPlus.Mobile.Models;

namespace WellFitPlus.Mobile.Database.Repositories
{
    public class VideoRepository 
    {
        private LocalDatabaseContext _context;

        public VideoRepository() {
            _context = new LocalDatabaseContext();

        }

        public int AddVideo(Video video)
        {
            int result = -1;
            try
            {
                _context.Lock.WaitOne();
                result = _context.DB.Insert(video);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return result;
        }
        
        public List<Video> GetVideos(DateTime? minimumLastPlayedDate = null)
        {
            List<Video> videos = null;
            try
            {
                _context.Lock.WaitOne();
                videos = _context.DB.Table<Video>()
                .OrderByDescending(v => v.LastPlayed)
                .ToList();

                if (minimumLastPlayedDate != null)
                {
                    videos = videos.Where(v => v.LastPlayed >= minimumLastPlayedDate).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return videos;
        }

		/// <summary>
		/// Gets a list of videos that has been successfully downloaded, has not be marked as deleted and has a 
		/// valid video file attached to it.
		/// </summary>
		/// <returns>The playable videos.</returns>
		public List<Video> GetPlayableVideos() {

			_context.Lock.WaitOne();
			var validVideos = _context.DB.Table<Video>().Where(v => v.DownloadedSuccessfully == true
														  && v.Deleted == false).ToList();
			
			_context.Lock.ReleaseMutex();


			var playableVideos = new List<Video>();

			foreach (var video in validVideos) {
				if (System.IO.File.Exists(video.GetFileNameWithFullPath()) == true) {
					playableVideos.Add(video);
				}
			}

			return playableVideos;
		}

		/// <summary>
		/// Gets a list of videos that are unwatched and playable. This will also handle ALL Bingo functionality
		/// by first checking if there are any videos that meet the above criteria of unwatched and playable.
		/// If there aren't any videos that meet the criteria then this method will mark ALL playable videos as
		/// unwatched to start the Bingo process over again.
		/// </summary>
		/// <returns>The fresh bingo videos.</returns>
		public List<Video> GetFreshBingoVideos() {

			List<Video> playableVideos = GetPlayableVideos();
			List<Video> freshBingoVideos = playableVideos.Where(v => v.IsWatched == false).ToList();

			if (freshBingoVideos.Count() == 0) {
				// We have no more unwatched videos. The Bingo has been fully used. Roll over the Bingo
				// by setting all playable Videos as watched

				foreach (var video in playableVideos) {
					video.IsWatched = false;
					UpdateVideo(video);
				}

				// All playable videos are fresh now since we have rolled over the bingo
				freshBingoVideos = playableVideos;
			}

			return freshBingoVideos; // Returned the filtered list of fresh bingo videos.
		}
        
        public List<Video> GetVideosNotWatchedSinceDownload()
        {
            List<Video> videos = null;
            try
            {
                _context.Lock.WaitOne();
                videos = this.GetVideos()
                    .Where(v =>
                        v.DownloadedSuccessfully == true  &&
                        v.Deleted == false &&
                        v.LastPlayed == DateTime.MinValue).ToList();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return videos;
        }

        public List<Video> GetVideosPreviouslyWatched()
        {
            List<Video> videos = null;
            Guid tempId = Guid.Empty;

            try
            {
                _context.Lock.WaitOne();
                var videosNotWatched = this.GetVideosNotWatchedSinceDownload();
                var activities = ActivitySessionRepository.Instance.GetActivities(DateTime.Today.AddDays(-14));
                
                videos = activities
                    .Join(videosNotWatched, a => a.VideoId, v => v.ID, (a, v) => new { Video =  v })
                    .Select(v => Guid.TryParse(v.Video.ID.ToString(), out tempId) == true ? (Video)v.Video : null)
                    .Where(v => v != null && v.ID != Guid.Empty).ToList();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return videos;
        }

        public List<Video> GetLastFivePlayedVideos()
        {
            List<Video> videos = null;
            try
            {
                _context.Lock.WaitOne();
                videos = _context.DB.Table<Video>()
				                 .Where(video => video.LastPlayed > DateTime.MinValue && video.ViewCount > 0 && 
				                        video.DownloadedSuccessfully == true && video.Deleted == false)
				                 .OrderByDescending(v => v.LastPlayed)
				                 .ToList();

				if (videos.Count <= 5)
                {
                    return videos;
                }

                return videos.Take(5).ToList();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return videos;
        }

        public Video GetVideo(Guid videoId)
        {
            Video video = null; 

            _context.Lock.WaitOne();

            video = _context.DB.Table<Video>()
                .Where(v => v.ID == videoId)
                .FirstOrDefault();

            _context.Lock.ReleaseMutex();

            return video;
        }
        
        public int UpdateVideo(Video video)
        {
            int result = -1;
            try
            {
                _context.Lock.WaitOne();
                result = _context.DB.Update(video);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return result;
        }

        public int InsertOrUpdateVideo(Video video)
        {
            int result = -1;
            try
            {                
               
                if (this.GetVideo(video.ID) != null)
                {
                    result = this.UpdateVideo(video);
                }
                else
                {
                    result = this.AddVideo(video);
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }
    }
}
