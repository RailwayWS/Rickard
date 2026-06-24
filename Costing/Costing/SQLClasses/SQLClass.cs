using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Costing.Other;


namespace Costing.SQLClasses
{

    class SQLConnection
    {
        SqlConnection SQLcon = new SqlConnection();
        public string serv = Properties.Settings.Default.CostingServer;
        public string db = Properties.Settings.Default.CostingDB;
        public List<SqlParameter> parms = new List<SqlParameter>();
        public bool error = false;


        public DataTable Dt(string query, string table)
        {
            try
            {
                error = false;
                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False;Connection TimeOut = 120";
                var l = SQLcon.ConnectionTimeout;
                SQLcon.Open();

                SqlCommand cmd = new SqlCommand(query, SQLcon);
                cmd.CommandTimeout = 200;
                DataTable Dt = new DataTable();

                parms.ForEach((p) => cmd.Parameters.Add(p));
                parms.Clear();

                Dt.Load(cmd.ExecuteReader());

                Dt.TableName = table;

                SQLcon.Close();
                return Dt;
            }
            catch (Exception ex)
            {
                Message errormess = new Message($"ERROR! Database not updated {ex.Message}");
                errormess.ShowDialog();
                error = true;


                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();
                }
            }

            return null;


        }

        public void Addparam(string Name, object Value)
        {
            SqlParameter sqlparm = new SqlParameter(Name, Value);
            parms.Add(sqlparm);


        }

        public void ExecuteStoredProcedureOnly(string procedure)
        {
            try
            {

                error = false;
                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False";
                SQLcon.Open();

                SqlCommand cmd = new SqlCommand(procedure, SQLcon);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);


               

                parms.ForEach((p) => cmd.Parameters.Add(p));
                cmd.ExecuteNonQuery();

             
                parms.Clear();
               

                SQLcon.Close();
              



            }
            catch (Exception ex)
            {
                Message errormess = new Message($"ERROR! Database not updated {ex.Message}");
                errormess.ShowDialog();
                error = true;


                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();
                }
            }
          
        }

      
        public DataTable ExecuteStoredProcedure(string procedure)
        {
            try
            {

                error = false;
                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False";
                SQLcon.Open();

                SqlCommand cmd = new SqlCommand(procedure, SQLcon);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);


              
                DataTable Dt = new DataTable();


                parms.ForEach((p) => cmd.Parameters.Add(p));
                parms.Clear();

                Dt.Load(cmd.ExecuteReader());

                Dt.TableName = procedure;

                SQLcon.Close();
                return Dt;



            }
            catch (Exception ex)
            {
                Message errormess = new Message($"ERROR! Database not updated {ex.Message}");
                errormess.ShowDialog();
                error = true;


                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();
                }
            }
            return null;


        }
        public void UpdateTable(string CommonTableName, string query, DataSet ChangedDS)
        {

            try
            {
                error = false;
                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False";
                SQLcon.Open();

                SqlCommand cmd = new SqlCommand(query, SQLcon);
                SqlDataAdapter da = new SqlDataAdapter(cmd);


                DataSet NewDS = new DataSet();

                parms.ForEach((p) => cmd.Parameters.Add(p));
                parms.Clear();

                da.Fill(NewDS, CommonTableName);
                SqlCommandBuilder cb = new SqlCommandBuilder();
                cb.DataAdapter = da;
                da.DeleteCommand = cb.GetDeleteCommand();
                da.InsertCommand = cb.GetInsertCommand();
                da.UpdateCommand = cb.GetUpdateCommand();
               
              

                da.Update(ChangedDS, CommonTableName);



                SQLcon.Close();
            }
            catch(Exception e)
            {

                Message errormess = new Message($"ERROR! Database not updated");
                errormess.ShowDialog();
                error = true;
                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();
                }
            }
        }
        public void ExecuteNonQuery(string query)
        {
           


            try
            {
                error = false;
             
                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False";
                SQLcon.Open();
                SqlCommand cmd = new SqlCommand(query, SQLcon);
                parms.ForEach((p) => cmd.Parameters.Add(p));
                parms.Clear();

              cmd.ExecuteNonQuery();
               
           
                

                    SQLcon.Close();
               
            }
            catch (Exception ex)
            {
                Message errormess = new Message($"ERROR! Database not updated {ex}");
                errormess.ShowDialog();
                error = true;
             
                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();

                }
            }



        }

        public void ExecuteBulkCopyNonQuery(DataTable dt,string Tablename)
        {
            try
            {
                error = false;

                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False";
                SQLcon.Open();
                SqlBulkCopy sqlB = new SqlBulkCopy(SQLcon);

                var colnames = dt.Columns;

                foreach (DataColumn col in colnames)
                {
                   
                        sqlB.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    
                }

            sqlB.DestinationTableName = Tablename;
              sqlB.WriteToServer(dt);
                SQLcon.Close();
              
            }
            catch (Exception ex)
            {
                Message errormess = new Message($"ERROR! Database not updated {ex}");
                errormess.ShowDialog();
                error = true;
              
                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();

                }
            }



        }


    }


}
