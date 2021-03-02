using System;
using System.Linq;
using System.Collections.Generic;
using WellFitPlus.Mobile.Models;

namespace WellFitPlus.Mobile.Database.Repositories
{
	public class FrequentVideoRepository
	{

		#region Static Fields
		private static FrequentVideoRepository _instance;
		#endregion

		#region Static Properties
		public static FrequentVideoRepository Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new FrequentVideoRepository();
				}
				return _instance;
			}
		}
		#endregion

		private readonly LocalDatabaseContext _context;

		private FrequentVideoRepository()
		{
			_context = new LocalDatabaseContext();
		}

		public void AddFrequentVideo(FrequentVideo video)
		{
			if (video.Id == 0)
			{
				_context.DB.Insert(video);
			}
		}

		public void AddFrequentVideos(List<FrequentVideo> videos) {

			var newVideos = videos.Where(v => v.Id == 0).ToList();

			_context.DB.InsertAll(newVideos);
		}

		public List<FrequentVideo> GetFrequentVideos()
		{
			return _context.DB.Table<FrequentVideo>().OrderBy(v => v.Id).ToList();
		}

		public void DeleteAllRecords()
		{
			_context.DB.DeleteAll<FrequentVideo>();
		}
	}
}

