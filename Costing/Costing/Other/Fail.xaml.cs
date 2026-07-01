using System.Windows;

namespace Costing.Other
{
    public partial class Fail : Window
    {
        public string fail { get; set; }
        public Fail(string message)
        {
            InitializeComponent();
            this.fail = fail;
            DataContext = this;

        }

        private void btok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();


        }
    }
}
