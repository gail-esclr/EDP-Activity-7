using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace dermaClinic
{
    public partial class AppointmentWindow : Window
    {
        private int _appointmentId = -1; // -1 means adding new appointment
        public bool Success { get; private set; } = false;

        public AppointmentWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            dpDate.SelectedDate = DateTime.Now;
        }

        // Overload for Editing
        public AppointmentWindow(int id, int patientId, int doctorId, int serviceId, DateTime date, string time, string status)
        {
            InitializeComponent();
            _appointmentId = id;
            lblTitle.Text = "Update Appointment";
            btnSave.Content = "SAVE CHANGES";
            
            LoadComboBoxes();

            cmbPatient.SelectedValue = patientId;
            cmbDoctor.SelectedValue = doctorId;
            cmbService.SelectedValue = serviceId;
            dpDate.SelectedDate = date;
            txtTime.Text = time;

            // Set Status
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == status) { cmbStatus.SelectedItem = item; break; }
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();

                    // Load Patients
                    MySqlDataAdapter pAdapter = new MySqlDataAdapter("SELECT patient_id, full_name FROM patients", conn);
                    DataTable pTable = new DataTable();
                    pAdapter.Fill(pTable);
                    cmbPatient.ItemsSource = pTable.DefaultView;

                    // Load Doctors
                    MySqlDataAdapter dAdapter = new MySqlDataAdapter("SELECT doctor_id, full_name FROM doctors", conn);
                    DataTable dTable = new DataTable();
                    dAdapter.Fill(dTable);
                    cmbDoctor.ItemsSource = dTable.DefaultView;

                    // Load Services
                    MySqlDataAdapter sAdapter = new MySqlDataAdapter("SELECT service_id, service_name FROM services", conn);
                    DataTable sTable = new DataTable();
                    sAdapter.Fill(sTable);
                    cmbService.ItemsSource = sTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dropdown data: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedValue == null || cmbDoctor.SelectedValue == null || cmbService.SelectedValue == null)
            {
                MessageBox.Show("Please select a patient, doctor, and service.", "Warning");
                return;
            }

            string status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime? date = dpDate.SelectedDate;
            string time = txtTime.Text.Trim();

            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query;
                    
                    if (_appointmentId == -1) // Insert
                    {
                        query = @"INSERT INTO appointments (patient_id, doctor_id, service_id, appointment_date, appointment_time, status) 
                                 VALUES (@pid, @did, @sid, @date, @time, @status)";
                    }
                    else // Update
                    {
                        query = @"UPDATE appointments SET patient_id=@pid, doctor_id=@did, service_id=@sid, 
                                 appointment_date=@date, appointment_time=@time, status=@status 
                                 WHERE appointment_id=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@pid", cmbPatient.SelectedValue);
                    cmd.Parameters.AddWithValue("@did", cmbDoctor.SelectedValue);
                    cmd.Parameters.AddWithValue("@sid", cmbService.SelectedValue);
                    cmd.Parameters.AddWithValue("@date", date?.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@time", time);
                    cmd.Parameters.AddWithValue("@status", status);
                    if (_appointmentId != -1) cmd.Parameters.AddWithValue("@id", _appointmentId);

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
