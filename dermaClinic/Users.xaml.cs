using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class Users : Window
    {
        public Users()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers(string filter = "")
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT user_id, username, password, email, role, status FROM users";
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE username LIKE @filter";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgUsers.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUsers(txtSearch.Text.Trim());
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            UserWindow win = new UserWindow();
            win.ShowDialog();
            if (win.Success) LoadUsers();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgUsers.SelectedItem as DataRowView;
            if (row != null)
            {
                UserWindow win = new UserWindow(
                    Convert.ToInt32(row["user_id"]),
                    row["username"].ToString(),
                    row["email"].ToString(),
                    row["password"].ToString(),
                    row["role"].ToString(),
                    row["status"].ToString()
                );
                win.ShowDialog();
                if (win.Success) LoadUsers();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = dgUsers.SelectedItem as DataRowView;
            if (row != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete user '{row["username"]}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = DbConnection.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM users WHERE user_id = @id";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", row["user_id"]);
                            cmd.ExecuteNonQuery();
                            LoadUsers();
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
