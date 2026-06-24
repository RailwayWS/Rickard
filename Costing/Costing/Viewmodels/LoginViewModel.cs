using Costing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Costing.Viewmodels
{
    public class LoginViewModel:ViewModelBase
    {
      private  ObservableCollection<LoginUser> _ocUsers = new ObservableCollection<LoginUser>();
        
        public ObservableCollection<LoginUser> OCUsers
        {
            get { return _ocUsers; }
            set
            {
                _ocUsers = value;
                OnPropertyChanged(nameof(OCUsers));
            }
        }


    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }
    }
}
