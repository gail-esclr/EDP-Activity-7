using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class DoctorWindow : Window
    {
        private int _doctorId = -1; // -1 means adding new doctor
        public bool Success { get; private set; } = false;

        public DoctorWindow()
        {
            InitializeComponent();
        }

        // Overload for Editing
        public DoctorWindow(int id, string fullName, string specialization, string contact, string email)
        {
            InitializeComponent();
            _doctorId = id;
            lblTitle.Text = "Update Doctor";
            txtFullName.Text = fullName;
            txtSpecialization.Text = specialization;
            txtContact.Text = contact;
            txtEmail.Text = email;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string specialization = txtSpecialization.Text.Trim();
            string contact = txtContact.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(specialization))
            {
                MessageBox.Show("Full Name and Specialization are required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query;
                    
                    if (_doctorId == -1) // Insert
                    {
                        query = "INSERT INTO doctors (full_name, specialization, contact_number, email) VALUES (@name, @spec, @contact, @email)";
                    }
                    else // Update
                    {
                        query = "UPDATE doctors SET full_name=@name, specialization=@spec, contact_number=@contact, email=@email WHERE doctor_id=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", fullName);
                    cmd.Parameters.AddWithValue("@spec", specialization);
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.Parameters.AddWithValue("@email", email);
                    if (_doctorId != -1) cmd.Parameters.AddWithValue("@id", _doctorId);

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
