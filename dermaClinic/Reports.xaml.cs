using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace dermaClinic
{
    public partial class Reports : Window
    {
        public Reports()
        {
            try
            {
                InitializeComponent();
                LoadReportData("Sales Transactions (Payments)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reports Initialization Error: " + ex.Message);
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string reportType = (cmbReportType.SelectedItem as ComboBoxItem)?.Content.ToString();
            LoadReportData(reportType);
        }

        private void cmbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgReport == null) return; 
            string reportType = (cmbReportType.SelectedItem as ComboBoxItem)?.Content.ToString();
            LoadReportData(reportType);
        }

        private void LoadReportData(string type)
        {
            try
            {
                using (MySqlConnection conn = DbConnection.GetConnection())
                {
                    conn.Open();
                    string query = "";

                    if (type.Contains("Sales"))
                    {
                        query = @"SELECT p.payment_id as 'ID', pt.full_name as 'Patient', s.service_name as 'Service', 
                                         p.amount_paid as 'Amount', p.payment_method as 'Method', p.payment_date as 'Date'
                                  FROM payments p
                                  JOIN appointments a ON p.appointment_id = a.appointment_id
                                  JOIN patients pt ON a.patient_id = pt.patient_id
                                  JOIN services s ON a.service_id = s.service_id";
                    }
                    else if (type.Contains("Appointment"))
                    {
                        query = @"SELECT a.appointment_id as 'ID', pt.full_name as 'Patient', d.full_name as 'Doctor', 
                                         s.service_name as 'Service', a.appointment_date as 'Date', a.status as 'Status'
                                  FROM appointments a
                                  JOIN patients pt ON a.patient_id = pt.patient_id
                                  JOIN doctors d ON a.doctor_id = d.doctor_id
                                  JOIN services s ON a.service_id = s.service_id";
                    }
                    else 
                    {
                        query = "SELECT patient_id as 'ID', full_name as 'Name', gender as 'Gender', email as 'Email', contact_number as 'Contact' FROM patients";
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgReport.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message);
            }
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgReport.ItemsSource == null)
            {
                MessageBox.Show("Please generate a report first.", "No Data");
                return;
            }

            try
            {
                DataView dv = (DataView)dgReport.ItemsSource;
                DataTable dt = dv.ToTable();

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Excel Workbook|*.xlsx";
                sfd.FileName = "DermaClinic_Report_" + DateTime.Now.ToString("yyyyMMdd");

                if (sfd.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Data Report");
                        // 1. Branding - Manual Styling for Exact Match
                        var brandColor = XLColor.FromHtml("#A32A53");
                        var lightPink = XLColor.FromHtml("#FDECEF");
                        string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                        
                        ws.Column(1).Width = 25;
                        ws.Row(1).Height = 75;

                        // Add Logo in A1 (Fitted inside the cell)
                        if (System.IO.File.Exists(logoPath))
                        {
                            var pic = ws.AddPicture(logoPath).MoveTo(ws.Cell(1, 1), 5, 5);
                            pic.Height = 70;
                            pic.Width = 70;
                        }

                        // Title in B1
                        ws.Cell("B1").Value = "DERMACARE DERMATOLOGY CLINIC";
                        ws.Cell("B1").Style.Font.Bold = true;
                        ws.Cell("B1").Style.Font.FontSize = 26;
                        ws.Cell("B1").Style.Font.FontColor = brandColor;
                        ws.Range("B1:H1").Merge();

                        ws.Cell("B2").Value = "Official Transaction Report";
                        ws.Cell("B2").Style.Font.Italic = true;
                        ws.Cell("B2").Style.Font.FontSize = 13;
                        ws.Cell("B2").Style.Font.FontColor = XLColor.Gray;
                        ws.Range("B2:H2").Merge();

                        ws.Cell("A4").Value = "Generated on: " + DateTime.Now.ToString("MMMM dd, yyyy HH:mm");
                        ws.Cell("A4").Style.Font.Bold = true;

                        // 2. Data Table - Manual Pink Banding
                        if (dt.Rows.Count > 0)
                        {
                            var table = ws.Cell(6, 1).InsertTable(dt);
                            table.Theme = XLTableTheme.None; // Use manual styling to avoid orange/blue issues
                            
                            var headerRange = table.HeadersRow();
                            headerRange.Style.Fill.BackgroundColor = brandColor;
                            headerRange.Style.Font.FontColor = XLColor.White;
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            // Apply Manual Banding (Every even row light pink)
                            for (int i = 2; i <= table.RowCount(); i++)
                            {
                                if (i % 2 == 0)
                                {
                                    table.Row(i).Style.Fill.BackgroundColor = lightPink;
                                }
                            }

                            ws.Columns().AdjustToContents();
                            ws.Column(1).Width = 15; // Reset Column A for logo
                        }

                        // 3. Signature
                        int lastRow = dt.Rows.Count + 9;
                        ws.Cell(lastRow, 1).Value = "Prepared by:";
                        ws.Cell(lastRow, 1).Style.Font.Bold = true;
                        ws.Cell(lastRow + 2, 1).Value = (UserSession.Username ?? "Administrator").ToUpper();
                        ws.Cell(lastRow + 2, 1).Style.Font.Bold = true;
                        ws.Cell(lastRow + 3, 1).Value = "System Administrator";
                        ws.Cell(lastRow + 3, 1).Style.Font.Italic = true;

                        // 4. SHEET 2: ANALYTICS VIEW
                        if (dt.Rows.Count > 0)
                        {
                            var ws2 = workbook.Worksheets.Add("Analytics View");
                            ws2.Column(1).Width = 25;
                            ws2.Row(1).Height = 75;

                            if (System.IO.File.Exists(logoPath))
                            {
                                var pic2 = ws2.AddPicture(logoPath).MoveTo(ws2.Cell(1, 1), 5, 5);
                                pic2.Height = 70;
                                pic2.Width = 70;
                            }

                            string reportTitle = (cmbReportType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Report";
                            ws2.Cell("B1").Value = reportTitle + " Analysis";
                            ws2.Cell("B1").Style.Font.Bold = true;
                            ws2.Cell("B1").Style.Font.FontSize = 22;
                            ws2.Cell("B1").Style.Font.FontColor = brandColor;

                            ws2.Cell("A3").Value = "Category";
                            ws2.Cell("B3").Value = "Count";
                            ws2.Cell("C3").Value = "Visual Proportion (Simple Bar Graph)";
                            ws2.Range("A3:C3").Style.Font.Bold = true;
                            ws2.Range("A3:C3").Style.Fill.BackgroundColor = brandColor;
                            ws2.Range("A3:C3").Style.Font.FontColor = XLColor.White;

                            var counts = new Dictionary<string, int>();
                            string groupColName = "";
                            if (reportTitle.Contains("Sales")) groupColName = "Service";
                            else if (reportTitle.Contains("Appointment")) groupColName = "Status";
                            else if (reportTitle.Contains("Patient")) groupColName = "Gender";

                            if (!string.IsNullOrEmpty(groupColName) && dt.Columns.Contains(groupColName))
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    string val = row[groupColName]?.ToString() ?? "Unknown";
                                    if (counts.ContainsKey(val)) counts[val]++;
                                    else counts[val] = 1;
                                }
                            }

                            int rowIdx = 4;
                            foreach (var kvp in counts)
                            {
                                ws2.Cell(rowIdx, 1).Value = kvp.Key;
                                ws2.Cell(rowIdx, 2).Value = kvp.Value;
                                ws2.Cell(rowIdx, 3).Value = kvp.Value; 
                                rowIdx++;
                            }

                            if (rowIdx > 4)
                            {
                                var barRange = ws2.Range(4, 3, rowIdx - 1, 3);
                                barRange.AddConditionalFormat().DataBar(XLColor.DeepPink);
                            }
                            ws2.Columns().AdjustToContents();
                            ws2.Column(3).Width = 45; 
                         }

                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Report exported successfully using ClosedXML!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export Error: " + ex.Message, "Error");
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
                case "btnAppointments":
                    Appointments appWindow = new Appointments();
                    appWindow.Show();
                    this.Close();
                    break;
                case "btnPayments":
                    Payments paymentWindow = new Payments();
                    paymentWindow.Show();
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
