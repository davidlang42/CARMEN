﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CarmenUI.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double Top {
            get {
                return ((double)(this["Top"]));
            }
            set {
                this["Top"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double Left {
            get {
                return ((double)(this["Left"]));
            }
            set {
                this["Left"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double Height {
            get {
                return ((double)(this["Height"]));
            }
            set {
                this["Height"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double Width {
            get {
                return ((double)(this["Width"]));
            }
            set {
                this["Width"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool FirstRun {
            get {
                return ((bool)(this["FirstRun"]));
            }
            set {
                this["FirstRun"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.WindowState WindowState {
            get {
                return ((global::System.Windows.WindowState)(this["WindowState"]));
            }
            set {
                this["WindowState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Generic.List<CarmenUI.ViewModels.RecentShow> RecentShows {
            get {
                return ((global::System.Collections.Generic.List<CarmenUI.ViewModels.RecentShow>)(this["RecentShows"]));
            }
            set {
                this["RecentShows"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LastCommaFirst")]
        public global::CarmenUI.Converters.FullNameFormat FullNameFormat {
            get {
                return ((global::CarmenUI.Converters.FullNameFormat)(this["FullNameFormat"]));
            }
            set {
                this["FullNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SaveOnApplicantChange {
            get {
                return ((bool)(this["SaveOnApplicantChange"]));
            }
            set {
                this["SaveOnApplicantChange"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowUnavailableApplicants {
            get {
                return ((bool)(this["ShowUnavailableApplicants"]));
            }
            set {
                this["ShowUnavailableApplicants"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowIneligibleApplicants {
            get {
                return ((bool)(this["ShowIneligibleApplicants"]));
            }
            set {
                this["ShowIneligibleApplicants"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowUnneededApplicants
        {
            get
            {
                return ((bool)(this["ShowUnneededApplicants"]));
            }
            set
            {
                this["ShowUnneededApplicants"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SaveOnCtrlS {
            get {
                return ((bool)(this["SaveOnCtrlS"]));
            }
            set {
                this["SaveOnCtrlS"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SaveAndExitOnCtrlEnter {
            get {
                return ((bool)(this["SaveAndExitOnCtrlEnter"]));
            }
            set {
                this["SaveAndExitOnCtrlEnter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool NewOnCtrlN {
            get {
                return ((bool)(this["NewOnCtrlN"]));
            }
            set {
                this["NewOnCtrlN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShortcutsOnStartWindow {
            get {
                return ((bool)(this["ShortcutsOnStartWindow"]));
            }
            set {
                this["ShortcutsOnStartWindow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShortcutsOnMainMenu {
            get {
                return ((bool)(this["ShortcutsOnMainMenu"]));
            }
            set {
                this["ShortcutsOnMainMenu"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool FilterOnCtrlF {
            get {
                return ((bool)(this["FilterOnCtrlF"]));
            }
            set {
                this["FilterOnCtrlF"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SelectAllOnFocusTextBox {
            get {
                return ((bool)(this["SelectAllOnFocusTextBox"]));
            }
            set {
                this["SelectAllOnFocusTextBox"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ExitToStartOnBackspace {
            get {
                return ((bool)(this["ExitToStartOnBackspace"]));
            }
            set {
                this["ExitToStartOnBackspace"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool OpenSettingsOnF1 {
            get {
                return ((bool)(this["OpenSettingsOnF1"]));
            }
            set {
                this["OpenSettingsOnF1"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int RegisterApplicantsGroupByIndex {
            get {
                return ((int)(this["RegisterApplicantsGroupByIndex"]));
            }
            set {
                this["RegisterApplicantsGroupByIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int AuditionApplicantsGroupByIndex {
            get {
                return ((int)(this["AuditionApplicantsGroupByIndex"]));
            }
            set {
                this["AuditionApplicantsGroupByIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool MoveOnCtrlArrow {
            get {
                return ((bool)(this["MoveOnCtrlArrow"]));
            }
            set {
                this["MoveOnCtrlArrow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AuditionEngine {
            get {
                return ((string)(this["AuditionEngine"]));
            }
            set {
                this["AuditionEngine"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SelectionEngine {
            get {
                return ((string)(this["SelectionEngine"]));
            }
            set {
                this["SelectionEngine"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AllocationEngine {
            get {
                return ((string)(this["AllocationEngine"]));
            }
            set {
                this["AllocationEngine"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int AutoCollapseGroupThreshold {
            get {
                return ((int)(this["AutoCollapseGroupThreshold"]));
            }
            set {
                this["AutoCollapseGroupThreshold"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("VerifyFull")]
        public global::MySqlConnector.MySqlSslMode ConnectionSslMode {
            get {
                return ((global::MySqlConnector.MySqlSslMode)(this["ConnectionSslMode"]));
            }
            set {
                this["ConnectionSslMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.ObjectModel.ObservableCollection<CarmenUI.ViewModels.ReportDefinition> ReportDefinitions {
            get {
                return ((global::System.Collections.ObjectModel.ObservableCollection<CarmenUI.ViewModels.ReportDefinition>)(this["ReportDefinitions"]));
            }
            set {
                this["ReportDefinitions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RefreshOnF5 {
            get {
                return ((bool)(this["RefreshOnF5"]));
            }
            set {
                this["RefreshOnF5"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ReportOnCtrlR {
            get {
                return ((bool)(this["ReportOnCtrlR"]));
            }
            set {
                this["ReportOnCtrlR"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ImageCachePath {
            get {
                return ((string)(this["ImageCachePath"]));
            }
            set {
                this["ImageCachePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SelectCastSortBySuitability {
            get {
                return ((bool)(this["SelectCastSortBySuitability"]));
            }
            set {
                this["SelectCastSortBySuitability"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SelectCastGroupByCastGroup {
            get {
                return ((bool)(this["SelectCastGroupByCastGroup"]));
            }
            set {
                this["SelectCastGroupByCastGroup"] = value;
            }
        }
    }
}
