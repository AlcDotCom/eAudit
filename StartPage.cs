using Audit;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;
using audit;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Drawing.Drawing2D;
using System.IO;
using audit.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Management;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public partial class StartPage : Form
    {
        private string DispString;
        private string COMPORT;
        SerialPort SP;
        private string IDclovek;

        public StartPage()
        {
            InitializeComponent();
            timer1 = new Timer(); timer1.Tick += new EventHandler(start); timer1.Interval = 2500; timer1.Start();
            Storage.DefaultLanguage = "1";
            slovenskyToolStripMenu1Item.Checked = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            Application.Exit();
        }

        private void slovenskyToolStripMenu1Item_Click(object sender, EventArgs e) //SK
        {
            Storage.DefaultLanguage = "1";
            slovenskyToolStripMenu1Item.Checked = true;
            englishToolStripMenuItem.Checked = false;
            button1.Text = "Začať audit";
            button2.Text = "Ukončiť";
            label1.Text = "Area auditu";
            label4.Text = "Kategória";
            label5.Text = "Zariadenie/výrobok";
            label2.Text = "Prihlásenie";
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e) //ENG
        {
            Storage.DefaultLanguage = "2";
            slovenskyToolStripMenu1Item.Checked = false;
            englishToolStripMenuItem.Checked = true;
            button1.Text = "Begin audit";
            button2.Text = "Close";
            label1.Text = " Audit area";
            label4.Text = "Category";
            label5.Text = " Machine/product";
            label2.Text = "   Sing in   ";
        }

        private void button1_Click(object sender, EventArgs e)  //Start audit
        {

            using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal)) //Clear InProgress table
            {
                Conn.Open(); string DelPreviousAudit = "delete from eAudit_LPAResults WHERE Status IS NULL"; SqlCommand cmd = new SqlCommand(DelPreviousAudit, Conn); cmd.ExecuteNonQuery();
            }

            if (comboBox2.SelectedIndex == -1)
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Vyberte areu auditu"); } else { MessageBox.Show("Choose audit area"); }
                comboBox2.Focus();
                return;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Vyberte kategóriu auditu"); } else { MessageBox.Show("Choose audit category"); }
                comboBox1.Focus();
                return;
            }

            if (comboBox3.SelectedIndex == -1)
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Vyberte Zariadenie/výrobok"); } else { MessageBox.Show("Choose Machine/product"); }
                comboBox3.Focus();
                return;
            }

            if (textBox1.Text == "")
            {
                if (Storage.DefaultLanguage == "1") { MessageBox.Show("Priložte kartu k čítačke"); } else { MessageBox.Show("Sign in with your card"); }
                textBox1.Focus();
                return;
            }

            Storage.Area = comboBox2.SelectedItem.ToString();
            Storage.AuditNum = Convert.ToInt64((DateTime.Now).ToString("yyyyMMddHHmmss"));
            Storage.AuditStart = (DateTime.Now);
            Storage.Auditor = textBox1.Text;

            Storage.Category = comboBox1.SelectedItem.ToString();
            Storage.MachineOrProduct = comboBox3.SelectedItem.ToString();

            Storage.PC = "PC: " + Environment.MachineName + ", User: " + Environment.UserName;
            if (DateTime.Now >= Convert.ToDateTime("06:00:00") && DateTime.Now < Convert.ToDateTime("14:00:00")) { Storage.Shift = "Ranná"; }
            else if (DateTime.Now >= Convert.ToDateTime("14:00:00") && DateTime.Now < Convert.ToDateTime("22:00:00")) { Storage.Shift = "Poobedná"; }
            else if (DateTime.Now >= Convert.ToDateTime("22:00:00") || DateTime.Now < Convert.ToDateTime("06:00:00")) { Storage.Shift = "Nočná"; }
            Dispose();
            try { Directory.GetFiles(Storage.PhotoPath).ToList().ForEach(File.Delete); } catch (Exception) { }
            serialPort1.Close();
            Questions nextForm = new Questions(); Hide(); nextForm.ShowDialog(); Close();
        }


        private void LoadAreas()
        {
            comboBox2.Items.Clear();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT Area FROM eAudit_LPAQuestions", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        comboBox2.Items.Add(sqlReader["Area"].ToString());
                        comboBox2.IntegralHeight = false; comboBox2.MaxDropDownItems = 12; comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                    }
                    sqlReader.Close();
                }
                comboBox2.SelectedIndex = 0;
                object[] distinctItems = (from object o in comboBox2.Items select o).Distinct().ToArray();
                comboBox2.Items.Clear(); comboBox2.Items.AddRange(distinctItems);

                comboBox2.SelectedIndex = -1;
            }
            catch (Exception) { }
        }

        private void LoadCategory()
        {
            comboBox1.Items.Clear();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT Category FROM eAudit_LPAQuestions WHERE Area = N'" + comboBox2.SelectedItem.ToString() + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        comboBox1.Items.Add(sqlReader["Category"].ToString());
                        comboBox1.IntegralHeight = false; comboBox1.MaxDropDownItems = 12; comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                    }
                    sqlReader.Close();
                }
                comboBox1.SelectedIndex = 0;
                object[] distinctItems = (from object o in comboBox1.Items select o).Distinct().ToArray();
                comboBox1.Items.Clear(); comboBox1.Items.AddRange(distinctItems);

                comboBox1.SelectedIndex = -1;
            }
            catch (Exception) { }
        }

        private void LoadMachineOrProduct()
        {
            comboBox3.Items.Clear();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT MachineOrProduct FROM eAudit_LPAmachinesOrProducts WHERE Area = N'" + comboBox2.SelectedItem.ToString() + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        comboBox3.Items.Add(sqlReader["MachineOrProduct"].ToString());
                        comboBox3.IntegralHeight = false; comboBox3.MaxDropDownItems = 17; comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
                    }
                    sqlReader.Close();
                }
                comboBox3.SelectedIndex = 0;
                object[] distinctItems = (from object o in comboBox3.Items select o).Distinct().ToArray();
                comboBox3.Items.Clear(); comboBox3.Items.AddRange(distinctItems);

                comboBox3.SelectedIndex = -1;
            }
            catch (Exception) { }
        }

        private void start(object sender, EventArgs e)
        {

            // zmazať

            List<Form> forms = new List<Form>();
            foreach (Form f in Application.OpenForms)
                if (f.Name == "HelloPage")
                    forms.Add(f);

            // Now let's close opened myForm instances
            foreach (Form f in forms)
                f.Close();

            Show();
                timer1.Stop();

            

        }



        private void StartPage_Load(object sender, EventArgs e)
        {
            LoadAreas();
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
                    if (Storage.DefaultLanguage == "1") { MessageBox.Show(w, "RFID čítačka nie je pripojená, začať nie je možný!"); }
                    else { MessageBox.Show(w, "RFID reader is not connected, it is not possible to start audit!"); }
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

        private void comboBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                e.DrawBackground();
                if (e.Index >= 0)
                {
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    Brush brush = new SolidBrush(cbx.ForeColor);
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        brush = SystemBrushes.HighlightText;
                    e.Graphics.DrawString(cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
                }
            }
        }

        private void button2_Paint(object sender, PaintEventArgs e)
        {
            int borderRadius = 60;  //param
            float borderThickness = 9f;  //param
            RectangleF Rect = new RectangleF(0, 0, button2.Width, button2.Height);
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button2.Region = new Region(GraphPath);
            using (Pen pen = new Pen(Color.FromArgb(250,128,114), borderThickness))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void button1_Paint(object sender, PaintEventArgs e)
        {
            int borderRadius = 60;  //param
            float borderThickness = 9f;  //param
            RectangleF Rect = new RectangleF(0, 0, button1.Width, button1.Height);  
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button1.Region = new Region(GraphPath);
            using (Pen pen = new Pen(Color.SteelBlue, borderThickness)) 
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
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
        public new void Dispose()
        {
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox2.Image.Dispose();
            pictureBox2.Image = null;
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e) //Area choose by user
        {
            Storage.Area = comboBox2.SelectedItem.ToString();
            LoadCategory();
            comboBox3.Items.Clear(); 
            pictureBox1.Visible = true;
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e) //Category choose by user
        {
            LoadMachineOrProduct();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Overview nextForm = new Overview(); nextForm.ShowDialog();
        }


    }
}
