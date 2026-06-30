using Costing.Models;
using System.Collections.ObjectModel;

namespace Costing.Viewmodels
{
    class AuditLogsViewModel : ViewModelBase
    {
        private ObservableCollection<AuditLog> _ocAuditLogs = new ObservableCollection<AuditLog>();

        public ObservableCollection<AuditLog> OCAuditLogs
        {
            get { return _ocAuditLogs; }
            set
            {
                _ocAuditLogs = value;
                OnPropertyChanged(nameof(OCAuditLogs));
            }
        }

        private AuditLog _selectedSnapshot;

        public AuditLog SelectedSnapshot
        {
            get { return _selectedSnapshot; }
            set
            {
                _selectedSnapshot = value;
                OnPropertyChanged(nameof(SelectedSnapshot));
            }
        }
    }
}