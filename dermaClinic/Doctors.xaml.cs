using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class Doctors : Window
    {
        public Doctors()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT doctor_id, full_name, specialization, contact_number, email FROM doctors";
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE full_name LIKE @filter OR specialization LIKE @filter";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgDoctors.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDoctors(txtSearch.Text.Trim());
        }

        private void btnAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            DoctorWindow win = new DoctorWindow();
            win.ShowDialog();
            if (win.Success) LoadDoctors();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgDoctors.SelectedItem as DataRowView;
            if (row != null)
            {
                DoctorWindow win = new DoctorWindow(
                    Convert.ToInt32(row["doctor_id"]),
                    row["full_name"].ToString(),
                    row["specialization"].ToString(),
                    row["contact_number"].ToString(),
                    row["email"].ToString()
                );
                win.ShowDialog();
                if (win.Success) LoadDoctors();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgDoctors.SelectedItem as DataRowView;
            if (row != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete Dr. {row["full_name"]}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = DbConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM doctors WHERE doctor_id = @id";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", row["doctor_id"]);
                            cmd.ExecuteNonQuery();
                            LoadDoctors();
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
                case "btnAppointments":
                    Appointments appWindow = new Appointments();
                    appWindow.Show();
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
