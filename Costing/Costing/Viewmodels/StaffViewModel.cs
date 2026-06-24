using Costing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Costing.Viewmodels
{
    class StaffViewModel : ViewModelBase
    {
        private ObservableCollection<BasicEmployee> _ocStaff = new ObservableCollection<BasicEmployee>();

        public ObservableCollection<BasicEmployee> OCStaff
        {
            get { return _ocStaff; }
            set
            {
                _ocStaff = value;
                OnPropertyChanged(nameof(OCStaff));
            }
        }
    }
}
