using Costing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Costing.Viewmodels
{
    class StaffCostsViewModel : ViewModelBase
    {
        private ObservableCollection<StaffCost> _ocStaffCosts;
        private StaffCost _selectedStaffCost;

        public ObservableCollection<StaffCost> OCStaffCosts
        {
            get { return _ocStaffCosts; }
            set
            {
                _ocStaffCosts = value;
                OnPropertyChanged(nameof(OCStaffCosts));
            }
        }

        public StaffCost SelectedStaffCost
        {
            get { return _selectedStaffCost; }
            set
            {
                _selectedStaffCost = value;
                OnPropertyChanged(nameof(SelectedStaffCost));
            }
        }

        public ObservableCollection<string> TypesList { get; set; }

        public StaffCostsViewModel()
        {
            TypesList = new ObservableCollection<string> { "Fixed", "Percentage" };

            OCStaffCosts = new ObservableCollection<StaffCost>();
        }
    }
}
