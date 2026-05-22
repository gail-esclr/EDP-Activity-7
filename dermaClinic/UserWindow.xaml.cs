using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class UserWindow : Window
    {
        private int _userId = -1; 
        public bool Success { get; private set; } = false;

        public UserWindow()
        {
            InitializeComponent();
        }

        public UserWindow(int id, string username, string email, string password, string role, string status)
        {
            InitializeComponent();
            _userId = id;
            lblTitle.Text = "Update Account";
            txtUsername.Text = username;
            txtEmail.Text = email;
            txtPassword.Text = password;
            lblPassHint.Visibility = Visibility.Collapsed;


            foreach (ComboBoxItem item in cmbRole.Items)
            {
                if (item.Content.ToString() == role) { cmbRole.SelectedItem = item; break; }
            }


            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == status) { cmbStatus.SelectedItem = item; break; }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            string status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || (_userId == -1 && string.IsNullOrEmpty(password)))
            {
                MessageBox.Show("Please fill in all required fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query;
                    
                    if (_userId == -1)
                    {
                        query = "INSERT INTO users (username, email, password, role, status) VALUES (@user, @email, @pass, @role, @status)";
                    }
                    else 
                    {
                        query = "UPDATE users SET username=@user, email=@email, password=@pass, role=@role, status=@status WHERE user_id=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@status", status);
                    if (_userId != -1) cmd.Parameters.AddWithValue("@id", _userId);

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
