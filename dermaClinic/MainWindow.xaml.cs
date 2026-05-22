using System;
using System.Windows;
using MySql.Data.MySqlClient; // If this has a red line, you need to install MySql.Data via NuGet

namespace dermaClinic
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // This runs when you click the LOGIN button
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 1. Grab data from your UI
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // 2. Simple check so we don't waste database resources
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 3. Open connection using your PUBLIC CLASS
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();

                    // 4. SQL Query (Checks Username, Password, and if the account is ACTIVE)
                    // This satisfies your "User Authentication" and "Active/Inactive" requirement.
                    string query = "SELECT COUNT(*) FROM users WHERE username = @user AND password = @pass AND status = 'Active'";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);

                    // ExecuteScalar returns the number of rows found
                    int userFound = Convert.ToInt32(cmd.ExecuteScalar());

                    if (userFound > 0)
                    {
                        // SUCCESS: Open the Admin Dashboard
                        MessageBox.Show("Login successful! Welcome to DermaCare.", "Success");

                        // Store session
                        UserSession.Username = username;

                        AdminDashboard dashboard = new AdminDashboard();
                        dashboard.Show();
                        this.Close(); // Close the login window
                    }
                    else
                    {
                        // FAIL: Wrong details or the account is marked 'Inactive' in Workbench
                        MessageBox.Show("Invalid username/password or account is deactivated.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // This will show if your database isn't running or the connection string is wrong
                MessageBox.Show("Database Error: " + ex.Message, "Error");
            }
        }

        // This handles your "Password Recovery" requirement
        private void ForgotPassword_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Jump to the Password Recovery page
            PasswordRecovery recoveryWindow = new PasswordRecovery();
            recoveryWindow.Show();
            this.Close(); // Close login window
        }
    }
}