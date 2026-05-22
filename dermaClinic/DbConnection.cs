using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace dermaClinic
{
    public class DbConnection
    {
        private static string connString = "server=localhost;database=dermaclinic_db;uid=root;pwd=;";

        public static MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(connString);
            try
            {
                return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed: " + ex.Message);
                return null;
            }
        }
    }
}