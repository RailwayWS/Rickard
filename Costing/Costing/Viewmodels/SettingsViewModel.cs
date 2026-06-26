using Costing.Models;
using System;
using System.Collections.ObjectModel;

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

        private string _importPathWages;
        public string ImportPathWages
        {
            get { return _importPathWages; }
            set { _importPathWages = value; OnPropertyChanged(nameof(ImportPathWages)); }
        }

        private string _importPathCosting;
        public string ImportPathCosting
        {
            get { return _importPathCosting; }
            set { _importPathCosting = value; OnPropertyChanged(nameof(ImportPathCosting)); }
        }

        private ObservableCollection<LoginUser> _oclUsers;
        public ObservableCollection<LoginUser> OclUsers
        {
            get { return _oclUsers; }
            set { _oclUsers = value; OnPropertyChanged(nameof(OclUsers)); }
        }
    }
}