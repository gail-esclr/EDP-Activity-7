using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class PasswordRecovery : Window
    {
        public PasswordRecovery()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email address.", "Required");
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();

                    // Verify the email exists in the database
                    string query = "SELECT COUNT(*) FROM users WHERE email = @email";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@email", email);

                    int exists = Convert.ToInt32(cmd.ExecuteScalar());

                    if (exists > 0)
                    {
                        // Proceed to Reset Password page and pass the email
                        ResentPassword resetPage = new ResentPassword(email);
                        resetPage.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("This email is not registered in our system.", "Not Found");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnBackToLogin_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}