using System;
using System.Data;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.IO.Compression;
using WellFitMobile.FileSystem.Information;
using WellFitMobile.FileSystem.Directory;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile;

namespace WellFitMobile.FileSystem.File.Extensions
{
    /// <summary>
    /// File Object Extension Methods
    /// </summary>
    public static class FileObjectExtensions
    {
        #region Check

        /// <summary>
        /// Check to see if a file is locked
        /// </summary>
        /// <param name="fileObject">File to check is locked</param>        
        /// <param name="boolIncludeCurrentProcess"></param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool CheckIsLocked(this FileObject fileObject, bool boolIncludeCurrentProcess = false)
        {
            return false;
            //return FileFunctions.CheckIsFileLocked(fileObject.FilePath, boolIncludeCurrentProcess);
        }

        /// <summary>
        /// Check to see if a file can be opened
        /// </summary>
        /// <param name="fileObject">Filename to check if it can be opened</param>
        /// <returns></returns>
        private static bool CheckCanOpen(this FileObject fileObject)
        {
            try
            {
                // Create File Info
                FileInfo fileInfo = new FileInfo(fileObject.FilePath);

                // Validation
                if (fileInfo == null || fileInfo.Exists == false) { return false; }

                // Open File
                fileInfo
                    .Open(FileMode.Open, FileAccess.Read, FileShare.None)
                    .Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        #endregion
        
        #region Attributes

        ///// <summary>
        ///// Retrieve an attribute property on a file
        ///// </summary>
        ///// <param name="fileObject">File to retrieve information from</param>
        ///// <returns></returns>
        //[Obsolete("To Be Reviewed For Implementation")]
        //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        //public static string GetAttribute(this FileObject fileObject)
        //{
        //    try
        //    {
        //        ShellObject objFile = ShellObject.FromParsingName(fileObject.FilePath);
        //        List<string> listPropertyNames = new List<string>();
        //        List<PropertyKey> listProperties = typeof(SystemProperties.System)
        //            .GetProperties().Select(property => (PropertyKey)property.GetValue(null, null)).ToList();

        //        foreach (PropertyKey property in listProperties)
        //        {
        //            IShellProperty attribute = objFile.Properties.GetProperty(property);
        //            listPropertyNames.Add(attribute.CanonicalName);
        //        }

        //        return "";

        //        //// Get File Attribute
        //        //ShellObject objFile = ShellObject.FromParsingName(fileObject.FilePath);

        //        //// Get File Property
        //        //IShellProperty attribute = objFile.Properties.GetProperty(propertyKey);

        //        //return attribute;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Retrieve an attribute property value on a file
        ///// </summary>
        ///// <param name="fileObject">File to retrieve information from</param>
        ///// <param name="propertyKey"></param>
        ///// <param name="propertyFormat"></param>
        ///// <param name="strError">Error string containing any Error message encountered</param>
        ///// <returns></returns>
        //[Obsolete("To Be Reviewed For Implementation")]
        //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        //private static string GetAttributeValue(this FileObject fileObject, PropertyKey propertyKey, PropertyDescriptionFormatOptions propertyFormat, ref string strError)
        //{
        //    return "";         
        //    //return FileFunctions.GetFileAttributeValue(fileObject.FilePath, propertyKey, propertyFormat);
        //}

        /// <summary>
        /// Set an attribute on a file
        /// </summary>
        /// <param name="fileObject">File to set information for</param>
        /// <param name="fileAttributes"></param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static AppGlobals.ResultType SetAttributes(this FileObject fileObject, System.IO.FileAttributes fileAttributes)
        {
            try
            {
                // Set File Attributes
                System.IO.File.SetAttributes(fileObject.FilePath, fileAttributes);

                // Refresh File Object
                fileObject.Refresh();

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

        #region Permissions

        /// <summary>
        /// Set permissions on a file directory
        /// </summary>
        /// <param name="fileObject">File to set permissions for</param>
        /// <param name="fileRights">The type of file rights to set access for</param>
        /// <param name="accessControlType">The type of access to </param>
        /// <param name="strSpecificUserName">Optional: Define a specific username to set permissions for</param>
        /// <returns></returns>
        public static AppGlobals.ResultType SetPermission(this FileObject fileObject, FileSystemRights fileRights, AccessControlType accessControlType)
        {
            try
            {
                // Get User Name
                string strFullyQualifiedUserName = Environment.UserDomainName + "\\" + Environment.UserName;

                // Create FileInfo
                FileInfo fileInfo = new FileInfo(fileObject.FilePath);

                //// Get File Security Settings
                //FileSecurity fileSecurity = fileInfo.GetAccessControl();

                //// Create Access Rule
                //FileSystemAccessRule fileAccessRule = new FileSystemAccessRule(strFullyQualifiedUserName, fileRights, accessControlType);

                //// Add the FileSystemAccessRule to the Security Settings.
                //fileSecurity.AddAccessRule(fileAccessRule);

                //// Set Access Control
                //fileInfo.SetAccessControl(fileSecurity);

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
        
        #region Encrypt / Decrypt

        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="fileObject">File to encrypt</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static AppGlobals.ResultType Encrypt(this FileObject fileObject)
        {
            try
            {
                System.IO.File.Encrypt(fileObject.FilePath);

                // Refresh File Object
                fileObject.Refresh();

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
        /// Encrypt a file
        /// </summary>
        /// <param name="fileObject">File to encrypt</param>
        /// <param name="strEncryptedFilename">Encrypted filename</param>
        /// <param name="strKey">Key to use to encrypt the file</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static AppGlobals.ResultType Encrypt(this FileObject fileObject, string strEncryptedFilename, string strKey)
        {
            try
            {
                // Read File
                FileStream stream = new FileStream(fileObject.FilePath, FileMode.Open, FileAccess.Read);
                FileStream streamEncrypted = new FileStream(strEncryptedFilename, FileMode.Create, FileAccess.Write);
                DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
                DES.Key = ASCIIEncoding.ASCII.GetBytes(strKey);
                DES.IV = ASCIIEncoding.ASCII.GetBytes(strKey);

                // Create Encryptor
                ICryptoTransform desencrypt = DES.CreateEncryptor();
                CryptoStream cryptostream = new CryptoStream(streamEncrypted, desencrypt, CryptoStreamMode.Write);

                byte[] bytearrayinput = new byte[stream.Length];
                stream.Read(bytearrayinput, 0, bytearrayinput.Length);

                // Write to File
                cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
                cryptostream.Close();
                stream.Close();

                // Refresh File Object
                fileObject.Refresh();

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
        /// Decrypt a file
        /// </summary>
        /// <param name="fileObject">Encrypted filename</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static AppGlobals.ResultType Decrypt(this FileObject fileObject)
        {
            try
            {
                // Decrypt File
                System.IO.File.Decrypt(fileObject.FilePath);

                // Refresh File Object
                fileObject.Refresh();

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
        /// Decrypt a file
        /// </summary>
        /// <param name="fileObject">Encrypted filename</param>
        /// <param name="strDecryptedFilename">Decrypted filename</param>
        /// <param name="strKey">Key to use to decrypt the file</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static AppGlobals.ResultType Decrypt(this FileObject fileObject, string strDecryptedFilename, string strKey)
        {
            try
            {
                // Create DES Provider
                DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

                DES.Key = ASCIIEncoding.ASCII.GetBytes(strKey);
                DES.IV = ASCIIEncoding.ASCII.GetBytes(strKey);

                // Read Encrypted File
                FileStream stream = new FileStream(fileObject.FilePath, FileMode.Open, FileAccess.Read);
                ICryptoTransform desdecrypt = DES.CreateDecryptor();
                CryptoStream cryptostreamDecr = new CryptoStream(stream, desdecrypt, CryptoStreamMode.Read);

                // Write Decrypted File
                StreamWriter fsDecrypted = new StreamWriter(strDecryptedFilename);
                fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
                fsDecrypted.Flush();
                fsDecrypted.Close();

                // Refresh File Object
                fileObject.Refresh();

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
        
        #region Path Information

        /// <summary>
        /// Retrieve the file extension of a file
        /// </summary>
        /// <param name="fileObject">Filename to retrieve extension from</param>
        /// <returns></returns>
        internal static string GetExtension(this FileObject fileObject)
        {
            try
            {
                // Get File Extenstion
                string strExtension = System.IO.Path.GetExtension(fileObject.FilePath);

                return strExtension;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
        
        /// <summary>
        /// Retrieve a filename with file extension
        /// </summary>
        /// <param name="fileObject">Filename to retrieve name with extension from</param>
        /// <returns></returns>
        internal static string GetNameWithExtenstion(this FileObject fileObject)
        {
            try
            {
                // Get File Name Without Extension
                string strFileName = System.IO.Path.GetFileName(fileObject.FilePath);

                return strFileName;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
        
        /// <summary>
        /// Retrieve a filename without file extension
        /// </summary>
        /// <param name="fileObject">Filename to retrieve name without extension from</param>
        /// <returns></returns>
        internal static string GetNameWithOutExtenstion(this FileObject fileObject)        
        {
            try
            {               
                // Get File Name Without Extension
                string strFileName = System.IO.Path.GetFileNameWithoutExtension(fileObject.FilePath);

                return strFileName;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        #endregion

        #region DataTable

        //public static Globals.ResultType SaveToCsvFile(this DataTable dt, string strFileName, bool boolIncludeColumnNamesAsHeader = true)
        //{
        //    try
        //    {
        //        // Create New File
        //        FileObject fileObject = new FileObject(strFileName);
                
        //        string strDataTableContent = "";

        //        // Validation
        //        if (boolIncludeColumnNamesAsHeader == true)
        //        {
        //            string strHeader = String.Join(TextConstants.Comma.ToString(), dt.ColumnNames().ToArray());
        //            strDataTableContent = strHeader + TextConstants.LineBreak;
        //        }

        //        // Get Rows As Delimited List
        //        List<string> listValues = dt.RowsToStringList().Select(list => String.Join(TextConstants.Comma.ToString(), list.ToArray())).ToList();

        //        // Get Content
        //        strDataTableContent += String.Join(TextConstants.LineBreak, listValues.ToArray());

        //        // Create File
        //        Globals.ResultType createFileResult = fileObject.Create(strDataTableContent);

        //        return createFileResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());

        //        return Globals.ResultType.Failure;
        //    }
        //}

        //public static DataTable LoadFromCsvFile(this DataTable dt, string strFileName)
        //{
        //    try
        //    {
        //        // Create New File
        //        FileObject fileObject = new FileObject(strFileName);

        //        // Validation
        //        if (fileObject.Exists == false || fileObject.Content.Lines.Count < 1) { return null; }
                
        //        // Get Column Names
        //        List<string> listColumnNames = fileObject.Content.Lines[0].Split(TextConstants.Comma).ToList();

        //        // Add Columns To DataTable
        //        listColumnNames.ForEach(column => dt.Columns.Add(column));

        //        // Check Line Count
        //        if (fileObject.Content.Lines.Count < 2) { return dt; }

        //        // Loop Lines
        //        for (int intIndex = 1; intIndex < fileObject.Content.Lines.Count; intIndex++)
        //        {
        //            // Create New Row
        //            DataRow dr = dt.NewRow();

        //            // Set Row Values
        //            dr.ItemArray = fileObject.Content.Lines[intIndex].Split(TextConstants.Comma);
                    
        //            // Add Row To List
        //            dt.Rows.Add(dr);
        //        }

        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());

        //        return null;
        //    }
        //}

        #endregion
    }
}
