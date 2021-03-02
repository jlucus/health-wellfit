using System;
using System.IO;
using WellFitMobile.FileSystem.Directory;
using WellFitMobile.FileSystem.Information;
using System.Security.AccessControl;
using WellFitMobile.FileSystem.FileSystem.Size;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitMobile.FileSystem.FileSystem.Information;
using WellFitPlus.Mobile;

namespace WellFitMobile.FileSystem.FileSystem.Entities
{
    /// <summary>
    /// Base class for a file system object
    /// </summary>
    public abstract class FileSystemObject : IFileSystemObject, IFileInformation
    {
        #region Properties
        
        #region Path

        private string m_FilePath = "";
        /// <summary>
        /// The fully qualified filepath
        /// </summary>
        public string FilePath
        {
            get
            {
                return this.m_FilePath;
            }
            set
            {
                // Check Dirty Flag
                if (this.m_FilePath != "")
                {
                    this.m_IsDirty = true;
                }

                // Set File Path
                this.m_FilePath = value;

                // Check Dirty Flag
                if (this.m_IsDirty == true)
                {
                    // Refresh File Information
                    this.Refresh();
                    this.m_IsDirty = false;
                }
            }
        }

        private string m_Name = "";
        /// <summary>
        /// The short name of the file system object
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get
            {
                return this.m_Name;
            }
            protected set
            {
                this.m_Name = value;
            }
        }
        
        #endregion

        #region Relational

        private DirectoryObject m_ParentDirectory;
        /// <summary>
        /// The parent directory of this directory
        /// </summary>
        /// <returns>The parent directory of the file system object</returns>
        public virtual DirectoryObject ParentDirectory
        {
            get
            {
                // Check Is Path Root
                bool boolIsRoot = System.IO.Path.GetPathRoot(this.FilePath) == this.m_FilePath;

                // Validation
                if (m_ParentDirectory == null && this.Exists == true && boolIsRoot == false)
                {
                    // Get Directory Path
                    DirectoryInfo directoryInfo = System.IO.Directory.GetParent(this.FilePath);

                    // Validation
                    if (directoryInfo != null && directoryInfo.Exists == true)
                    {
                        // Get Parent Directory
                        this.m_ParentDirectory = new DirectoryObject(directoryInfo.FullName);
                    }
                }

                return this.m_ParentDirectory;
            }
            protected set
            {
                this.m_ParentDirectory = value;
            }
        }

        #endregion

        #region Security

        private Permissions.FileSystemSecurity m_Permissions;
        /// <summary>
        /// The user permissions for the current executing user
        /// </summary>
        /// <returns></returns>
        public virtual Permissions.FileSystemSecurity Permissions
        {
            get
            {
                return this.m_Permissions;
            }
        }

        #endregion

        #region Size

        private FileSizeInformation m_Size;
        /// <summary>
        /// Information about the size of the file system object
        /// </summary>
        public FileSizeInformation Size
        {
            get
            {
                return this.m_Size;
            }
            protected set
            {
                this.m_Size = value;
            }
        }

        #endregion

        #region Attributes

        private FileAttributes m_Attributes;
        /// <summary>
        /// The attributes the file has
        /// </summary>
        /// <returns></returns>
        public FileAttributes Attributes
        {
            get
            {
                return this.m_Attributes;
            }
            set
            {
                this.m_Attributes = value;
            }
        }

        private DateTime m_CreationDate = DateTime.MinValue;
        /// <summary>
        /// The date that the file was created
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                return this.m_CreationDate;
            }
            protected set
            {
                this.m_CreationDate = value;
            }
        }

        private DateTime m_LastAccessedDate = DateTime.MinValue;
        /// <summary>
        /// The date that the file was last accessed
        /// </summary>
        public DateTime LastAccessedDate
        {
            get
            {
                return this.m_LastAccessedDate;
            }
            protected set
            {
                this.m_LastAccessedDate = value;
            }
        }

        private DateTime m_LastWriteDate = DateTime.MinValue;
        /// <summary>
        /// The date that the file was last written to
        /// </summary>
        public DateTime LastWriteDate
        {
            get
            {
                return this.m_LastWriteDate;
            }
            protected set
            {
                this.m_LastWriteDate = value;
            }
        }

        #endregion

        #region Validation
        
        /// <summary>
        /// Flag indicating whether or not the file exists in the file system
        /// </summary>
        public abstract bool Exists { get; }

        private bool m_IsDirty;
        /// <summary>
        /// Whether or not this objects properties have been modified and its property information needs to be refreshed
        /// </summary>
        /// <returns></returns>
        protected bool IsDirty
        {
            get
            {
                return this.m_IsDirty;
            }
        }

        /// <summary>
        /// Returns whether this file object is read only
        /// </summary>
        /// <returns></returns>
        public bool IsReadOnly
        {
            get
            {
                return (this.Attributes & FileAttributes.ReadOnly) != 0;
            }
        }
        
        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="strPath">File path of the system file or directory</param>
        protected FileSystemObject(string strPath)
        {
            // Validation
            if (strPath == "") { return; }

            // Set File Path
            this.FilePath = strPath;
            
            // Load Properties
            this.LoadProperties();
        }

        /// <summary>
        /// Load the properties for this file object
        /// </summary>
        /// <returns></returns>
        private AppGlobals.ResultType LoadProperties()
        {
            try
            {                
                #region Attributes

                // Get File Information
                FileInfo info = new FileInfo(this.m_FilePath);

                // Get Short Name
                this.m_Name = info.Name;

                // Get Attributes
                this.Attributes = info.Attributes;

                // Get File Creation Date
                this.m_CreationDate = System.IO.File.GetCreationTime(this.FilePath);

                // Get File Last Accessed Date
                this.m_LastAccessedDate = System.IO.File.GetLastAccessTime(this.FilePath);

                // Get File Last Write Date
                this.m_LastWriteDate = System.IO.File.GetLastWriteTime(this.FilePath);

                #endregion

                #region Security

                // Get Security Rules
                this.m_Permissions = new Permissions.FileSystemSecurity(this);

                #endregion
                
                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return AppGlobals.ResultType.Failure;
            }
        }

        #endregion
        
        #region Abstract Methods

        /// <summary>
        /// Refresh the properties of this file
        /// </summary>        
        /// <returns></returns>
        public abstract AppGlobals.ResultType Refresh();

        /// <summary>
        /// Delete this file
        /// </summary>
        /// <returns></returns>
        public abstract AppGlobals.ResultType Delete();

        /// <summary>
        /// Rename this file
        /// </summary>
        /// <param name="strNewName">New filename</param>
        /// <returns></returns>
        public abstract AppGlobals.ResultType Rename(string strNewName);

        /// <summary>
        /// Refresh the properties of this file
        /// </summary>        
        /// <returns></returns>
        public abstract AppGlobals.ResultType Move(DirectoryObject destinationDirectoryObject);

        #endregion        
    }
}
