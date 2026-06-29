using Costing.Models;
using System.Collections.ObjectModel;

namespace Costing.Viewmodels
{
    public class AllocationsViewModel : ViewModelBase
    {
        private ObservableCollection<EmployeeAllocationSummary> _ocAllocations
            = new ObservableCollection<EmployeeAllocationSummary>();

        public ObservableCollection<EmployeeAllocationSummary> OCAllocations
        {
            get => _ocAllocations;
            set { _ocAllocations = value; OnPropertyChanged(nameof(OCAllocations)); }
        }

        private ObservableCollection<WorkCentre> _ocWorkCentres
            = new ObservableCollection<WorkCentre>();

        public ObservableCollection<WorkCentre> OCWorkCentres
        {
            get => _ocWorkCentres;
            set { _ocWorkCentres = value; OnPropertyChanged(nameof(OCWorkCentres)); }
        }
    }
}