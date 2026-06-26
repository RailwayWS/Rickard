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

        private ObservableCollection<WorkCentre> _ocWorkCentres;
        public ObservableCollection<WorkCentre> OCWorkCentres
        {
            get { return _ocWorkCentres; }
            set { _ocWorkCentres = value; OnPropertyChanged(nameof(OCWorkCentres)); }
        }

        private string _costingServer;
        public string CostingServer
        {
            get { return _costingServer; }
            set { _costingServer = value; OnPropertyChanged(nameof(CostingServer)); }
        }

        private string _costingDB;
        public string CostingDB
        {
            get { return _costingDB; }
            set { _costingDB = value; OnPropertyChanged(nameof(CostingDB)); }
        }

        private string _tempFolder;
        public string TempFolder
        {
            get { return _tempFolder; }
            set { _tempFolder = value; OnPropertyChanged(nameof(TempFolder)); }
        }
    }
}