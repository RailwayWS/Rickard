using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Costing.Models
{
    public class AllocationRow : INotifyPropertyChanged //used for the dialog popup for assigning work centres
    {
        private string _workCentre;
        private decimal _portion;

        public string WorkCentre
        {
            get => _workCentre;
            set { _workCentre = value; OnPropertyChanged(); }
        }

        public decimal Portion
        {
            get => _portion;
            set { _portion = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}