using Costing.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Costing.Viewmodels
{
    public class OverheadViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<GLPeriodControl> _oclPeriods;
        public ObservableCollection<GLPeriodControl> OCLPeriods
        {
            get { return _oclPeriods; }
            set { _oclPeriods = value; OnPropertyChanged(nameof(OCLPeriods)); }
        }

        private ObservableCollection<GLAccount> _oclGlAccounts;
        public ObservableCollection<GLAccount> OCLGlAccounts
        {
            get { return _oclGlAccounts; }
            set { _oclGlAccounts = value; OnPropertyChanged(nameof(OCLGlAccounts)); }
        }

        private GLPeriodControl _selectedPeriod;
        public GLPeriodControl SelectedPeriod
        {
            get { return _selectedPeriod; }
            set { _selectedPeriod = value; OnPropertyChanged(nameof(SelectedPeriod)); }
        }

        public OverheadViewModel()
        {
            OCLPeriods = new ObservableCollection<GLPeriodControl>();
            OCLGlAccounts = new ObservableCollection<GLAccount>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}