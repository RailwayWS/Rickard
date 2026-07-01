using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Costing.Models
{
    public class EmployeeAllocationSummary : INotifyPropertyChanged // used to display the summary of work centres in allocations
    {
        private string _code;
        private string _name;
        private string _allocationSummary;
        private decimal _totalPortion;

        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string AllocationSummary
        {
            get => _allocationSummary;
            set { _allocationSummary = value; OnPropertyChanged(); }
        }

        public decimal TotalPortion
        {
            get => _totalPortion;
            set { _totalPortion = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}