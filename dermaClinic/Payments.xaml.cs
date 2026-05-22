using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class Payments : Window
    {
        public Payments()
        {
            InitializeComponent();
            LoadPayments();
        }

        private void LoadPayments(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    // Join payments with appointments, patients, and services
                    string query = @"
                        SELECT py.payment_id, p.full_name as patient_name, s.service_name, 
                               py.amount_paid, py.payment_method, py.payment_date,
                               py.appointment_id
                        FROM payments py
                        LEFT JOIN appointments a ON py.appointment_id = a.appointment_id
                        LEFT JOIN patients p ON a.patient_id = p.patient_id
                        LEFT JOIN services s ON a.service_id = s.service_id";
                    
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE p.full_name LIKE @filter OR py.payment_method LIKE @filter";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgPayments.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payments: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadPayments(txtSearch.Text.Trim());
        }

        private void btnNewPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentWindow win = new PaymentWindow();
            win.ShowDialog();
            if (win.Success) LoadPayments();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgPayments.SelectedItem as DataRowView;
            if (row != null)
            {
                PaymentWindow win = new PaymentWindow(
                    Convert.ToInt32(row["payment_id"]),
                    Convert.ToInt32(row["appointment_id"]),
                    Convert.ToDecimal(row["amount_paid"]),
                    row["payment_method"].ToString()
                );
                win.ShowDialog();
                if (win.Success) LoadPayments();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgPayments.SelectedItem as DataRowView;
            if (row != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete payment record ID {row["payment_id"]}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = DbConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM payments WHERE payment_id = @id";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", row["payment_id"]);
                            cmd.ExecuteNonQuery();
                            LoadPayments();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Delete Error: " + ex.Message);
                    }
                }
            }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            switch (btn.Name)
            {
                case "btnDashboard":
                    AdminDashboard dashboard = new AdminDashboard();
                    dashboard.Show();
                    this.Close();
                    break;
                case "btnUsers":
                    Users usersWindow = new Users();
                    usersWindow.Show();
                    this.Close();
                    break;
                case "btnPatients":
                    Patients patientWindow = new Patients();
                    patientWindow.Show();
                    this.Close();
                    break;
                case "btnDoctors":
                    Doctors doctorWindow = new Doctors();
                    doctorWindow.Show();
                    this.Close();
                    break;
                case "btnServices":
                    Services serviceWindow = new Services();
                    serviceWindow.Show();
                    this.Close();
                    break;
                case "btnAppointments":
                    Appointments appWindow = new Appointments();
                    appWindow.Show();
                    this.Close();
                    break;
                case "btnReports":
                    Reports reportsWindow = new Reports();
                    reportsWindow.Show();
                    this.Close();
                    break;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
        }
    }
}
