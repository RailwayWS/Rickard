using Costing.Models;
using System.Collections.ObjectModel;

namespace Costing.Viewmodels
{
    public class AllocationsViewModel : ViewModelBase
    {
        private ObservableCollection<Allocation> _ocAllocations = new ObservableCollection<Allocation>();
        public ObservableCollection<Allocation> OCAllocations
        {
            get { return _ocAllocations; }
            set { _ocAllocations = value; OnPropertyChanged(nameof(OCAllocations)); }
        }

        private ObservableCollection<WorkCentre> _ocWorkCentres = new ObservableCollection<WorkCentre>();
        public ObservableCollection<WorkCentre> OCWorkCentres
        {
            get { return _ocWorkCentres; }
            set { _ocWorkCentres = value; OnPropertyChanged(nameof(OCWorkCentres)); }
        }
    }
}