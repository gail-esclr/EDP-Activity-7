using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class PatientWindow : Window
    {
        private int _patientId = -1; // -1 means adding new patient
        public bool Success { get; private set; } = false;

        public PatientWindow()
        {
            InitializeComponent();
            dpBirthdate.SelectedDate = DateTime.Now;
        }

        // Overload for Editing
        public PatientWindow(int id, string fullName, string gender, DateTime birthdate, string contact, string email, string address)
        {
            InitializeComponent();
            _patientId = id;
            lblTitle.Text = "Update Patient";
            txtFullName.Text = fullName;
            dpBirthdate.SelectedDate = birthdate;
            txtContact.Text = contact;
            txtEmail.Text = email;
            txtAddress.Text = address;

            // Set Gender
            foreach (ComboBoxItem item in cmbGender.Items)
            {
                if (item.Content.ToString() == gender) { cmbGender.SelectedItem = item; break; }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string gender = (cmbGender.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime? birthdate = dpBirthdate.SelectedDate;
            string contact = txtContact.Text.Trim();
            string email = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(contact))
            {
                MessageBox.Show("Full Name and Contact Number are required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query;
                    
                    if (_patientId == -1) // Insert
                    {
                        query = "INSERT INTO patients (full_name, gender, birthdate, contact_number, email, address) VALUES (@name, @gender, @birth, @contact, @email, @address)";
                    }
                    else // Update
                    {
                        query = "UPDATE patients SET full_name=@name, gender=@gender, birthdate=@birth, contact_number=@contact, email=@email, address=@address WHERE patient_id=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", fullName);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@birth", birthdate?.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@address", address);
                    if (_patientId != -1) cmd.Parameters.AddWithValue("@id", _patientId);

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
