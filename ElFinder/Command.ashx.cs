using DNNConnect.CKEditorProvider.Constants;
using DNNConnect.CKEditorProvider.Objects;
using DNNConnect.CKEditorProvider.Utilities;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using ElFinder;
using System.Reflection;
using System.Collections.Specialized;

namespace DNNConnect.CKEditorProvider.ElFinder
{
    /// <summary>
    /// Summary description for Command
    /// </summary>
    public class Command : IHttpHandler
    {
        #region Constants and Fields

        /// <summary>
        /// The Image or Link that is selected inside the Editor.
        /// </summary>
        private static string ckFileUrl;

        /// <summary>
        ///   The allowed flash ext.
        /// </summary>
        private readonly string[] allowedFlashExt = { "swf", "flv", "mp3" };

        /// <summary>
        ///   The allowed image ext.
        /// </summary>

        /// <summary>
        ///   The request.
        /// </summary>
        private HttpRequest request;

        /// <summary>
        /// Current Settings Base
        /// </summary>
        private EditorProviderSettings currentSettings = new EditorProviderSettings();

        /// <summary>
        ///   The _portal settings.
        /// </summary>
        private PortalSettings _portalSettings;

        /// <summary>
        ///   The extension white list.
        /// </summary>
        private string extensionWhiteList;

        /// <summary>
        /// The browser modus
        /// </summary>
        private string browserModus;

        #endregion

