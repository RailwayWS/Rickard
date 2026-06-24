using Costing.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Costing.Viewmodels
{
    public class CalculatedStaffViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CalculatedStaff> _ocCalculatedStaff;
        public ObservableCollection<CalculatedStaff> OCCalculatedStaff
        {
            get { return _ocCalculatedStaff; }
            set
            {
                _ocCalculatedStaff = value;
                OnPropertyChanged(nameof(OCCalculatedStaff));
            }
        }

        public CalculatedStaffViewModel()
        {
            OCCalculatedStaff = new ObservableCollection<CalculatedStaff>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}