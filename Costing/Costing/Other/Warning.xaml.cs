using System.Windows;

namespace Costing.Other
{
    public partial class Warning : Window
    {
        public string warning { get; set; }
        public Warning(string message)
        {
            InitializeComponent();
            this.warning = warning;
            DataContext = this;

        }

        private void btok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();


        }


    }
}
