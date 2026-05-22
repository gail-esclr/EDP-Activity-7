using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class Appointments : Window
    {
        public Appointments()
        {
            InitializeComponent();
            LoadAppointments();
        }

        private void LoadAppointments(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    // Join with patients, doctors, and services to get names
                    string query = @"
                        SELECT a.appointment_id, p.full_name as patient_name, d.full_name as doctor_name, 
                               s.service_name, a.appointment_date, a.appointment_time, a.status,
                               a.patient_id, a.doctor_id, a.service_id
                        FROM appointments a
                        LEFT JOIN patients p ON a.patient_id = p.patient_id
                        LEFT JOIN doctors d ON a.doctor_id = d.doctor_id
                        LEFT JOIN services s ON a.service_id = s.service_id";
                    
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE p.full_name LIKE @filter OR d.full_name LIKE @filter OR s.service_name LIKE @filter";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgAppointments.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAppointments(txtSearch.Text.Trim());
        }

        private void btnAddAppointment_Click(object sender, RoutedEventArgs e)
        {
            AppointmentWindow win = new AppointmentWindow();
            win.ShowDialog();
            if (win.Success) LoadAppointments();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgAppointments.SelectedItem as DataRowView;
            if (row != null)
            {
                AppointmentWindow win = new AppointmentWindow(
                    Convert.ToInt32(row["appointment_id"]),
                    Convert.ToInt32(row["patient_id"]),
                    Convert.ToInt32(row["doctor_id"]),
                    Convert.ToInt32(row["service_id"]),
                    (DateTime)row["appointment_date"],
                    row["appointment_time"].ToString(),
                    row["status"].ToString()
                );
                win.ShowDialog();
                if (win.Success) LoadAppointments();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgAppointments.SelectedItem as DataRowView;
            if (row != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete appointment ID {row["appointment_id"]}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = DbConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM appointments WHERE appointment_id = @id";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", row["appointment_id"]);
                            cmd.ExecuteNonQuery();
                            LoadAppointments();
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
                case "btnPayments":
                    Payments paymentWindow = new Payments();
                    paymentWindow.Show();
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
