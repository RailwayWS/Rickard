using System;

namespace Costing.Viewmodels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string _sysproServer;
        public string SysproServer
        {
            get { return _sysproServer; }
            set { _sysproServer = value; OnPropertyChanged(nameof(SysproServer)); }
        }

        private string _sysproDB;
        public string SysproDB
        {
            get { return _sysproDB; }
            set { _sysproDB = value; OnPropertyChanged(nameof(SysproDB)); }
        }

        private string _defaultImportPath;
        public string DefaultImportPath
        {
            get { return _defaultImportPath; }
            set { _defaultImportPath = value; OnPropertyChanged(nameof(DefaultImportPath)); }
        }
    }
}