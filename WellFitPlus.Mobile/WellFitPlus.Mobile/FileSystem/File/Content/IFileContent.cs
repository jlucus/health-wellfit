using System.Collections.Generic;
using System.IO;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile;

namespace WellFitMobile.FileSystem.File.Content
{
    /// <summary>
    /// FileObject interface
    /// </summary>
    internal interface IFileContent
    {
        #region Properties

        #region File

        /// <summary>
        /// The file object this content resides in
        /// </summary>
        /// <returns>File object that content resides in</returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        FileObject File { get; }

        /// <summary>
        /// The memory stream representing the value of the file content
        /// </summary>
        /// <returns></returns>
        MemoryStream MemoryStream { get; }

        #endregion

        #region Content

        /// <summary>
        /// The value of the file content
        /// </summary>
        /// <returns></returns>
        string Value { get; set; }

        /// <summary>
        /// The list of lines that make up the file content
        /// </summary>
        /// <returns></returns>
        List<string> Lines { get; }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        /// Load the file's content
        /// </summary>
        /// <returns></returns>
        AppGlobals.ResultType Load();

        /// <summary>
        /// Save the content value back to the file
        /// </summary>
        /// <returns></returns>
        AppGlobals.ResultType Save();

        /// <summary>
        /// Refresh the file's content
        /// </summary>
        /// <returns></returns>
        AppGlobals.ResultType Refresh();

        #endregion
    }
}
