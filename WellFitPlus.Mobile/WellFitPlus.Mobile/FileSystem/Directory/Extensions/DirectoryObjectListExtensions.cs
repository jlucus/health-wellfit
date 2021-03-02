using System.Collections.Generic;
using System.Linq;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitMobile.FileSystem.File;
using WellFitMobile.FileSystem.File.Entities;

namespace WellFitMobile.FileSystem.Directory.Extensions
{
    /// <summary>
    /// Directory object list extension methods
    /// </summary>
    public static class DirectoryObjectListExtensions
    {
        #region Get Files

        /// <summary>
        /// Get file in directory by name
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from. Search file name is not case sensitive.</param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public static FileObject GetFile(this List<DirectoryObject> directoryObjectList, string strName)
        {
            return directoryObjectList
                .Where(directory => directory.Files != null)
                .SelectMany(directory => directory.Files
                    .Where(file => file.Name.ToLower() == strName.ToLower() 
                        || file.FullName.ToLower() == strName.ToLower())).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from</param>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public static FileObjectList GetFiles(this List<DirectoryObject> directoryObjectList, bool boolRecurse)
        {
            return directoryObjectList.GetFiles("*", boolRecurse);
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from</param>
        /// <param name="strFind">Search string to find</param>
        /// <param name="boolRecurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public static FileObjectList GetFiles(this List<DirectoryObject> directoryObjectList, string strFind, bool boolRecurse)
        {
            return new FileObjectList(directoryObjectList
                .SelectMany(directory => directory.GetFiles(strFind, boolRecurse)).ToList());            
        }
        
        #endregion

        #region Get Directory

        /// <summary>
        /// Get sub directory in directory by name
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from</param>
        /// <param name="strFolderName"></param>
        /// <returns></returns>
        public static DirectoryObject GetDirectory(this List<DirectoryObject> directoryObjectList, string strFolderName)
        {
            return directoryObjectList
                .SelectMany(directory => directory.SubDirectories).Where(directory => directory.Name == strFolderName).FirstOrDefault();
        }
        
        #endregion
    }
}
