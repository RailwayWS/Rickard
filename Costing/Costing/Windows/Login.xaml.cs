using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Costing.SQLClasses;
using Costing.Other;
using Costing.Models;
using Costing.Viewmodels;
using Costing.Windows;


namespace Costing.UserControls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        #region Controls
        public ObservableCollection<LoginUser> OCUsers { get; set; } = new ObservableCollection<LoginUser>();
      
        DataSet DS = new DataSet();
        SQLConnection SQLCosting = new SQLConnection();
        LoginViewModel vmLogin = new LoginViewModel();



        #endregion
        public Login()
        {
            InitializeComponent();


            GetUsers();
            this.Loaded += Login_Loaded; // triggers login loaded when window is loaded
            DataContext = vmLogin; // connects window to view model


            vmLogin.OCUsers = new ObservableCollection<LoginUser>(OCUsers);

        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            GetUsers();
        }


        #region Get Data Methods
        private void GetUsers()
        {
            try
            {

               //get users from database
               if(DS.Tables.Contains("Users"))
                {
                    DS.Tables.Remove("Users");
                }
                DS.Tables.Add(SQLCosting.Dt("Select * from LoginUsers", "Users"));



                //Fill the ObservableCollection with the data from the DataSet
                vmLogin.OCUsers.Clear();
                if (DS.Tables["Users"].Rows.Count == 0)
                {
                    Message msg = new Message("No Users Found");
                    msg.ShowDialog();
                    this.Close();
                }
                else
                {
                    foreach (DataRow dr in DS.Tables["Users"].Rows)
                    {
                        vmLogin.OCUsers.Add(new LoginUser
                        {
                            UserName = dr["UserName"].ToString(),
                            Password = dr["Password"].ToString(),
                            Email = dr["Email"].ToString(),

                        });
                    }

                    cmbUsers.SelectedIndex = 0;
                }


            }
            catch (Exception ex)
            {
                Message errmess = new Message($"{ex.ToString()}");
                errmess.ShowDialog();

            }

        }

        private void btcancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Login Method
        private void btOK_Click(object sender, RoutedEventArgs e)
        {

            Mouse.OverrideCursor = Cursors.Wait;



            if (cmbUsers.SelectedItem != null)
            {

                LoginUser lgUser = (LoginUser)cmbUsers.SelectedItem;

                if (txtPassword.Password == lgUser.Password)
                {

                    //load main window
                    MainWindow main = new MainWindow();
                    this.Close();
                    main.Show();
                    





                }


            }
            else
            {
                Message msg = new Message("Incorrect Password");
                msg.ShowDialog();
                txtPassword.Focus();

            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }
          


           

          

        
        #endregion
       
    }
  
}
