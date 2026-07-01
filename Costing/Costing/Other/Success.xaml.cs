using System.Windows;

namespace Costing.Other
{
    public partial class Success : Window
    {
        public string success { get; set; }
        public Success(string message)
        {
            InitializeComponent();
            this.success = success;
            DataContext = this;

        }

        private void btok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();


        }

    }
}
