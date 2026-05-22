using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();
            LoadStats();
        }

        private void LoadStats()
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();

                    // 1. Total Patients
                    string q1 = "SELECT COUNT(*) FROM patients";
                    MySqlCommand cmd1 = new MySqlCommand(q1, conn);
                    lblTotalPatients.Text = cmd1.ExecuteScalar()?.ToString() ?? "0";

                    // 2. Today's Appointments
                    string q2 = "SELECT COUNT(*) FROM appointments WHERE DATE(appointment_date) = CURDATE()";
                    MySqlCommand cmd2 = new MySqlCommand(q2, conn);
                    lblTodaysAppointments.Text = cmd2.ExecuteScalar()?.ToString() ?? "0";

                    // 3. Total Revenue (Sum of payments)
                    string q3 = "SELECT SUM(amount_paid) FROM payments";
                    MySqlCommand cmd3 = new MySqlCommand(q3, conn);
                    object revenue = cmd3.ExecuteScalar();
                    decimal totalRevenue = revenue != DBNull.Value ? Convert.ToDecimal(revenue) : 0;
                    lblTotalRevenue.Text = "₱" + totalRevenue.ToString("N2");
                }
            }
            catch (Exception ex)
            {
                // We'll fail silently or log it to avoid interrupting the user experience
                Console.WriteLine("Stats Error: " + ex.Message);
            }
        }

        // Generic navigation handler for sidebar buttons
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            string btnName = btn.Name;

            // Depending on which button was clicked, open the corresponding window
            // and close this one if needed.
            switch (btnName)
            {
                case "btnDashboard":
                    // Already here
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
                    Appointments appointmentWindow = new Appointments();
                    appointmentWindow.Show();
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
                    try
                    {
                        Reports reportsWindow = new Reports();
                        reportsWindow.Show();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening Reports: " + ex.Message + "\n\nTip: Ensure all libraries are correctly installed.", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    break;
                default:
                    MessageBox.Show($"{btn.Name} clicked. Functionality coming soon!", "Information");
                    break;
            }
        }

        // Handles the Log Out button click
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutUs aboutWindow = new AboutUs();
            aboutWindow.Show();
            this.Close();
        }
    }
}