        public void ProcessRequest(HttpContext context)
        {
            _portalSettings = GetPortalSettings(context);

            NameValueCollection parameters = request.HttpMethod != "POST" ? request.QueryString : request.Form;

            bool imagesOnly = parameters["Type"] == "Image";

            DnnFileSystemDriver driver = new DnnFileSystemDriver(imagesOnly);

            var folder = StartingDir();

            var root = new Root(folder)
            {
                // Sample using ASP.NET built in Membership functionality...
                // Only the super user can READ (download files) & WRITE (create folders/files/upload files).
                // Other users can only READ (download files)
                // IsReadOnly = !User.IsInRole(AccountController.SuperUser)

                IsReadOnly = false, // Can be readonly according to user's membership permission
                Alias = "Files", // Beautiful name given to the root/home folder
                MaxUploadSizeInKb = 500, // Limit imposed to user uploaded file <= 500 KB
                LockedFolders = new List<string>(new string[] { "Folder1" })
            };

            driver.AddRoot(root);

            var connector = new Connector(driver);

            var result = connector.Process(context.Request);

            var jsonResult = result as JsonResult;

            if (jsonResult != null)
            {
                string json = JsonConvert.SerializeObject(jsonResult.Data);

                context.Response.ContentType = jsonResult.ContentType;

                context.Response.Write(json);
            }

            var fileResult = result as DownloadFileResult;

            if (fileResult != null)
            {
                if (fileResult.IsDownload)
                {
                    var baseContext = new System.Web.HttpContextWrapper(context);

                    fileResult.Download(baseContext.Response, baseContext.Request);
                }
                else
                {
                    string url = FileManager.Instance.GetUrl(fileResult.File);

                    context.Response.Redirect(url, true);
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private PortalSettings GetPortalSettings(HttpContext context)
        {
            int iTabId = 0, iPortalId = 0;

            PortalSettings portalSettings;
            request = context.Request;

            try
            {
                if (request.QueryString["tabid"] != null)
                {
                    iTabId = int.Parse(request.QueryString["tabid"]);
                }

                if (request.QueryString["PortalID"] != null)
                {
                    iPortalId = int.Parse(request.QueryString["PortalID"]);
                }

                string sDomainName = Globals.GetDomainName(request, true);

                string sPortalAlias = PortalAliasController.GetPortalAliasByPortal(iPortalId, sDomainName);

                PortalAliasInfo objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(sPortalAlias);

                portalSettings = new PortalSettings(iTabId, objPortalAliasInfo);
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>
        /// Get the Current Starting Directory
        /// </summary>
        /// <returns>
        /// Returns the Starting Directory.
        /// </returns>
        private IFolderInfo StartingDir()
        {
            IFolderInfo startingFolderInfo = null;

            if (!currentSettings.BrowserRootDirId.Equals(-1))
            {
                // var rootFolder = new FolderController().GetFolderInfo(this._portalSettings.PortalId, this.currentSettings.BrowserRootDirId);
                var rootFolder = FolderManager.Instance.GetFolder(currentSettings.BrowserRootDirId);

                if (rootFolder != null)
                {
                    startingFolderInfo = rootFolder;
                }
            }
            else
            {
                startingFolderInfo = FolderManager.Instance.GetFolder(_portalSettings.PortalId, string.Empty);
            }

            if (Utility.IsInRoles(_portalSettings.AdministratorRoleName, _portalSettings))
            {
                return startingFolderInfo;
            }

            if (currentSettings.SubDirs)
            {
                startingFolderInfo = GetUserFolderInfo(startingFolderInfo.PhysicalPath);
            }
            else
            {
                return startingFolderInfo;
            }

            if (Directory.Exists(startingFolderInfo.PhysicalPath))
            {
                return startingFolderInfo;
            }

            var folderStart = startingFolderInfo.PhysicalPath;

            folderStart =
                folderStart.Substring(_portalSettings.HomeDirectoryMapPath.Length).Replace(
                    "\\", "/");

            startingFolderInfo = FolderManager.Instance.AddFolder(_portalSettings.PortalId, folderStart);

            Directory.CreateDirectory(startingFolderInfo.PhysicalPath);

            SetFolderPermission(startingFolderInfo);

            return startingFolderInfo;
        }

        /// <summary>
        /// Gets the user folder Info.
        /// </summary>
        /// <param name="startingDir">The Starting Directory.</param>
        /// <returns>Returns the user folder path</returns>
        private IFolderInfo GetUserFolderInfo(string startingDir)
        {
            IFolderInfo userFolderInfo;

            var userFolderPath = Path.Combine(startingDir, "userfiles");

            // Create "userfiles" folder if not exists
            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart = folderStart.Substring(_portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                userFolderInfo = FolderManager.Instance.AddFolder(_portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                SetFolderPermission(userFolderInfo);
            }

            // Create user folder based on the user id
            userFolderPath = Path.Combine(
                userFolderPath,
                string.Format("{0}\\", UserController.Instance.GetCurrentUserInfo().UserID));

            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart = folderStart.Substring(_portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                userFolderInfo = FolderManager.Instance.AddFolder(_portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                SetFolderPermission(userFolderInfo);

                SetUserFolderPermission(userFolderInfo, UserController.Instance.GetCurrentUserInfo());
            }
            else
            {
                userFolderInfo = Utility.ConvertFilePathToFolderInfo(userFolderPath, _portalSettings);

                // make sure the user has the correct permissions set
                SetUserFolderPermission(userFolderInfo, UserController.Instance.GetCurrentUserInfo());
            }

            return userFolderInfo;
        }

        /// <summary>
        /// Set Folder Permission
        /// </summary>
        /// <param name="folderId">The Folder Id.</param>
        private void SetFolderPermission(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);

            SetFolderPermission(folder);
        }

        /// <summary>
        /// Set Folder Permission
        /// </summary>
        /// <param name="folderInfo">The folder info.</param>
        private void SetFolderPermission(IFolderInfo folderInfo)
        {
            FolderManager.Instance.CopyParentFolderPermissions(folderInfo);
        }

        /// <summary>
        /// Set Folder Permission for the Current User
        /// </summary>
        /// <param name="folderInfo">The folder info.</param>
        /// <param name="currentUserInfo">The current user info.</param>
        private void SetUserFolderPermission(IFolderInfo folderInfo, UserInfo currentUserInfo)
        {
            if (FolderPermissionController.CanManageFolder((FolderInfo)folderInfo))
            {
                return;
            }

            foreach (
                var folderPermission in from PermissionInfo permission in PermissionController.GetPermissionsByFolder()
                                        where
                                            permission.PermissionKey.ToUpper() == "READ"
                                            || permission.PermissionKey.ToUpper() == "WRITE"
                                            || permission.PermissionKey.ToUpper() == "BROWSE"
                                        select
                                            new FolderPermissionInfo(permission)
                                            {
                                                FolderID = folderInfo.FolderID,
                                                UserID = currentUserInfo.UserID,
                                                RoleID = Null.NullInteger,
                                                AllowAccess = true
                                            })
            {
                folderInfo.FolderPermissions.Add(folderPermission);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folderInfo);
        }

    }
}