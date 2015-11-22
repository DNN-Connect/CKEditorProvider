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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DNNConnect.CKEditorProvider.ElFinder
{
    public partial class Browser : System.Web.UI.Page
    {
        #region Constants and Fields

        /// <summary>
        ///   The request.
        /// </summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

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

        #endregion

        public string ElOptions { get; set; }

        public string LanguageFileUrl
        {
            get
            {
                string langCode = Request.QueryString["langCode"];

                if (string.IsNullOrEmpty(langCode) || langCode == "en")
                    return null;

                return ResolveUrl(string.Format("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ElFinder/js/i18n/elfinder.{0}.js", langCode));
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _portalSettings = GetPortalSettings();

        }
        protected void Page_Load(object sender, EventArgs e)
        {
            string langCode = Request.QueryString["langCode"];

            extensionWhiteList = HostController.Instance.GetString("FileExtensions").ToLower();

            if (!string.IsNullOrEmpty(request.QueryString["mode"]))
            {
                currentSettings.SettingMode =
                    (SettingsMode)Enum.Parse(typeof(SettingsMode), request.QueryString["mode"]);
            }

            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            var settingsDictionary = EditorController.GetEditorHostSettings();

            var portalRoles = RoleController.Instance.GetRoles(_portalSettings.PortalId);

            switch (currentSettings.SettingMode)
            {
                case SettingsMode.Default:
                    // Load Default Settings
                    currentSettings = SettingsUtil.GetDefaultSettings(
                        _portalSettings,
                        _portalSettings.HomeDirectoryMapPath,
                        objProvider.Attributes["ck_configFolder"],
                        portalRoles);
                    break;
                case SettingsMode.Portal:
                    currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                        _portalSettings,
                        currentSettings,
                        settingsDictionary,
                        string.Format("DNNCKP#{0}#", request.QueryString["PortalID"]),
                        portalRoles);
                    break;
                case SettingsMode.Page:
                    currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                        _portalSettings,
                        currentSettings,
                        settingsDictionary,
                        string.Format("DNNCKT#{0}#", request.QueryString["tabid"]),
                        portalRoles);
                    break;
                case SettingsMode.ModuleInstance:
                    currentSettings = SettingsUtil.LoadModuleSettings(
                        _portalSettings,
                        currentSettings,
                        string.Format(
                            "DNNCKMI#{0}#INS#{1}#", request.QueryString["mid"], request.QueryString["ckId"]),
                        int.Parse(request.QueryString["mid"]),
                        portalRoles);
                    break;
            }

            // set current Upload file size limit
            currentSettings.UploadFileSizeLimit = SettingsUtil.GetCurrentUserUploadSize(
                currentSettings,
                _portalSettings,
                HttpContext.Current.Request);

            string[] myCommands = {
                            "back", "forward",
                             "reload",
                             "home", "up",
                             "mkdir", "upload",
                             "download",
                             "info",
                             "quicklook",
                             "copy", "cut", "paste",
                             "rm",
                             "rename",
                             "view", "sort" };

            string onlyMimes = request.QueryString["Type"] == "Image"  ? "image" : null;

            var options = new
            {
                url = ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ElFinder/Command.ashx?") + Request.QueryString.ToString(),
                rememberLastDir = false, // Prevent elFinder saving in the Browser LocalStorage the last visited directory
                commands = myCommands,
                lang = langCode, // elFinder supports UI and messages localization. Check the folder Content\elfinder\js\i18n for all available languages. Be sure to include the corresponding .js file(s) in the JavaScript bundle.
                onlyMimes = new object[] { onlyMimes },
                uiOptions = new
                { // UI buttons available to the user
                    toolbar = new object[] {
                            new string[] { "back", "forward" },
                            new string[] {"reload" },
                            new string[] {"home", "up" },
                            new string[] {"mkdir", "upload" },
                            new string[] {"download" },
                            new string[] {"info" },
                            new string[] {"quicklook" },
                           // new string[] {"copy", "cut", "paste" },
                            new string[] {"rm" },
                            new string[] {"rename" },
                            new string[] {"view", "sort" }
                        }
                },
                CKEditorFuncNum = Request.QueryString["CKEditorFuncNum"]
            };

            ElOptions = JsonConvert.SerializeObject(options);
        }


        /// <summary>
        /// The get portal settings.
        /// </summary>
        /// <returns>
        /// Current Portal Settings
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            int iTabId = 0, iPortalId = 0;

            PortalSettings portalSettings;

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

                string sDomainName = Globals.GetDomainName(Request, true);

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


    }
}