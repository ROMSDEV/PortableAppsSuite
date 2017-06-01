﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppUpdater.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AppUpdater.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouse.
        /// </summary>
        internal static string AppName {
            get {
                return ResourceManager.GetString("AppName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to %CurDir%\YoloMouse.exe.
        /// </summary>
        internal static string AppPath {
            get {
                return ResourceManager.GetString("AppPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap diagonal_pattern {
            get {
                object obj = ResourceManager.GetObject("diagonal_pattern", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /a &quot;{0}&quot; TARGETDIR=&quot;{1}&quot; /qn.
        /// </summary>
        internal static string Extract {
            get {
                return ResourceManager.GetString("Extract", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A newer version is available. Would you like to update now?.
        /// </summary>
        internal static string Msg_Hint_00 {
            get {
                return ResourceManager.GetString("Msg_Hint_00", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No newer version available..
        /// </summary>
        internal static string Msg_Hint_01 {
            get {
                return ResourceManager.GetString("Msg_Hint_01", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouse successfully updated..
        /// </summary>
        internal static string Msg_Hint_02 {
            get {
                return ResourceManager.GetString("Msg_Hint_02", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouse update canceled..
        /// </summary>
        internal static string Msg_Hint_03 {
            get {
                return ResourceManager.GetString("Msg_Hint_03", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouse must be closed..
        /// </summary>
        internal static string Msg_Warn_00 {
            get {
                return ResourceManager.GetString("Msg_Warn_00", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouse update failed..
        /// </summary>
        internal static string Msg_Warn_01 {
            get {
                return ResourceManager.GetString("Msg_Warn_01", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;tr&gt;&lt;td class=&quot;ListHeader&quot;&gt;&lt;a href=&quot;https://github.com/PandaTeemo/YoloMouse/releases/download/(.+?){0}.msi&quot;&gt;YoloMouse {0}bit&lt;/a&gt;&lt;/td&gt;&lt;td class=&quot;ListContent&quot;&gt;for 32 {1}&lt;/td&gt;&lt;/tr&gt;.
        /// </summary>
        internal static string RegexFileNamePattern {
            get {
                return ResourceManager.GetString("RegexFileNamePattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://pandateemo.github.io/YoloMouse/index.html.
        /// </summary>
        internal static string RegexUrl {
            get {
                return ResourceManager.GetString("RegexUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to %TEMP%\YoloMouseUpdater-{{{0}}}.
        /// </summary>
        internal static string TmpDir {
            get {
                return ResourceManager.GetString("TmpDir", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to %CurDir%\Update-{{{0}}}.
        /// </summary>
        internal static string UpdateDir {
            get {
                return ResourceManager.GetString("UpdateDir", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://github.com/PandaTeemo/YoloMouse/releases/download/{0}{1}.msi.
        /// </summary>
        internal static string UpdateUrl {
            get {
                return ResourceManager.GetString("UpdateUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mozilla/5.0.
        /// </summary>
        internal static string UserAgent {
            get {
                return ResourceManager.GetString("UserAgent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouseupdater64.exe
        ///YoloMouseupdater64.exe.config
        ///YoloMouseupdater64.ini
        ///YoloMouseupdater64.pdb
        ///YoloMouseupdater.exe
        ///YoloMouseupdater.exe.config
        ///YoloMouseupdater.ini
        ///YoloMouseupdater.pdb
        ///sildev.csharplib.dll
        ///sildev.csharplib64.dll
        ///settings.ini.
        /// </summary>
        internal static string WhiteList {
            get {
                return ResourceManager.GetString("WhiteList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to YoloMouse Updater.
        /// </summary>
        internal static string WindowTitle {
            get {
                return ResourceManager.GetString("WindowTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon YoloMouseUpdater {
            get {
                object obj = ResourceManager.GetObject("YoloMouseUpdater", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
    }
}
