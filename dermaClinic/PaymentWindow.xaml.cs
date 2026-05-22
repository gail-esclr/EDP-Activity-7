using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class PaymentWindow : Window
    {
        private int _paymentId = -1; // -1 means adding new payment
        public bool Success { get; private set; } = false;

        public PaymentWindow()
        {
            InitializeComponent();
            LoadAppointments();
        }

        // Overload for Editing
        public PaymentWindow(int id, int appointmentId, decimal amount, string method)
        {
            InitializeComponent();
            _paymentId = id;
            lblTitle.Text = "Update Payment";
            
            LoadAppointments();

            cmbAppointment.SelectedValue = appointmentId;
            txtAmount.Text = amount.ToString();

            // Set Method
            foreach (ComboBoxItem item in cmbMethod.Items)
            {
                if (item.Content.ToString() == method) { cmbMethod.SelectedItem = item; break; }
            }
        }

        private void LoadAppointments()
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    // Load appointments with patient and service names for selection
                    string query = @"
                        SELECT a.appointment_id, 
                               CONCAT(p.full_name, ' - ', s.service_name) as display_info
                        FROM appointments a
                        JOIN patients p ON a.patient_id = p.patient_id
                        JOIN services s ON a.service_id = s.service_id";
                    
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbAppointment.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAppointment.SelectedValue == null)
            {
                MessageBox.Show("Please select an appointment.", "Warning");
                return;
            }

            string amountStr = txtAmount.Text.Trim();
            string method = (cmbMethod.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(amountStr))
            {
                MessageBox.Show("Amount is required.", "Warning");
                return;
            }

            if (!decimal.TryParse(amountStr, out decimal amount))
            {
                MessageBox.Show("Please enter a valid numeric amount.", "Warning");
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query;
                    
                    if (_paymentId == -1) // Insert
                    {
                        query = "INSERT INTO payments (appointment_id, amount_paid, payment_method) VALUES (@aid, @amount, @method)";
                    }
                    else // Update
                    {
                        query = "UPDATE payments SET appointment_id=@aid, amount_paid=@amount, payment_method=@method WHERE payment_id=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@aid", cmbAppointment.SelectedValue);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@method", method);
                    if (_paymentId != -1) cmd.Parameters.AddWithValue("@id", _paymentId);

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
