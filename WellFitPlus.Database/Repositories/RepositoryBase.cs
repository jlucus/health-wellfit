using System;
using System.Data.Entity;

namespace WellFitPlus.Database.Repositories {
    public abstract class RepositoryBase : IDisposable {
        protected DbContext _context;

        public RepositoryBase() {
        }

        protected void Dispose(bool disposing) {
            if (disposing) {
                if (_context != null) {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static void LogSQL(string sql) {
            System.Diagnostics.Debug.Write(sql);
        }
    }
}
