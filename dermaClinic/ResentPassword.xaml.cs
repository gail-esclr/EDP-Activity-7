using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    /// <summary>
    /// Interaction logic for ResentPassword.xaml
    /// </summary>
    public partial class ResentPassword : Window
    {
        // Store the email passed from PasswordRecovery page
        private string _userEmail;

        public ResentPassword(string email)
        {
            InitializeComponent();
            _userEmail = email;
        }

        // Default constructor (fallback)
        public ResentPassword()
        {
            InitializeComponent();
            _userEmail = "";
        }

        // This runs when you click "RESET PASSWORD"
        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // 1. Make sure fields are not empty
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in both password fields.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Check minimum password length
            if (newPassword.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Too Short", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Check if both passwords match
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Mismatch", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();

                    // 4. Update the password in the database for this email
                    string query = "UPDATE users SET password = @newPass WHERE email = @email";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@newPass", newPassword);
                    cmd.Parameters.AddWithValue("@email", _userEmail);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Password has been reset successfully! You can now log in with your new password.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Go back to login
                        MainWindow loginWindow = new MainWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Could not update the password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // This runs when you click "Cancel and Return to Login"
        private void btnCancelReturn_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
