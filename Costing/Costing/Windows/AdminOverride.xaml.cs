using Costing.Helpers;
using Costing.Models;
using System.Windows;

namespace Costing.Windows
{
    public partial class AdminOverride : Window
    {
        public AdminOverride()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtAdminPassword.Password == "Admin123")
            {
                AppSession.CurrentUser = new LoginUser
                {
                    UserName = "Admin",
                    Password = "Admin123",
                    Role = "Admin"
                };

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect Password", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                txtAdminPassword.Focus();
            }
        }
    }
}