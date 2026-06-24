using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace Costing.SQLClasses
{
    class SQLSyspro
    {
        SqlConnection SQLcon = new SqlConnection();
        public string serv = Properties.Settings.Default.SysproServer;
        public string db = Properties.Settings.Default.SysproDB;
        public List<SqlParameter> parms = new List<SqlParameter>();



        public DataTable Dt(string query, string table)
        {
           

            try
            {
                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True; Encrypt = False ;Connection Timeout = 120";
                var t = SQLcon.ConnectionTimeout;
                SQLcon.Open();

                SqlCommand cmd = new SqlCommand(query, SQLcon);
                cmd.CommandTimeout = 200 ;
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
                MessageBox.Show(ex.Message);


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


        public void UpdateTable(string CommonTableName, string query, DataSet ChangedDS)
        {

            try
            {
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

                da.Update(ChangedDS, CommonTableName);

                SQLcon.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
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

                SQLcon.ConnectionString = $"Data Source = {serv}; Initial Catalog = {db}; Integrated Security = True;";
                SQLcon.Open();
                SqlCommand cmd = new SqlCommand(query, SQLcon);
                parms.ForEach((p) => cmd.Parameters.Add(p));
                parms.Clear();

                cmd.ExecuteNonQuery();

                SQLcon.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (SQLcon.State == ConnectionState.Open)
                {
                    SQLcon.Close();
                }
            }

        }
    }
}
