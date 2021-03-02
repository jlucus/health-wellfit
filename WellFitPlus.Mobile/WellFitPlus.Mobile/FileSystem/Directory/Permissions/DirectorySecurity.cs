﻿using System;
using System.Security.AccessControl;
using System.Security.Principal;
using WellFitMobile.FileSystem.Directory;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitMobile.FileSystem.FileSystem.Entities;


namespace WellFitMobile.FileSystem.Directory.Permissions
{
    /// <summary>
    /// This class contains file system security information for a FileSystemObjectBase instance
    /// </summary>
    public sealed class DirectorySecurity : WellFitMobile.FileSystem.FileSystem.Permissions.FileSystemSecurity, IDirectorySecurity
    {
        #region Properties

        #region File System Object
        
        /// <summary>
        /// Copy of the file system directory for refreshing permissions
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private DirectoryObject m_FileSystemObject { get; set; }

        #endregion

        #region Permissions

        private bool m_CanAccess;

        /// <summary>
        /// Whether or not the current executing user has access privledges
        /// </summary>
        /// <returns></returns>
        public bool CanAccess
        {
            get
            {
                return this.m_CanAccess;
            }
        }

        private bool m_CanCreateFile;
        /// <summary>
        /// Whether or not the current executing user can create files in this directory
        /// </summary>
        /// <returns></returns>
        public bool CanCreateFile
        {
            get
            {
                return this.m_CanCreateFile;
            }
        }

        private bool m_CanCreateDirectory;
        /// <summary>
        /// Whether or not the current executing user can create directories in this directory
        /// </summary>
        /// <returns></returns>
        public bool CanCreateDirectory
        {
            get
            {
                return this.m_CanCreateDirectory;
            }
        }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="directoryObject">File system directory to retrieve security information for</param>
        public DirectorySecurity(DirectoryObject directoryObject)
            : base((FileSystemObject)directoryObject)
        {
            this.m_FileSystemObject = directoryObject;

            // Load Rights
            this.LoadPermissions();
        }

        /// <summary>
        /// Load the permissions for this directory
        /// </summary>
        private void LoadPermissions()
        {
            try
            {
                //                // Get Current User Identity
                //                WindowsIdentity userIdentity = WindowsIdentity.GetCurrent();

                //                // Get Current NT User
                //                NTAccount userAccount = new NTAccount(userIdentity.Name);

                //                // Get File Security
                //                FileSecurity fileSecurity = new FileSecurity(this.m_FileSystemObject.FilePath, AccessControlSections.Access);

                //                // Get Access Rules
                //                AuthorizationRuleCollection securityRules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));

                //                // Loop Rules
                //                foreach (FileSystemAccessRule rule in securityRules)
                //                {
                //                    // If Denied Access - Continue
                //                    if (rule.AccessControlType == AccessControlType.Deny) { continue; }

                //                    // Determine If Rule Account Is System Role And Current User Belongs To That Group
                //                    bool boolIsInSystemRole = this.IsInSystemRole(rule.IdentityReference.Value, userIdentity) == false;

                //                    // Detemine If User Or Group Applies To The Current User
                //                    if (rule.IdentityReference != userAccount && boolIsInSystemRole == false) { continue; }

                //#if NET40 || NET45
                //                // Set Can Access Permission
                //                this.m_CanAccess = (this.m_CanAccess == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.ListDirectory) : this.m_CanAccess;

                //                // Set Can Create Directory Permission
                //                this.m_CanCreateDirectory = (this.m_CanCreateDirectory == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) : this.m_CanCreateDirectory;

                //                // Set Can Create File Permission
                //                this.m_CanCreateFile = (this.m_CanCreateFile == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles) : this.m_CanCreateFile;
                //#endif
                //                }
            }
            catch (Exception ex)
            {
                string strError = "An Error Occurred In Method '" + System.Reflection.MethodBase.GetCurrentMethod().Name + "'. Detail: " + ex.Message;
                Console.WriteLine(strError);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Refresh the security privledges for the file system object
        /// </summary>
        public sealed override void Refresh()
        {
            // Load Rights
            this.LoadPermissions();
        }

        #endregion
    }
}

