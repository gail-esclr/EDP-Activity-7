using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class ServiceWindow : Window
    {
        private int _serviceId = -1; 
        public bool Success { get; private set; } = false;

        public ServiceWindow()
        {
            InitializeComponent();
        }

        public ServiceWindow(int id, string serviceName, string description, decimal price)
        {
            InitializeComponent();
            _serviceId = id;
            lblTitle.Text = "Update Service";
            txtServiceName.Text = serviceName;
            txtDescription.Text = description;
            txtPrice.Text = price.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = txtServiceName.Text.Trim();
            string description = txtDescription.Text.Trim();
            string priceStr = txtPrice.Text.Trim();

            if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(priceStr))
            {
                MessageBox.Show("Service Name and Price are required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(priceStr, out decimal price))
            {
                MessageBox.Show("Please enter a valid numeric price.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query;
                    
                    if (_serviceId == -1) 
                    {
                        query = "INSERT INTO services (service_name, description, price) VALUES (@name, @desc, @price)";
                    }
                    else 
                    {
                        query = "UPDATE services SET service_name=@name, description=@desc, price=@price WHERE service_id=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", serviceName);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@price", price);
                    if (_serviceId != -1) cmd.Parameters.AddWithValue("@id", _serviceId);

                    cmd.ExecuteNonQuery();
                    Success = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
