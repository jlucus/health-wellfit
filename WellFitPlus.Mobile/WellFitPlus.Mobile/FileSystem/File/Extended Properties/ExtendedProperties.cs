using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WellFitMobile.FileSystem.File.Entities;

namespace WellFitMobile.FileSystem.File.ExtendedProperties
{
    /// <summary>
    /// This class provides extended system file properties
    /// </summary>
    public class ExtendedProperties
    {

        #region Properties

        /// <summary>
        /// The file object these properties are attributed to
        /// </summary>
        /// <returns></returns>
        private FileObject m_File;
        
        private string m_ApplicationName = "";
        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return this.m_ApplicationName;
            }
        }

        private string m_Author = "";
        /// <summary>
        /// File author
        /// </summary>
        public string Author
        {
            get
            {
                return this.m_Author;
            }
        }

        private string m_ComputerName = "";
        /// <summary>
        /// Copmuter name
        /// </summary>
        public string ComputerName
        {
            get
            {
                return this.m_ComputerName;
            }
        }

        private string m_CopyRight = "";
        /// <summary>
        /// Copyright
        /// </summary>
        public string CopyRight
        {
            get
            {

                return this.m_CopyRight;
            }
        }

        private string m_Comments = "";
        /// <summary>
        /// File comments
        /// </summary>
        public string Comments
        {
            get
            {

                return this.m_Comments;
            }
        }

        private string m_Company = "";
        /// <summary>
        /// Company
        /// </summary>
        public string Company
        {
            get
            {
                return this.m_Company;
            }
        }

        private string m_FileOwner = "";
        /// <summary>
        /// File owner
        /// </summary>
        public string FileOwner
        {
            get
            {
                return this.m_FileOwner;
            }
        }

        private string m_FileVersion = "";
        /// <summary>
        /// File version
        /// </summary>
        public string FileVersion
        {
            get
            {
                return this.m_FileVersion;
            }
        }

        private string m_FileDescription = "";
        /// <summary>
        /// File description
        /// </summary>
        public string FileDescription
        {
            get
            {
                return this.m_FileDescription;
            }
        }

        #endregion

        #region Initialization

        internal ExtendedProperties(FileObject fileObject)
        {
            this.m_File = fileObject;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Load extended file properties. Calling this method is computationally expensive therefore properties are not loaded by default
        /// </summary>
        public void Load()
        {
            // Get Attributes
            //this.m_ApplicationName = this.GetAttribute(SystemProperties.System.ApplicationName);
            //this.m_Author = this.GetAttribute(SystemProperties.System.Author);
            //this.m_Comments = this.GetAttribute(SystemProperties.System.Comment);
            //this.m_Company = this.GetAttribute(SystemProperties.System.Company);
            //this.m_ComputerName = this.GetAttribute(SystemProperties.System.ComputerName);
            //this.m_CopyRight = this.GetAttribute(SystemProperties.System.Copyright);
            //this.m_FileDescription = this.GetAttribute(SystemProperties.System.FileDescription);
            //this.m_FileOwner = this.GetAttribute(SystemProperties.System.FileOwner);
            //this.m_FileVersion = this.GetAttribute(SystemProperties.System.FileVersion);
        }

        ///// <summary>
        ///// Retrieve an attribute property on a file
        ///// </summary>
        ///// <returns></returns>        
        //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        //private string GetAttribute(PropertyKey key)
        //{
        //    try
        //    {
        //        // Get File Attribute
        //        ShellObject objFile = ShellObject.FromParsingName(this.m_File.FilePath);
                
        //        // Get File Property
        //        IShellProperty attribute = objFile.Properties.GetProperty(key);

        //        // Validation
        //        if (attribute == null || attribute.ValueAsObject == null) { return ""; }

        //        return attribute.ValueAsObject.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());
        //        return "";
        //    }
        //}


        #endregion
    }
}
