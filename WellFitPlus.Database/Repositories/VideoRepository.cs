using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using WellFitPlus.Common;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {
    public class VideoRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(VideoRepository));

        public int Add(Video video) {
            try {
                _context.Videos.Add(video);
                return _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
                return -1;
            }
        }

        public int Edit(Video vid) {
            try {
                return _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
                return -1;
            }
        }

        public int Delete(Guid id) {
            try {
                Video video = _context.Videos.Where(v => v.Id == id).FirstOrDefault();

                _context.Videos.Remove(video);
                return _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
                return -1;
            }
        }

        public List<Video> GetVideos() {
            List<Video> vidList = new List<Video>();
            try {
                vidList = _context.Videos.OrderByDescending(v=>v.DateUploaded).ToList();

            } catch (Exception ex) {
                log.Error(ex);
            }
            return vidList;
        }
        
        public Video GetVideo(Guid id) {
            Video video = new Video();
            try {
                video = _context.Videos.Where(v => v.Id == id).FirstOrDefault();

            } catch (Exception ex) {
                log.Error(ex);
                return null;
            }
            return video;
        }
    }
}
