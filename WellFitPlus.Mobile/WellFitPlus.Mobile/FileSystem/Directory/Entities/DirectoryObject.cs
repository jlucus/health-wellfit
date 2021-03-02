using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using WellFitMobile.FileSystem.File;
using WellFitMobile.FileSystem.Information;
using WellFitMobile.FileSystem.Directory;
using WellFitMobile.FileSystem.FileSystem.Entities;
using WellFitMobile.FileSystem.FileSystem.Size;
using WellFitMobile.FileSystem.File.Entities;
using System.Security.AccessControl;
using WellFitMobile.FileSystem.File.Extensions;
using WellFitMobile.FileSystem.Directory.Extensions;
using WellFitPlus.Mobile;

namespace WellFitMobile.FileSystem.Directory.Entities
{
    /// <summary>
    /// This class represents a filesystem directory 
    /// </summary>
    public sealed class DirectoryObject : FileSystemObject, IFileSystemObject
    {
        #region Properties

        #region All Children

        private DirectoryObjectList m_AllDirectories;
        /// <summary>
        /// List of all directories within this directory. This property loads recusrively, so the first instantiation may be expensive
        /// </summary>
        public DirectoryObjectList AllDirectories
        {
            get
            {
                // Validation
                if (this.m_AllDirectories == null)
                {
                    // Get SubDirectories
                    this.m_AllDirectories = this.GetSubDirectories("*", true);
                }

                return this.m_AllDirectories;
            }
        }

        private FileObjectList m_AllFiles;
        /// <summary>
        /// List of all files within this directory. This property loads recusrively, so the first instantiation may be expensive
        /// </summary>
        public FileObjectList AllFiles
        {
            get
            {
                // Validation
                if (this.m_AllFiles == null)
                {
                    // Get SubDirectories
                    this.m_AllFiles = this.GetFiles("*", true);
                }

                return this.m_AllFiles;
            }
        }
        
        #endregion

        #region Sub Directories

        private DirectoryObjectList m_SubDirectories;
        /// <summary>
        /// List of sub directories within this directory
        /// </summary>
        public DirectoryObjectList SubDirectories
        {
            get
            {
                // Validation
                if (this.m_SubDirectories == null)
                {
                    // Get SubDirectories
                    this.m_SubDirectories = this.GetSubDirectories("*", false);
                }

                return this.m_SubDirectories;
            }
        }

        #endregion

        #region Files

        private FileObjectList m_Files;
        /// <summary>
        /// List of filenames within this directory
        /// </summary>
        public FileObjectList Files
        {
            get
            {
                // Validation
                if (this.m_Files == null)
                {
                    // Get Files
                    this.m_Files = this.GetFiles("*", false);
                }

                return this.m_Files;
            }
        }

        #endregion

        #region Validation
        
        /// <summary>
        /// Overrides the base method and refreshes the exists property
        /// </summary>
        public sealed override bool Exists
        {
            get
            {
                // Refresh Exists Property
                return this.CheckExists();
            }
        }

        #endregion

        #region Permissions

        private Permissions.DirectorySecurity m_Permissions;
        /// <summary>
        /// Directory permissions
        /// </summary>
        public new Permissions.DirectorySecurity Permissions
        {
            get
            {
                return this.m_Permissions;
            }
        }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="strFileDirectory">Directory filepath</param>
        public DirectoryObject(string strFileDirectory)
            : base(strFileDirectory)
        {
            // Load Directory Properties
            this.LoadProperties();
        }
        
