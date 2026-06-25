using Costing.Data;
using Costing.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Costing.Viewmodels
{
    public class AllocationsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // The main grid data
        private ObservableCollection<Allocation> _ocAllocations;
        public ObservableCollection<Allocation> OCAllocations
        {
            get { return _ocAllocations; }
            set { _ocAllocations = value; OnPropertyChanged(nameof(OCAllocations)); }
        }

        // The dropdown menu options
        private ObservableCollection<WorkCentre> _ocWorkCentres;
        public ObservableCollection<WorkCentre> OCWorkCentres
        {
            get { return _ocWorkCentres; }
            set { _ocWorkCentres = value; OnPropertyChanged(nameof(OCWorkCentres)); }
        }

        public AllocationsViewModel()
        {
            OCAllocations = new ObservableCollection<Allocation>();
            OCWorkCentres = new ObservableCollection<WorkCentre>();
        }

        public void LoadData()
        {
            using (var context = new CostingDbContext())
            {
                // Load the dropdown options
                var workCentres = context.WorkCentres.OrderBy(w => w.WcDescription).ToList();
                OCWorkCentres = new ObservableCollection<WorkCentre>(workCentres);

                // Load lists from the database
                var allEmployees = context.Staff.ToList();
                var existingAllocations = context.Allocations.ToList();

                var displayList = new System.Collections.Generic.List<Allocation>();

                foreach (var emp in allEmployees)
                {
                    var savedAlloc = existingAllocations.FirstOrDefault(a => a.Code == emp.Code);

                    if (savedAlloc != null)
                    {
                        // They already have an allocation, add it to the screen
                        displayList.Add(savedAlloc);
                    }
                    else
                    {
                        // They don't have one yet. Create a temporary one just for the UI
                        displayList.Add(new Allocation
                        {
                            Code = emp.Code,
                            Name = emp.Name
                        });
                    }
                }

                // Bind it to the grid
                OCAllocations = new ObservableCollection<Allocation>(displayList.OrderBy(a => a.Name));
            }
        }
    }
}