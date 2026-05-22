using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class Patients : Window
    {
        public Patients()
        {
            InitializeComponent();
            LoadPatients();
        }

        private void LoadPatients(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT patient_id, full_name, gender, birthdate, contact_number, email, address FROM patients";
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE full_name LIKE @filter OR email LIKE @filter";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgPatients.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patients: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadPatients(txtSearch.Text.Trim());
        }

        private void btnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            PatientWindow win = new PatientWindow();
            win.ShowDialog();
            if (win.Success) LoadPatients();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgPatients.SelectedItem as DataRowView;
            if (row != null)
            {
                PatientWindow win = new PatientWindow(
                    Convert.ToInt32(row["patient_id"]),
                    row["full_name"].ToString(),
                    row["gender"].ToString(),
                    row["birthdate"] != DBNull.Value ? (DateTime)row["birthdate"] : DateTime.Now,
                    row["contact_number"].ToString(),
                    row["email"].ToString(),
                    row["address"].ToString()
                );
                win.ShowDialog();
                if (win.Success) LoadPatients();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgPatients.SelectedItem as DataRowView;
            if (row != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete patient '{row["full_name"]}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = DbConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM patients WHERE patient_id = @id";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", row["patient_id"]);
                            cmd.ExecuteNonQuery();
                            LoadPatients();
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
                case "btnAppointments":
                    Appointments appWindow = new Appointments();
                    appWindow.Show();
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