        /// <summary>
        /// Load the properties for this file directory
        /// </summary>
        /// <returns></returns>
        private AppGlobals.ResultType LoadProperties()
        {
            try
            {
                #region Validation

                // Validation
                if (this.Exists == false) { return AppGlobals.ResultType.Failure; }

                #endregion
                
                #region Size

                // Get Short Name
                this.Name = new DirectoryInfo(this.FilePath).Name;

                decimal decSize = this.GetSize(FileInformation.FileSizeType.Bytes, false);

                // Get Directory Size In Bytes
                this.Size = new FileSizeInformation(decSize);

                #endregion

                #region Permissions

                this.m_Permissions = new Permissions.DirectorySecurity(this);

                #endregion

                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                string strError = "An Error Occurred In Method '" + System.Reflection.MethodBase.GetCurrentMethod().Name + "'. Detail: " + ex.Message;
                Console.WriteLine(strError);

                return AppGlobals.ResultType.Failure;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check file directory exists
        /// </summary>
        /// <returns></returns>
        private bool CheckExists()
        {
            try
            {
                // Check Directory Exists
                bool boolExits = System.IO.Directory.Exists(this.FilePath);

                return boolExits;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Refresh the contents of this directory
        /// </summary>
        /// <returns></returns>
        public override AppGlobals.ResultType Refresh()
        {
            // Delete File
            return this.LoadProperties();
        }

        #endregion

        #region Actions

        #region Archive

#if !NET45
        ///// <summary>
        ///// Archive an entire directory
        ///// </summary>
        ///// <param name="destinationDirectoryObject">Destination directory</param>
        ///// <param name="compressionLevel">File compression level</param>
        ///// <param name="fileMode">File mode. Default is to create a new archive file or add to an existing archive</param>
        ///// <returns></returns>
        //public Globals.ResultType ArchiveDirectory(
        //    DirectoryObject destinationDirectoryObject, CompressionLevel compressionLevel, FileMode fileMode = FileMode.OpenOrCreate)
        //{
        //    try
        //    {
        //        // Create Archive
        //        ZipFile.CreateFromDirectory(this.FilePath, destinationDirectoryObject.FilePath, compressionLevel, true);

        //        return Globals.ResultType.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());
        //        return Globals.ResultType.Failure;
        //    }
        //}
#endif

        #endregion

        #region Delete

        /// <summary>
        /// Delete this directory
        /// </summary>        
        /// <returns></returns>
        public sealed override AppGlobals.ResultType Delete()
        {
            try
            {
                // Attempt File Deletion
                System.IO.Directory.Delete(this.FilePath, true);

                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return AppGlobals.ResultType.Failure;
            }
        }

        /// <summary>
        /// Deletes files from a directory
        /// </summary>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public AppGlobals.ResultType DeleteFiles(bool boolRecurse)
        {
            // Get Files
            FileObjectList listFiles = this.GetFiles(boolRecurse);

            // Delete Files
            AppGlobals.ResultType deleteResult = listFiles.Delete();

            return deleteResult;
        }
        
        #endregion

        #region Rename

        /// <summary>
        /// Rename this directory
        /// </summary>
        /// <param name="strNewName">New file directory name</param>
        /// <returns></returns>
        public sealed override AppGlobals.ResultType Rename(string strNewName)
        {
            try
            {
                // Get New File Path
                string strNewPath = Path.Combine(this.ParentDirectory.FilePath, strNewName);
                
                // Copy Existing Directory With New Name
                System.IO.Directory.Move(this.FilePath, strNewPath);
                
                // Set New File Path
                this.FilePath = strNewPath;

                // Refresh File
                this.Refresh();

                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return AppGlobals.ResultType.Failure;
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Create a new directory. This will create the full filepath hierarchy including all parent folders if permissions permit
        /// </summary>
        /// <param name="fileSystemRights">Type of file system rights to be set on the new directory</param>
        /// <param name="boolAllowEveryone">Flag to include Everyone permissions on the new directory</param>
        /// <returns></returns>
        public AppGlobals.ResultType Create(System.Security.AccessControl.FileSystemRights fileSystemRights, bool boolAllowEveryone = false)
        {
            try
            {
                // Create Directory
                System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(this.FilePath);

                // Validation
                if (directoryInfo == null || directoryInfo.Exists == false) { return AppGlobals.ResultType.Failure; }

                //// Set Directory Permissions
                //Globals.ResultType createResultType = this.SetPermissions(fileSystemRights, System.Security.AccessControl.AccessControlType.Allow);

                //// Validation
                //if (createResultType == Globals.ResultType.Failure) { return Globals.ResultType.Failure; }

                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return AppGlobals.ResultType.Failure;
            }
        }
                
        #endregion

        #region Move

        /// <summary>
        /// Moves a file directory to a destination location
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination for directory to be moved to</param>
        /// <returns></returns>
        public sealed override AppGlobals.ResultType Move(DirectoryObject destinationDirectoryObject)
        {
            try
            {
                // Validation
                if (destinationDirectoryObject.Exists == false) { return AppGlobals.ResultType.Failure; }

                // Get Directory Info
                System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(this.FilePath);

                // Move Directory
                directoryInfo.MoveTo(destinationDirectoryObject.FilePath);

                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return AppGlobals.ResultType.Failure;
            }
        }

        #endregion

        #region Copy

        /// <summary>
        /// Copies a file directory to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination path for directory to be copied to</param> 
        /// <returns></returns>
        public AppGlobals.ResultType Copy(DirectoryObject destinationDirectoryObject)
        {
            return this.Copy(destinationDirectoryObject, this);
        }

        /// <summary>
        /// Copies a file directory recursively to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination path for directory to be copied to</param> 
        /// <param name="directoryToCopy">Directory to be copied</param>
        /// <returns></returns>
        private AppGlobals.ResultType Copy(DirectoryObject destinationDirectoryObject, DirectoryObject directoryToCopy)
        {
            try
            {
                // Get New Path
                string strNewDirectory = Path.Combine(destinationDirectoryObject.FilePath, directoryToCopy.Name);

                // Get Directory
                DirectoryObject directory = new DirectoryObject(strNewDirectory);

                // Validation
                if (directory.Exists == false)
                {
                    // Create Directory
                    AppGlobals.ResultType createResult = directory.Create(System.Security.AccessControl.FileSystemRights.Write);

                    // Validation
                    if (createResult == AppGlobals.ResultType.Failure) { return AppGlobals.ResultType.Failure; }
                }

                // Copy All Files To New Directory
                this.Files.ForEach(file => file.Copy(directory, true));

                // Loop SubDirectories
                foreach (DirectoryObject childDirectory in directoryToCopy.SubDirectories)
                {
                    // Copy Directory
                    AppGlobals.ResultType copyResult = directoryToCopy.Copy(directory, childDirectory);

                    // Validation
                    if (copyResult == AppGlobals.ResultType.Failure) { return AppGlobals.ResultType.Failure; }
                }

                return AppGlobals.ResultType.Success;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return AppGlobals.ResultType.Failure;
            }
        }

        /// <summary>
        /// Copies directory files to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination for directory files to be copied to</param>
        /// <param name="strFind">Search pattern to use retrieving files</param>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <param name="boolDeleteOriginal">Flag that determines whether to delete the original files</param>
        /// <returns></returns>
        public AppGlobals.ResultType CopyFiles(DirectoryObject destinationDirectoryObject, string strFind, bool boolRecurse, bool boolDeleteOriginal = false)
        {
            return this.CopyFiles(destinationDirectoryObject, strFind, boolRecurse, boolDeleteOriginal);
        }

        /// <summary>
        /// Copies directory files to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination for directory files to be copied to</param>
        /// <param name="boolOverWrite">Flag for whether or not to overwrite any existing file in the destination directory</param>
        /// <param name="strFind">Search pattern to use retrieving files</param>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <param name="boolDeleteOriginal">Flag that determines whether to delete the original files</param>
        /// <returns></returns>
        public AppGlobals.ResultType CopyFiles(DirectoryObject destinationDirectoryObject, bool boolOverWrite, string strFind, bool boolRecurse, bool boolDeleteOriginal = false)
        {
            // Get Files
            FileObjectList listFiles = this.GetFiles(strFind, boolRecurse);
            AppGlobals.ResultType copyResult = AppGlobals.ResultType.Success;

            // Loop Files
            foreach (FileObject file in listFiles)
            {
                // Copy File
                AppGlobals.ResultType copyFileResult = file.Copy(destinationDirectoryObject, boolOverWrite, boolDeleteOriginal);

                // Validation
                copyResult = (copyFileResult != AppGlobals.ResultType.Success) ? AppGlobals.ResultType.Failure : AppGlobals.ResultType.Success;
            }

            return copyResult;
        }

        #endregion

        #endregion

        #region Get Files

        #region Synchronously

        /// <summary>
        /// Get file in directory by name
        /// </summary>
        /// <param name="strFileShortName">File name to retrieve, not case sensitive. The name may be with or without extension (e.g. "ReadMe.txt" or just "ReadMe")</param>
        /// <returns></returns>
        public FileObject File(string strFileShortName)
        {
            return this.Files.Where(file => 
                file.FullName.ToLower() == strFileShortName.ToLower() ||
                file.Name.ToLower() == strFileShortName.ToLower())
                .FirstOrDefault();
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public FileObjectList GetFiles(bool boolRecurse)
        {
            // Create Directory List
            FileObjectList files = this.GetFiles("*", boolRecurse);

            return files;
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>        
        /// <param name="strFind">Search string to find</param>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public FileObjectList GetFiles(string strFind, bool boolRecurse)
        {
            try
            {
                // Determine Search Pattern
                strFind = (strFind == null || strFind == "") ? "*" : strFind;
                strFind = (strFind.Length > 0 && strFind[0].ToString() != "*") ? "*" + strFind + "*" : strFind;

                // Determine Search Option
                System.IO.SearchOption option = (boolRecurse == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

                // Get Directory Files
                List<string> listFileNames = System.IO.Directory.GetFiles(this.FilePath, strFind, option).ToList();

                // Create File List
                FileObjectList files = new FileObjectList(listFileNames);

                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (DirectoryNotFoundException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (IOException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }


        #endregion

        #region Asynchronous

        ///// <summary>
        ///// Retrieves files from a file directory. 
        ///// </summary>
        ///// <param name="searchInformation">Search information to query the directory with</param>
        ///// <param name="callback">Callback method to be invoked after each provided timeout period elapses</param>
        ///// <param name="intCallbackTimeoutSecounds">Number of seconds to wait before invoking the callback method</param>
        ///// <returns></returns>
        //public FileObjectList GetFilesAsync(SearchInformation searchInformation, SearchInformation.SearchCallback callback, int intCallbackTimeoutSecounds = 10)
        //{
        //    try
        //    {
        //        // Validation
        //        FileObjectList fileObjectList = new FileObjectList();

        //        // Set Initial Search
        //        DateTime lastDate = DateTime.Now;

        //        // Create Directories Searched
        //        DirectoryObjectList directoriesSearched = new DirectoryObjectList();

        //        // Get File Asynchronously
        //        this.GetFilesAsync(searchInformation, callback, ref fileObjectList, ref directoriesSearched, ref lastDate, intCallbackTimeoutSecounds);

        //        return fileObjectList;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Retrieves files from a file directory. This method is recursive and requires a helper method to ensure proper encapsulation
        ///// </summary>
        ///// <param name="searchInformation">Search information to query the directory with</param>
        ///// <param name="callback">Callback method to be invoked after each provided timeout period elapses</param>
        ///// <param name="fileObjectList">Number of seconds to wait before invoking the callback method</param>
        ///// <param name="directoriesSearched">The directories that have been searched</param>
        ///// <param name="intCallbackTimeoutSecounds">Number of seconds to wait before invoking the callback method</param>
        ///// <param name="lastDate">Last time the callback was invoked</param>
        ///// <returns></returns>
        //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        //private FileObjectList GetFilesAsync(SearchInformation searchInformation, SearchInformation.SearchCallback callback,
        //    ref FileObjectList fileObjectList, ref DirectoryObjectList directoriesSearched, ref DateTime lastDate, int intCallbackTimeoutSecounds = 10)
        //{
        //    try
        //    {
        //        #region Get Files

        //        // Get Directory Files - Top Level Only
        //        List<string> listFileNames = System.IO.Directory.GetFiles(this.FilePath, searchInformation.SearchPattern, System.IO.SearchOption.TopDirectoryOnly).ToList();

        //        // Create File List
        //        FileObjectList listFiles = new FileObjectList(listFileNames);

        //        // Validation
        //        if (listFiles.Count > 0)
        //        {
        //            // Filter Files By Search Information
        //            List<FileObject> filesToAdd = searchInformation.GetSearchFilesFiltered(listFiles);

        //            // Create File List                
        //            fileObjectList.AddRange(filesToAdd.ToArray());

        //            // Distinct List
        //            fileObjectList = new FileObjectList(fileObjectList.Distinct().ToList());
        //        }

        //        // Add Directory To Searched List
        //        directoriesSearched.Add(this);

        //        #endregion

        //        #region Recurse Sub Directories

        //        // Loop SubDirectories
        //        foreach (DirectoryObject directory in this.SubDirectories)
        //        {
        //            // Get Sub Directory Files
        //            directory.GetFilesAsync(searchInformation, callback, ref fileObjectList, ref directoriesSearched, ref lastDate, intCallbackTimeoutSecounds);
        //        }

        //        #endregion

        //        #region Invoke Callback

        //        // Check Time For Callback
        //        if (fileObjectList != null && DateTime.Now > lastDate.AddSeconds(intCallbackTimeoutSecounds))
        //        {
        //            // Invoke Callback
        //            callback.Invoke(fileObjectList, directoriesSearched);
        //            lastDate = DateTime.Now;
        //        }

        //        #endregion

        //        return fileObjectList;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}
        
        #endregion

        #endregion

        #region Get Directories

        /// <summary>
        /// Retrieves a list of sub-directory paths from a file directory
        /// </summary>        
        /// <param name="strFolderName">Folder name to retrieve</param>        
        /// <returns></returns>
        public DirectoryObject SubDirectory(string strFolderName)
        {
            return this.SubDirectories.Where(directory => directory.Name == strFolderName).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a list of sub-directory paths from a file directory
        /// </summary>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public DirectoryObjectList GetSubDirectories(bool boolRecurse)
        {
            // Create Directory List
            DirectoryObjectList directories = this.GetSubDirectories("*", boolRecurse);

            return directories;
        }

        /// <summary>
        /// Retrieves a list of sub-directory paths from a file directory
        /// </summary>
        /// <param name="strFind">Search string to find</param>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public DirectoryObjectList GetSubDirectories(string strFind, bool boolRecurse)
        {
            try
            {
                // Determine Search Pattern
                strFind = (strFind != "") ? "*" + strFind + "*" : "";

                // Determine Search Option
                System.IO.SearchOption option = (boolRecurse == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

                // Get File Directories
                List<string> listDirectories = System.IO.Directory.GetDirectories(this.FilePath, strFind, option).ToList();

                // Create Directory List
                DirectoryObjectList directories = new DirectoryObjectList(listDirectories);

                return directories;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion
    }
}