using audit.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Reflection.Emit;
using PdfSharp.Drawing.Layout;
using static System.Collections.Specialized.BitVector32;
using PdfSharp.Charting;
using System.Reflection;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using PdfSharp.Drawing.BarCodes;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.IO.Ports;
using System.Management;

namespace audit
{
    public partial class EndPage : Form
    {
        private string DispString;
        private string COMPORT;
        SerialPort SP;
        private string IDclovek;
        private string OK;
        private string NOK;
        private string OFI;
        private string NA;

        public EndPage()
        {
            InitializeComponent();
            if (Storage.DefaultLanguage == "1") { Text = "Potvrdenie auditu majiteľom procesu"; }
            else { Text = "Audit confirmation by process owner"; }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            Close();
        }

        private void EndPage_Load(object sender, EventArgs e)
        {
            if (Storage.DefaultLanguage == "1") //SK
            {
                label1.Text = "Audit:";
                label3.Text = "Area:";
                label4.Text = "Audítor:";
                label2.Text = "Potvrdenie majiteľom procesu";
                label5.Text = "Výsledok auditu:";
                label12.Text = "LPA";
                label13.Text = "Kategória:";
                label15.Text = "Zariadenie/výrobok:";
            }
            else  //ENG
            {
                label1.Text = "Audit:";
                label3.Text = "Area:";
                label4.Text = "Auditor:";
                label2.Text = "  Process owner confirmation";
                label5.Text = "Audit results:";
                label12.Text = "LPA";
                label13.Text = " Category:";
                label15.Text = "    Machine/product:";
            }
            label11.Text = Storage.Area;
            label6.Text = Storage.Auditor;
            label14.Text = Storage.Category;
            label16.Text = Storage.MachineOrProduct;    

            try //Load final results to overview
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal + ";MultipleActiveResultSets=True")) 
                {
                    sqlConnection.Open();
                    // OK
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT COUNT(*) FROM eAudit_LPAResults WHERE Result = 'OK' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection))
                    {
                        if (Storage.DefaultLanguage == "1") { label7.Text = "OK - " + sqlCmd.ExecuteScalar().ToString(); } else { label7.Text = "OK - " + sqlCmd.ExecuteScalar().ToString(); } OK = sqlCmd.ExecuteScalar().ToString();
                    }
                    // NOK
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT COUNT(*) FROM eAudit_LPAResults WHERE Result = 'NOK' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection))
                    {
                        if (Storage.DefaultLanguage == "1") { label8.Text = "NOK - " + sqlCmd.ExecuteScalar().ToString(); } else { label8.Text = "NOK - " + sqlCmd.ExecuteScalar().ToString(); } NOK = sqlCmd.ExecuteScalar().ToString();
                    }
                    // OFI
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT COUNT(*) FROM eAudit_LPAResults WHERE Result = 'OfI' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection))
                    {
                        if (Storage.DefaultLanguage == "1") { label9.Text = "PnZ - " + sqlCmd.ExecuteScalar().ToString(); }else { label9.Text = "OfI - " + sqlCmd.ExecuteScalar().ToString(); } OFI = sqlCmd.ExecuteScalar().ToString();
                    }
                    // N/A
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT COUNT(*) FROM eAudit_LPAResults WHERE Result = 'N/A' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection))
                    {
                        if (Storage.DefaultLanguage == "1") { label10.Text = "N/A - " + sqlCmd.ExecuteScalar().ToString(); } else { label10.Text = "N/A - " + sqlCmd.ExecuteScalar().ToString(); } NA = sqlCmd.ExecuteScalar().ToString();
                    }
                    sqlConnection.Close();  
                }
            }
            catch (Exception) { }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                string rightCOMport = (string)queryObj["DeviceID"] + (string)queryObj["PNPDeviceID"] + queryObj["Name"];
                if (rightCOMport.Contains("09D8&PID"))  // RFID reader's name (device manager)
                {
                    string devicename = (string)queryObj["Name"];
                    string toFind = "COM"; int start = devicename.IndexOf(toFind);
                    string COMwithParenthesis = devicename.Substring(start);
                    COMPORT = COMwithParenthesis.Replace(")", "");
                }
            }
            SP = new SerialPort();
            if (SP.IsOpen == false)
            {
                try
                {
                    serialPort1.PortName = COMPORT;
                    serialPort1.BaudRate = 9600;
                    serialPort1.DataBits = 8;
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Open();
                    serialPort1.ReadTimeout = 500;
                    if (serialPort1.IsOpen)
                    {
                        DispString = "";
                    }
                }
                catch (Exception)
                {
                    serialPort1.Close();
                    var w = new Form(); Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith((t) => w.Close(), TaskScheduler.FromCurrentSynchronizationContext());
                    if (Storage.DefaultLanguage == "1") { MessageBox.Show(w, "RFID čítačka nie je pripojená, nie je možné dokončiť audit!"); }
                    else { MessageBox.Show(w, "RFID reader is not connected, it is not possible to finish audit!"); }
                }
            }
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(RFID_DataReceived);
        }
        private void RFID_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (DispString.Length >= 30)
            {
                serialPort1.Close();
            }
            else
            {
                DispString = serialPort1.ReadLine();
                DispString = (DispString.Remove(0, 4));   // remove useless first chars 4200,4100.. stored in a card
                Invoke(new EventHandler(DisplayText));
            }
        }
        private void DisplayText(object sender, EventArgs e)
        {
            textBox2.Text = DispString;   //HEX num form serialport
        }


        private void pictureBox1_Click(object sender, EventArgs e)  //SAVE AUDIT
        {
            try  //ConfirmedBy a process owner to SQL for this AuditNum
            {
                using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    Conn.Open();
                    string Update = "UPDATE eAudit_LPAResults SET ConfirmedBy = '" + textBox1.Text + "' WHERE AuditNum = '" + Storage.AuditNum + "' ";
                    SqlCommand cmd1 = new SqlCommand(Update, Conn); cmd1.ExecuteNonQuery(); Conn.Close();
                }
            }
            catch (Exception) 
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Audit nie je možné dokončiť, kontaktujte správcu!"); }
                else { MessageBox.Show("It is not possible to finish audit, contact administrator!"); }
                return;
            }


            try  //mark questions & results as finished to be exported to main server
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal)) 
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("UPDATE eAudit_LPAResults SET Status = 'Finished' WHERE AuditNum = '" + Storage.AuditNum + "' AND Area <> 'Tréningový audit' ", sqlConnection))
                    { sqlCmd.ExecuteScalar(); }

                    sqlConnection.Close();
                }
            }
            catch (Exception) 
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Audit nie je možné dokončiť, kontaktujte správcu!"); }
                else { MessageBox.Show("It is not possible to finish audit, contact administrator!"); }
                return;
            }


            try   //Load pictures to local cql
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT ID,PhotoFileName1,PhotoFileName2,PhotoFileName3 FROM eAudit_LPAResults WHERE AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        if (!string.IsNullOrEmpty(sqlReader["PhotoFileName1"].ToString()))
                        {
                            byte[] imageData1 = File.ReadAllBytes(Storage.PhotoPath + "\\" + sqlReader["PhotoFileName1"].ToString());
                            SqlConnection CN = new SqlConnection(Connection.ConnectionStringLocal);
                            string qry = "UPDATE eAudit_LPAResults SET Photo1 = @ImageData1 WHERE ID = '" + sqlReader["ID"].ToString() + "'";
                            SqlCommand SqlCom = new SqlCommand(qry, CN); SqlCom.Parameters.Add(new SqlParameter("@ImageData1", (object)imageData1));
                            CN.Open(); SqlCom.ExecuteNonQuery();CN.Close();
                        }
                        if (!string.IsNullOrEmpty(sqlReader["PhotoFileName2"].ToString()))
                        {
                            byte[] imageData2 = File.ReadAllBytes(Storage.PhotoPath + "\\" + sqlReader["PhotoFileName2"].ToString());
                            SqlConnection CN = new SqlConnection(Connection.ConnectionStringLocal);
                            string qry = "UPDATE eAudit_LPAResults SET Photo2 = @ImageData2 WHERE ID = '" + sqlReader["ID"].ToString() + "'";
                            SqlCommand SqlCom = new SqlCommand(qry, CN); SqlCom.Parameters.Add(new SqlParameter("@ImageData2", (object)imageData2));
                            CN.Open(); SqlCom.ExecuteNonQuery(); CN.Close();
                        }
                        if (!string.IsNullOrEmpty(sqlReader["PhotoFileName3"].ToString()))
                        {
                            byte[] imageData3 = File.ReadAllBytes(Storage.PhotoPath + "\\" + sqlReader["PhotoFileName3"].ToString());
                            SqlConnection CN = new SqlConnection(Connection.ConnectionStringLocal);
                            string qry = "UPDATE eAudit_LPAResults SET Photo3 = @ImageData3 WHERE ID = '" + sqlReader["ID"].ToString() + "'";
                            SqlCommand SqlCom = new SqlCommand(qry, CN); SqlCom.Parameters.Add(new SqlParameter("@ImageData3", (object)imageData3));
                            CN.Open(); SqlCom.ExecuteNonQuery(); CN.Close();
                        }
                    }
                    sqlConnection.Close();
                }
            }
            catch (Exception ) 
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Nebolo možné spracovať fotky urobené počas auditu, kontaktujte správcu!"); }
                else { MessageBox.Show("It is not possible to process photos taken during this audit, contact administrator!"); }
            }

            serialPort1.Close();
            ByeByePage nextForm = new ByeByePage(); nextForm.ShowDialog();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)  // search user name and num
        {
            if (textBox2.Text.Length >= 5)
            {
                try
                {
                    textBox1.Text = "";
                    using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                    {
                        SqlCommand sqlCmd = new SqlCommand("SELECT id_doch,meno,priez FROM eAudit_People WHERE card_id LIKE '%" + textBox2.Text.ToString() + "%'", sqlConnection);
                        sqlConnection.Open();
                        SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                        while (sqlReader.Read())
                        {
                            IDclovek = (sqlReader["meno"].ToString() + " " + sqlReader["priez"].ToString() + " " + sqlReader["id_doch"].ToString().TrimStart(new Char[] { '0' }));
                        }
                        sqlReader.Close();
                        textBox1.Text = IDclovek; textBox1.BackColor = Color.Green;  //Complete name from local SQL
                    }
                }
                catch (Exception)
                {
                    var w = new Form(); Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith((t) => w.Close(), TaskScheduler.FromCurrentSynchronizationContext());
                    if (Storage.DefaultLanguage == "1") { MessageBox.Show(w, "Neznámy užívateľ, kontaktujte správcu!"); }
                    else { MessageBox.Show(w, "Unknown user, contact administrator!"); }
                }
            }
            else
            { textBox2.Text = ""; }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            pictureBox1.Visible = true; 
        }
    }
}
