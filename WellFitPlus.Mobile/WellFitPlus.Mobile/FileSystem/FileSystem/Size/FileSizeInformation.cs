using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellFitMobile.FileSystem.FileSystem.Size
{
    /// <summary>
    /// This class holds file system object size information
    /// </summary>
    public class FileSizeInformation : IFileSizeInformation
    {

        #region Properties

        private decimal m_Bytes = 0;
        /// <summary>
        /// The size of the file in Bytes
        /// </summary>
        public decimal Bytes
        {
            get
            {
                // Validation
                if (this.m_Bytes <= 0) { return 0; }
                
                return this.m_Bytes;
            }
        }
        
        /// <summary>
        /// The size of the file in KiloBytes
        /// </summary>
        public decimal KiloBytes
        {
            get
            {
                // Validation
                if (this.Bytes <= 0) { return 0; }
                
                return this.Bytes / 1024;
            }
        }
        
        /// <summary>
        /// The size of the file in MegaBytes
        /// </summary>
        public decimal MegaBytes
        {
            get
            {
                // Validation
                if (this.KiloBytes <= 0) { return 0; }

                return this.KiloBytes / 1024;
            }
        }
        
        /// <summary>
        /// The size of the file in GigaBytes
        /// </summary>
        public decimal GigaBytes
        {
            get
            {
                // Validation
                if (this.MegaBytes <= 0) { return 0; }

                return this.MegaBytes / 1024;
            }
        }

        #endregion

        #region Initialization

        internal FileSizeInformation(decimal decBytes)
        {
            this.m_Bytes = decBytes;
        }

        #endregion
    }
}
