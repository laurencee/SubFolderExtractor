﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18213
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SubFolderExtractor.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>rar</string>
  <string>r00</string>
  <string>zip</string>
  <string>7z</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection CompressionExtensions {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["CompressionExtensions"]));
            }
            set {
                this["CompressionExtensions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7zip\\7z.exe")]
        public string ExtractionToolPath {
            get {
                return ((string)(this["ExtractionToolPath"]));
            }
            set {
                this["ExtractionToolPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RenameToFolder {
            get {
                return ((bool)(this["RenameToFolder"]));
            }
            set {
                this["RenameToFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>.*part\\d+.rar</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection ChainedFileRegularExpressions {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["ChainedFileRegularExpressions"]));
            }
            set {
                this["ChainedFileRegularExpressions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DeleteAfterExtract {
            get {
                return ((bool)(this["DeleteAfterExtract"]));
            }
            set {
                this["DeleteAfterExtract"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("x \"[CompressedFile]\" -o\"[Destination]\" -y")]
        public string ExtractionCommand {
            get {
                return ((string)(this["ExtractionCommand"]));
            }
            set {
                this["ExtractionCommand"] = value;
            }
        }
    }
}
