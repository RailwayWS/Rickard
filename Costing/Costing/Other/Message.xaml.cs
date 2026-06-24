using System.Windows;

namespace Costing.Other
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class Message : Window
    {
        public string message { get; set; }
        public Message(string message)
        {
            InitializeComponent();
            this.message = message;
            DataContext = this;

        }

        private void btok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();


        }

        private void btcancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();

        }


    }
}
