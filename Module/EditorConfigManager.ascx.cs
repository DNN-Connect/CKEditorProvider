﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using DNNConnect.CKEditorProvider.Objects;
using DNNConnect.CKEditorProvider.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace DNNConnect.CKEditorProvider.Module
{
    using DNNConnect.CKEditorProvider.Constants;

    /// <summary>
    /// The Editor Config Manger Module
    /// </summary>
    public partial class EditorConfigManager : ModuleSettingsBase
    {
        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LangCode
        {
            get
            {
                return CultureInfo.CurrentCulture.Name;
            }
        }

        /// <summary>
        ///   Gets the Name for the Current Resource file name
        /// </summary>
        protected string ResXFile
        {
            get
            {
                return
                    ResolveUrl(
                        string.Format(
							"~/Providers/HtmlEditorProviders/DNNConnect.CKE/{0}/Options.aspx.resx",
                            Localization.LocalResourceDirectory));
            }
        }

        /// <summary>
        /// Gets or sets the editor options control
        /// </summary>
        private CKEditorOptions EditorOptions { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                EditorOptions =
                   (CKEditorOptions)
                   Page.LoadControl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/CKEditorOptions.ascx");

                EditorOptions.IsHostMode = true;

                EditorOptions.CurrentPortalOnly = PortalOnly.Checked;

                EditorOptions.CurrentOrSelectedTabId = PortalSettings.ActiveTab.TabID;
                EditorOptions.CurrentOrSelectedPortalId = PortalSettings.PortalId;

                EditorOptions.DefaultHostLoadMode = 0;

                EditorOptions.ID = "CKEditor_Options";

                OptionsPlaceHolder.Controls.Add(EditorOptions);

                if (Page.IsPostBack)
                {
                    return;
                }

                SetLanguage();
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///  the contents of the method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    BindPortalTabsAndModulesTree();
                }

                PortalTabsAndModulesTree.SelectedNodeChanged += PortalTabsAndModulesTree_SelectedNodeChanged;
                PortalOnly.CheckedChanged += PortalOnly_CheckedChanged;
            }
            catch (Exception exception)
            {
                Exceptions.ProcessModuleLoadException(this, exception);
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the PortalOnly control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void PortalOnly_CheckedChanged(object sender, EventArgs e)
        {
            BindPortalTabsAndModulesTree();

            EditorOptions.Visible = true;
            ModuleInstanceInfoPlaceHolder.Visible = false;

            PortalTabsAndModulesTree.Nodes[0].Selected = true;

            ////
            PortalTabsAndModulesTree.SelectedNode.ExpandAll();

            EditorOptions.IsHostMode = true;

            EditorOptions.CurrentPortalOnly = PortalOnly.Checked;

            // Load Portal Settings for the selected Portal if exist
            var portalId = PortalTabsAndModulesTree.SelectedValue.Substring(1);
            var tabId = PortalTabsAndModulesTree.SelectedNode.ChildNodes[0].Value.Substring(1);

            EditorOptions.CurrentOrSelectedPortalId = EditorOptions.CurrentPortalOnly ? Convert.ToInt32(portalId) : -1;
            EditorOptions.CurrentOrSelectedTabId = EditorOptions.CurrentPortalOnly ? Convert.ToInt32(tabId) : -1;

            EditorOptions.DefaultHostLoadMode = 0;

            BindPortalTabsAndModulesTree();

            // Load Settings
            EditorOptions.BindOptionsData(true);
        }

        /// <summary>
        /// Loads the Settings based on the Selected Portal/Tab/Module
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void PortalTabsAndModulesTree_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (PortalTabsAndModulesTree.SelectedNode == null)
            {
                return;
            }

            PortalTabsAndModulesTree.SelectedNode.ExpandAll();

            EditorOptions.IsHostMode = true;
            EditorOptions.CurrentPortalOnly = PortalOnly.Checked;

            if (PortalTabsAndModulesTree.SelectedNode == null)
            {
                return;
            }
            
            if (PortalTabsAndModulesTree.SelectedValue.StartsWith("h"))
            {
                EditorOptions.Visible = true;
                ModuleInstanceInfoPlaceHolder.Visible = false;

                // Load Portal Settings for the selected Portal if exist
                var portalId = PortalTabsAndModulesTree.SelectedValue.Substring(1);
                var tabId = PortalTabsAndModulesTree.SelectedNode.ChildNodes[0].Value.Substring(1);

                int temp;
                EditorOptions.CurrentOrSelectedPortalId = int.TryParse(portalId, out temp) ? temp : -1;
                EditorOptions.CurrentOrSelectedTabId = Convert.ToInt32(tabId);

                EditorOptions.DefaultHostLoadMode = -1;

                BindPortalTabsAndModulesTree();

                // Load Settings
                EditorOptions.BindOptionsData(true);
            }
            if (PortalTabsAndModulesTree.SelectedValue.StartsWith("p"))
            {
                EditorOptions.Visible = true;
                ModuleInstanceInfoPlaceHolder.Visible = false;

                // Load Portal Settings for the selected Portal if exist
                var portalId = PortalTabsAndModulesTree.SelectedValue.Substring(1);
                var tabId = PortalTabsAndModulesTree.SelectedNode.ChildNodes[0].Value.Substring(1);

                EditorOptions.CurrentOrSelectedPortalId = Convert.ToInt32(portalId);
                EditorOptions.CurrentOrSelectedTabId = Convert.ToInt32(tabId);

                EditorOptions.DefaultHostLoadMode = 0;

                BindPortalTabsAndModulesTree();

                // Load Settings
                EditorOptions.BindOptionsData(true);
            }
            else if (PortalTabsAndModulesTree.SelectedValue.StartsWith("t"))
            {
                EditorOptions.Visible = true;
                ModuleInstanceInfoPlaceHolder.Visible = false;

                // Load Tab Settings for the selected Tab if exist
                var portalId = PortalTabsAndModulesTree.SelectedNode.Parent.Value.Substring(1);
                var tabId = PortalTabsAndModulesTree.SelectedValue.Substring(1);

                EditorOptions.CurrentOrSelectedPortalId = Convert.ToInt32(portalId);
                EditorOptions.CurrentOrSelectedTabId = Convert.ToInt32(tabId);

                EditorOptions.DefaultHostLoadMode = 1;

                BindPortalTabsAndModulesTree();

                // Load Settings
                EditorOptions.BindOptionsData(true);
            }
            else if (PortalTabsAndModulesTree.SelectedValue.StartsWith("m"))
            {
                EditorOptions.Visible = false;

                // Show Info Notice
                ModuleInstanceInfoPlaceHolder.Visible = true;
            }
        }

        /// <summary>
        /// Sets the language for all Controls
        /// </summary>
        private void SetLanguage()
        {
            ModuleHeader.Text = Localization.GetString("ModuleHeader.Text", ResXFile, LangCode);
            PortalOnlyLabel.Text = Localization.GetString("PortalOnlyLabel.Text", ResXFile, LangCode);
            PortalOnly.Text = Localization.GetString("PortalOnly.Text", ResXFile, LangCode);
            HostHasSettingLabel.Text = Localization.GetString(
                "HostHasSettingLabel.Text", ResXFile, LangCode);
            HostNoSettingLabel.Text = Localization.GetString(
                "HostNoSettingLabel.Text", ResXFile, LangCode);
            PortalHasSettingLabel.Text = Localization.GetString(
                "PortalHasSettingLabel.Text", ResXFile, LangCode);
            PortalNoSettingLabel.Text = Localization.GetString(
                "PortalNoSettingLabel.Text", ResXFile, LangCode);
            PageHasSettingLabel.Text = Localization.GetString(
                "PageHasSettingLabel.Text", ResXFile, LangCode);
            PageNoSettingLabel.Text = Localization.GetString(
                "PageNoSettingLabel.Text", ResXFile, LangCode);
            ModuleHasSettingLabel.Text = Localization.GetString(
                "ModuleHasSettingLabel.Text", ResXFile, LangCode);
            ModuleNoSettingLabel.Text = Localization.GetString(
                "ModuleNoSettingLabel.Text", ResXFile, LangCode);
            IconLegendLabel.Text = Localization.GetString(
                "IconLegendLabel.Text", ResXFile, LangCode);
            ModuleInstanceInfo.Text = Localization.GetString("ModuleError.Text", ResXFile, LangCode);
        }

        /// <summary>
        /// Renders the Portal <see cref="Tabs"/> and Module Tree
        /// </summary>
        private void BindPortalTabsAndModulesTree()
        {
            PortalTabsAndModulesTree.Nodes.Clear();

            var moduleController = new ModuleController();

            var settingsDictionary = EditorController.GetEditorHostSettings();

            if (PortalOnly.Checked)
            {
                RenderPortalNode(
                    new PortalController().GetPortal(PortalSettings.PortalId), moduleController, settingsDictionary);
            }
            else
            {
                var portals = new PortalController().GetPortals().Cast<PortalInfo>().ToList();
                RenderHostNode(portals, moduleController, settingsDictionary);
            }

            PortalTabsAndModulesTree.DataBind();
        }

        private void RenderHostNode(IEnumerable<PortalInfo> portals, ModuleController moduleController, List<EditorHostSetting> editorHostSettings)
        {
            const string hostKey = "DNNCKH#";
            var hostSettingsExist = SettingsUtil.CheckSettingsExistByKey(editorHostSettings, hostKey);

            var hostNode = new TreeNode()
                           {
                               Text = Localization.GetString("AllPortals.Text", ResXFile, LangCode),
                               Value = "h",
                               ImageUrl = 
                               hostSettingsExist
                                    ? "../js/ckeditor/4.5.3/images/HostHasSetting.png"
                                    : "../js/ckeditor/4.5.3/images/HostNoSetting.png",
                               Expanded = true,
            };

            foreach (var portal in portals)
            {
                RenderPortalNode(portal, moduleController, editorHostSettings, hostNode);
            }

            PortalTabsAndModulesTree.Nodes.Add(hostNode);
        }

        /// <summary>
        /// Renders the <paramref name="portal" /> node.
        /// </summary>
        /// <param name="portal">The <paramref name="portal" />.</param>
        /// <param name="moduleController">The module controller.</param>
        /// <param name="editorHostSettings">The editor host settings.</param>
        /// <param name="parentNode">The parent node.</param>
        private void RenderPortalNode(PortalInfo portal, ModuleController moduleController, List<EditorHostSetting> editorHostSettings, TreeNode parentNode = null)
        {
            var portalKey = $"DNNCKP#{portal.PortalID}#";

            var portalSettingsExists = SettingsUtil.CheckSettingsExistByKey(editorHostSettings, portalKey);

            // Portals
            var portalNode = new TreeNode
            {
                Text = portal.PortalName,
                Value = $"p{portal.PortalID}",
                ImageUrl =
                    portalSettingsExists
                        ? "../js/ckeditor/4.5.3/images/PortalHasSetting.png"
                        : "../js/ckeditor/4.5.3/images/PortalNoSetting.png",
                Expanded = PortalOnly.Checked
            };

            foreach (var tabInfo in TabController.GetTabsByParent(-1, portal.PortalID))
            {
                RenderTabNode(portalNode, tabInfo, moduleController, editorHostSettings);
            }

            if (parentNode == null)
            {
                PortalTabsAndModulesTree.Nodes.Add(portalNode);
            }
            else
            {
                parentNode.ChildNodes.Add(portalNode);
            }
        }

        /// <summary>
        /// Renders the tab node.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="tabInfo">The tab info.</param>
        /// <param name="moduleController">The module controller.</param>
        /// <param name="editorHostSettings">The editor host settings.</param>
        private void RenderTabNode(
            TreeNode parentNode,
            TabInfo tabInfo,
            ModuleController moduleController,
            List<EditorHostSetting> editorHostSettings)
        {
            var tabKey = $"DNNCKT#{tabInfo.TabID}#";

            var tabSettingsExists = SettingsUtil.CheckSettingsExistByKey(editorHostSettings, tabKey);

            // Tabs
            var tabNode = new TreeNode
                              {
                                  Text = tabInfo.TabName,
                                  Value = $"t{tabInfo.TabID}",
                                  ImageUrl =
                                      tabSettingsExists
                                          ? "../js/ckeditor/4.5.3/images/PageHasSetting.png"
                                          : "../js/ckeditor/4.5.3/images/PageNoSetting.png"
                              };

            if (tabInfo.HasChildren)
            {
                foreach (var childTab in TabController.GetTabsByParent(tabInfo.TabID, tabInfo.PortalID))
                {
                    RenderTabNode(tabNode, childTab, moduleController, editorHostSettings);
                }
            }

            var modules = moduleController.GetTabModules(tabInfo.TabID).Values;

            foreach (var moduleNode in from moduleInfo in modules
                                       let moduleKey = $"DNNCKMI#{moduleInfo.ModuleID}#INS#"
                                       let moduleSettingsExists =
                                           SettingsUtil.CheckExistsModuleSettings(moduleKey, moduleInfo.ModuleID)
                                       select
                                           new TreeNode
                                               {
                                                   Text = moduleInfo.ModuleTitle,
                                                   ImageUrl =
                                                       moduleSettingsExists
                                                           ? "../js/ckeditor/4.5.3/images/ModuleHasSetting.png"
                                                           : "../js/ckeditor/4.5.3/images/ModuleNoSetting.png",
                                                   Value = $"m{moduleInfo.ModuleID}" })
            {
                tabNode.ChildNodes.Add(moduleNode);
            }

            parentNode.ChildNodes.Add(tabNode);
        }
    }
}