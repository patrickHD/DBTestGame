using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sql_test
{
    class DBConnection
    {
        private DBConnection()
        {
        }

        private string databaseName = string.Empty;
        public string Failstr { get; set; }
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public string Password { get; set; }
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            DatabaseName = "mytestdb18";
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(databaseName))
                    return false;
                string connstring = string.Format("Server=db4free.net; database={0}; UID=zanor18; password=Wsaddata12@", databaseName);
                connection = new MySqlConnection(connstring);
                if (connection != null) {
                connection.Close(); }
                try
                {
                    connection.Open();
                } catch(Exception ex)
                {
                    MessageBox.Show("Failure at dbcon \n" + ex.ToString());
                    Failstr = "Failure at dbcon \n" + ex.ToString() + "\n" + ex.Message;
                    return false;
                }
            }

            return true;
        }
        public void Open()
        {
            if (connection != null && connection.State != System.Data.ConnectionState.Open)
            try
            {
                connection.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failure at Conopen\n" + ex.ToString());
            }
        }
        public void Close()
        {
            if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                connection.Close();
        }
    }
}
