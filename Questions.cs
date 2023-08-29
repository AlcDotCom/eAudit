using audit;
using audit.Properties;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using WindowsFormsApp1;
using Timer = System.Windows.Forms.Timer;

namespace Audit
{
    public partial class Questions : Form
    {
        public string actualfile;
        public string ClickedResult;
        public string ImportedPhotos = "";
        public int PhotoPresence = 0;
        public int FullOfPhotos = 0;
        public string QuestionCodeOfNOK;
        public string QuestionNOK;
        public static int PhotoImported = 0;
        public static string PhotoPosition = "";
        public static string PhotoCreationTime = "";
        public static int OK = 0;
        public static int NOK = 0;
        public static int NA = 0;
        public static int OFI = 0;
        public static int AuditIsDoneStopTimer = 0;
        public static int SEC = 1;

        public Questions()
        {
            InitializeComponent();
            Text = "LPA audit" + " - " + Storage.Area;
            QuestionRoll(); pictureBox4.Visible = false; 
            timer1 = new Timer(); timer1.Tick += new EventHandler(LookForPicFromAudit); timer1.Interval = 800; timer1.Start();
            timer2 = new Timer(); timer2.Tick += new EventHandler(ShowViewerIfPicExistForThisQuestion); timer2.Interval = 500; timer2.Start();
            timer3 = new Timer(); timer3.Tick += new EventHandler(Record1PlusSecondSpendForActualQuestion); timer3.Interval = 1000; timer3.Start();
            if (Storage.DefaultLanguage == "1") { button2.Text = "Ukončiť audit bez uloženia"; button10.Text = "Nezhoda"; button4.Text = "Zhoda"; button12.Text = "PnZ"; button11.Text = "N/A";} 
            else { button2.Text = "Close audit without saving"; button10.Text = "Non conformity"; button4.Text = "Conformity"; button12.Text = "OfI"; button11.Text = "N/A"; }
        }
        private void LookForPicFromAudit(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(Storage.PhotoPath);
                if (d.Exists)
                {
                    FileInfo[] Files = d.GetFiles();
                    foreach (FileInfo file in Files)
                    {
                        if (!ImportedPhotos.Contains(file.Name))
                        {
                            if (file.CreationTime > Storage.AuditStart)
                            {
                                if (file.Name != "desktop.ini"  &&  !file.Name.Contains("~"))
                                {


                                    PhotoImported = 0;
                                    using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                                    {
                                        SqlCommand sqlCmd = new SqlCommand("SELECT PhotoFileName1,PhotoFileName2,PhotoFileName3 FROM eAudit_LPAResults WHERE AuditNum = '" + Storage.AuditNum + "' ", sqlConnection); //Check if the pic already exists, if doesnt, continue
                                        sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                                        while (sqlReader.Read())
                                        {
                                            if (sqlReader["PhotoFileName1"].ToString() == file.Name || sqlReader["PhotoFileName2"].ToString() == file.Name || sqlReader["PhotoFileName3"].ToString() == file.Name) { PhotoImported = 1; }
                                        }
                                        sqlReader.Close();
                                    }
                                    if (PhotoImported == 0)
                                    {
                                        using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                                        {
                                            SqlCommand sqlCmd = new SqlCommand("SELECT PhotoFileName1,PhotoFileName2,PhotoFileName3 FROM eAudit_LPAResults WHERE QuestionCode = '" + Storage.ActualQuestionCode + "' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection); //Find first free position to store picture (max 3)
                                            sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                                            while (sqlReader.Read())
                                            {
                                                if (string.IsNullOrEmpty(sqlReader["PhotoFileName1"].ToString())) { PhotoPosition = "PhotoFileName1"; PhotoCreationTime = "CreationTime1"; break; }
                                                if (string.IsNullOrEmpty(sqlReader["PhotoFileName2"].ToString())) { PhotoPosition = "PhotoFileName2"; PhotoCreationTime = "CreationTime2"; break; }
                                                if (string.IsNullOrEmpty(sqlReader["PhotoFileName3"].ToString())) { PhotoPosition = "PhotoFileName3"; PhotoCreationTime = "CreationTime3"; } else { PhotoPosition = "Full"; }
                                            }
                                            sqlReader.Close();
                                        }
                                        using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal)) // Insert new pic name for actual question(code)
                                        {
                                            Conn.Open();
                                            string Insert = "UPDATE eAudit_LPAResults SET " + PhotoPosition + " = '" + file.Name + "'," + PhotoCreationTime + " = '" + file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE QuestionCode = '" + Storage.ActualQuestionCode + "' AND AuditNum = '" + Storage.AuditNum + "' "; 
                                            SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                                        }
                                    }


                                }
                            }
                        }
                        ImportedPhotos += file.Name;
                    }
                }
                else
                {
                    timer1.Stop();
                    if (Storage.DefaultLanguage == "1") { MessageBox.Show("Nie je možné nájsť cieľový súbor s fotkami, kontaktujte správcu."); }
                    else { MessageBox.Show("Directory for created pictures is not possible to find, contact administrator."); }
                }
            }
            catch (Exception) { }   
        }

        private void ShowViewerIfPicExistForThisQuestion(object sender, EventArgs e)
        {
            if (NOK == 1 || OFI == 1)
            {
                PhotoPresence = 0;
                FullOfPhotos= 0;    
                try
                {
                    using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                    {
                        SqlCommand sqlCmd = new SqlCommand("SELECT PhotoFileName1,PhotoFileName2,PhotoFileName3 FROM eAudit_LPAResults WHERE QuestionCode = '" + Storage.ActualQuestionCode + "' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                        sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                        while (sqlReader.Read())
                        {
                            if (!string.IsNullOrEmpty(sqlReader["PhotoFileName1"].ToString()) || !string.IsNullOrEmpty(sqlReader["PhotoFileName2"].ToString()) || !string.IsNullOrEmpty(sqlReader["PhotoFileName3"].ToString())) { pictureBox4.Visible = true; PhotoPresence = 1;
                            }
                            if (!string.IsNullOrEmpty(sqlReader["PhotoFileName1"].ToString()) && !string.IsNullOrEmpty(sqlReader["PhotoFileName2"].ToString()) && !string.IsNullOrEmpty(sqlReader["PhotoFileName3"].ToString())) { pictureBox1.Visible = false; FullOfPhotos = 1; }
                        }
                        sqlReader.Close();
                    }
                }
                catch (Exception) { pictureBox4.Visible = false; }
                if (PhotoPresence == 0) { pictureBox4.Visible = false; }
                if (FullOfPhotos == 0) { pictureBox1.Visible = true; }
            }
            else { pictureBox4.Visible = false; pictureBox1.Visible = false; } //For OK or N/A no need for camera nor PictureViewer
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Storage.DefaultLanguage == "1")
            {
                DialogResult dialogResult = MessageBox.Show("Ukončiť audit bez uloženia?", "UKONČIŤ AUDIT", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    try { Directory.GetFiles(Storage.PhotoPath).ToList().ForEach(File.Delete); } catch (Exception) { }
                    Application.Exit();
                }
                else if (dialogResult == DialogResult.No) { return; }
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Close without saving?", "CLOSE AUDIT", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    try { Directory.GetFiles(Storage.PhotoPath).ToList().ForEach(File.Delete); } catch (Exception) { }
                    Application.Exit();
                }
                else if (dialogResult == DialogResult.No) { return; }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("microsoft.windows.camera:");
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            QuestionRoll();
        }

        private void pictureBox2_Click(object sender, EventArgs e) //NEXT question
        {
            if (vScrollBar1.Value < vScrollBar1.Maximum) { vScrollBar1.Value = vScrollBar1.Value + 1; }  
            QuestionRoll();
        }
        private void pictureBox3_Click(object sender, EventArgs e) //PREVIOUS question
        {
            if (vScrollBar1.Value > vScrollBar1.Minimum) { vScrollBar1.Value = vScrollBar1.Value - 1; }
            QuestionRoll();
        }
        private void QuestionRoll()
        {
                pictureBox4.Visible = false; 
            if (vScrollBar1.Value == vScrollBar1.Minimum) { pictureBox3.Visible = false; } else { pictureBox3.Visible = true; } 
            if (vScrollBar1.Value == vScrollBar1.Maximum) { pictureBox2.Visible = false; } else { pictureBox2.Visible = true; }

            try        // Load scrolled question 
            {
                Storage.ActualQuestionNum = vScrollBar1.Value + 1;
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal)) 
                {
                    SqlCommand sqlCmd1 = new SqlCommand("select TOP(" + Storage.ActualQuestionNum + ") QuestionCode,Question,Finding,Result FROM eAudit_LPAResults WHERE AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader1 = sqlCmd1.ExecuteReader();
                    while (sqlReader1.Read())
                    {
                        textBox2.Text = sqlReader1["QuestionCode"].ToString();
                        Storage.ActualQuestionCode = sqlReader1["QuestionCode"].ToString();
                        button1.Text = sqlReader1["Question"].ToString();
                        textBox1.Text = sqlReader1["Finding"].ToString();
                        string Result = sqlReader1["Result"].ToString();

                        if (Result == string.Empty) { button1.BackgroundImage = Resources.Empty; pictureBox1.Visible = false; NA = 0; OK = 0; NOK = 0; OFI = 0; }

                        else if (Result == "OK")
                        {
                            pictureBox1.Visible = false; OK = 1; NOK = 0; OFI = 0; NA = 0; 
                            if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.Zhoda; }
                            else { button1.BackgroundImage = Resources.Conformity; }
                        }

                        else if (Result == "NOK")
                        {
                            pictureBox1.Visible = true; NOK = 1; OK = 0; OFI = 0; NA = 0;  
                            if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.Nezhoda; }
                            else { button1.BackgroundImage = Resources.NonConformity; }
                        }

                        else if (Result == "N/A")
                        {
                            pictureBox1.Visible = false; NA = 1; OK = 0; OFI = 0; NOK = 0;
                            if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.Neaplikovatelne; }
                            else { button1.BackgroundImage = Resources.NotApplicable; }
                        }

                        else if (Result == "OfI")
                        {
                            pictureBox1.Visible = true; OFI = 1; OK= 0; NOK = 0; NA = 0;
                            if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.PnZ; }
                            else { button1.BackgroundImage = Resources.OfI; }
                        }
                    }
                    sqlReader1.Close();
                }
            }
            catch (Exception) { }
            SetFontSizeForQuestion();
        }

        private void Questions_Load(object sender, EventArgs e) //LOAD
        {
            if(vScrollBar1.Value == vScrollBar1.Minimum) { pictureBox3.Visible=false; }
            if (vScrollBar1.Value == vScrollBar1.Maximum) { pictureBox2.Visible = false; }
            int borderRadius = 36;  //param
            RectangleF Rect = new RectangleF(0, 0, vScrollBar1.Width, vScrollBar1.Height);
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            vScrollBar1.Region = new Region(GraphPath);

            try   //Load audit questions & insert it to LPAresults table      
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT QuestionCode,QuestionSK,QuestionENG,DailyFrequency FROM eAudit_LPAQuestions WHERE Area = N'" + Storage.Area + "' AND QuestionCode <> 'TBD' AND Category = N'" + Storage.Category + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                        {
                            Conn.Open();

                            int DayNum = (DateTime.Now.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek;
                            if (sqlReader["DailyFrequency"].ToString().Contains(DayNum.ToString()))
                            {
                                if (Storage.DefaultLanguage == "1")
                                {
                                    string Insert = "insert into eAudit_LPAResults (AuditNum,Area,Category,MachineOrProduct,QuestionCode,Question,TimeSpentOnQuestion,Auditor,DateOfAudit,Shift,PC)values('" + Storage.AuditNum + "','" + Storage.Area + "','" + Storage.Category + "','" + Storage.MachineOrProduct + "','" + sqlReader["QuestionCode"].ToString() + "','" + sqlReader["QuestionSK"].ToString() + "','1','" + Storage.Auditor + "','" + Storage.AuditStart.ToString("yyyy-MM-dd HH:mm:ss") + "','" + Storage.Shift + "','" + Storage.PC + "')";
                                    SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    string Insert = "insert into eAudit_LPAResults (AuditNum,Area,Category,MachineOrProduct,QuestionCode,Question,TimeSpentOnQuestion,Auditor,DateOfAudit,Shift,PC)values('" + Storage.AuditNum + "','" + Storage.Area + "','" + Storage.Category + "','" + Storage.MachineOrProduct + "','" + sqlReader["QuestionCode"].ToString() + "','" + sqlReader["QuestionENG"].ToString() + "','1','" + Storage.Auditor + "','" + Storage.AuditStart.ToString("yyyy-MM-dd HH:mm:ss") + "','" + Storage.Shift + "','" + Storage.PC + "')";
                                    SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                                }
                            }

                            Conn.Close();
                        }
                    }
                    sqlConnection.Close();

                    SqlCommand sqlCmd1 = new SqlCommand("select TOP(1) QuestionCode,Question FROM eAudit_LPAResults WHERE AuditNum = '" + Storage.AuditNum + "' ", sqlConnection); // Load 1st question
                    sqlConnection.Open(); SqlDataReader sqlReader1 = sqlCmd1.ExecuteReader();
                    while (sqlReader1.Read())
                    {
                        textBox2.Text = sqlReader1["QuestionCode"].ToString();
                        Storage.ActualQuestionCode = sqlReader1["QuestionCode"].ToString();
                        button1.Text = sqlReader1["Question"].ToString();
                        Storage.ActualQuestionNum = 1;
                        break;
                    }
                    sqlReader1.Close();
                    SetFontSizeForQuestion();
                }
            }
            catch (Exception) { }

            try   //hide navigation if theres not enough questions
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT count(QuestionCode) as int FROM eAudit_LPAResults WHERE QuestionCode <> 'TBD' AND Question <> '' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        if (Convert.ToInt32(sqlReader["int"]) < 2) //only 1 question
                        {
                            pictureBox2.Visible = false;
                            pictureBox3.Visible = false;
                            vScrollBar1.Visible = false;
                        }
                        if (Convert.ToInt32(sqlReader["int"]) < 1)
                        {
                            pictureBox2.Visible = false;
                            pictureBox3.Visible = false;
                            vScrollBar1.Visible = false;
                            button4.Visible = false;
                            button10.Visible = false;
                            button11.Visible = false;
                            button12.Visible = false;
                            pictureBox1.Visible = false;
                            pictureBox4.Visible = false;
                            if (Storage.DefaultLanguage == "1") { button1.Text = "Pre tento audit nie je definovaná žiadna otázka, kontaktuje správcu."; }
                            else { button1.Text = "There is no question defined for this audit, contact administrator."; }
                        }
                        vScrollBar1.Maximum = Convert.ToInt32(sqlReader["int"]) - 1;
                    }
                    sqlReader.Close();
                }
            }
            catch (Exception) { }
        }

        private void button10_Paint(object sender, PaintEventArgs e)  //NOK button
        {
            int borderRadius = 80;  //param
            float borderThickness = 9f;  //param
            RectangleF Rect = new RectangleF(0, 0, button10.Width, button10.Height);  //button num
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button10.Region = new Region(GraphPath);  //button num
            using (Pen pen = new Pen(Color.Maroon, borderThickness))  //color 
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void button4_Paint(object sender, PaintEventArgs e)  //OK button
        {
            int borderRadius = 80; 
            float borderThickness = 9f;  
            RectangleF Rect = new RectangleF(0, 0, button4.Width, button4.Height); 
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button4.Region = new Region(GraphPath);  
            using (Pen pen = new Pen(Color.ForestGreen, borderThickness))  
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void button12_Paint(object sender, PaintEventArgs e)  //OFI button
        {
            int borderRadius = 20; 
            float borderThickness = 9f; 
            RectangleF Rect = new RectangleF(0, 0, button12.Width, button12.Height); 
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button12.Region = new Region(GraphPath); 
            using (Pen pen = new Pen(Color.FromArgb(192, 255, 192), borderThickness)) 
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void button11_Paint(object sender, PaintEventArgs e)  //N/A button
        {
            int borderRadius = 30;  
            float borderThickness = 9f;  
            RectangleF Rect = new RectangleF(0, 0, button11.Width, button11.Height); 
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button11.Region = new Region(GraphPath);  
            using (Pen pen = new Pen(Color.Gray, borderThickness)) 
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void button2_Paint(object sender, PaintEventArgs e) //Close audit button
        {
            int borderRadius = 35;  
            float borderThickness = 9f;  
            RectangleF Rect = new RectangleF(0, 0, button2.Width, button2.Height); 
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            button2.Region = new Region(GraphPath);  
            using (Pen pen = new Pen(Color.Goldenrod, borderThickness)) 
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }

        private void button4_Click(object sender, EventArgs e) //Conformity button
        {
            if (OK == 0){ OK = 1; NOK = 0; NA = 0; OFI = 0; if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.Zhoda;}else { button1.BackgroundImage = Resources.Conformity; } ClickedResult = "OK"; InsertResult(); pictureBox1.Visible = false; }
            else { OK = 0; NOK = 0; NA = 0; OFI = 0; button1.BackgroundImage = Resources.Empty; RemoveResult(); }
            CheckIfImDone();
        }
        
        private void button10_Click(object sender, EventArgs e) //NonConformity button
        {
            if (NOK == 0) { NOK = 1; OK = 0; NA = 0; OFI = 0; if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.Nezhoda; } else { button1.BackgroundImage = Resources.NonConformity; } ClickedResult = "NOK"; InsertResult(); pictureBox1.Visible = true; }
            else { OK = 0; NOK = 0; NA = 0; OFI = 0; button1.BackgroundImage = Resources.Empty; RemoveResult(); pictureBox1.Visible = false; }
            CheckIfImDone();
        }

        private void button12_Click(object sender, EventArgs e) //OFI button
        {
            if (OFI == 0) { OFI = 1;OK = 0; NA = 0; NOK = 0; if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.PnZ; } else { button1.BackgroundImage = Resources.OfI; } ClickedResult = "OfI"; InsertResult(); pictureBox1.Visible = true; }
            else { OFI = 0; NOK = 0; NA = 0; OK = 0; button1.BackgroundImage = Resources.Empty; RemoveResult(); pictureBox1.Visible = false; }
            CheckIfImDone();
        }

        private void button11_Click(object sender, EventArgs e) //NA button
        {
            if (NA == 0) { NA = 1; OK = 0; NOK = 0; OFI = 0; if (Storage.DefaultLanguage == "1") { button1.BackgroundImage = Resources.Neaplikovatelne; } else { button1.BackgroundImage = Resources.NotApplicable; } ClickedResult = "N/A"; InsertResult(); pictureBox1.Visible = false; }
            else { NA = 0; NOK = 0; OK = 0; OFI = 0; button1.BackgroundImage = Resources.Empty; RemoveResult(); }
            CheckIfImDone();
        }

        private void InsertResult()
        {
            using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
            {
                Conn.Open();
                string InsertResult = "UPDATE eAudit_LPAResults SET Result = '" + ClickedResult + "' WHERE QuestionCode = '" + textBox2.Text + "' AND Question = '" + button1.Text + "' AND AuditNum = '" + Storage.AuditNum + "' ";
                SqlCommand cmd = new SqlCommand(InsertResult, Conn); cmd.ExecuteNonQuery(); Conn.Close();
            }
        }

        private void RemoveResult()
        {
            using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
            {
                Conn.Open();
                string InsertResult = "UPDATE eAudit_LPAResults SET Result = '' WHERE Area = '" + Storage.Area + "' AND QuestionCode = '" + textBox2.Text + "' AND Question = '" + button1.Text + "' AND AuditNum = '" + Storage.AuditNum + "' ";
                SqlCommand cmd = new SqlCommand(InsertResult, Conn); cmd.ExecuteNonQuery(); Conn.Close();
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Storage.ActualQuestionCode = textBox2.Text;
            PictureViewer nextForm = new PictureViewer();
            nextForm.ShowDialog();
        }
        public new void Dispose()
        {
            pictureBox1.Image.Dispose();pictureBox1.Image = null;
            pictureBox2.Image.Dispose();pictureBox2.Image = null;
            pictureBox3.Image.Dispose();pictureBox3.Image = null;
            pictureBox4.Image.Dispose();pictureBox4.Image = null;
            pictureBox5.Image.Dispose();pictureBox5.Image = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e) //insert Finding
        {
            try
            {
                using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal)) 
                {
                    Conn.Open();
                    string Update = "UPDATE eAudit_LPAResults SET Finding = '" + textBox1.Text + "' WHERE QuestionCode = '" + textBox2.Text + "' AND Question = '" + button1.Text + "' AND AuditNum = '" + Storage.AuditNum + "' ";
                    SqlCommand cmd1 = new SqlCommand(Update, Conn); cmd1.ExecuteNonQuery();Conn.Close();
                }
            }
            catch (Exception ) { }
        }
        private void CheckIfImDone() // 🍆 😱
        {
            try 
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT Result FROM eAudit_LPAResults WHERE AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        if (string.IsNullOrEmpty(sqlReader["Result"].ToString())) {pictureBox5.Visible= false; break;}
                        else { pictureBox5.Visible= true; }
                    }
                    sqlReader.Close();
                }
            }
            catch (Exception) { }
        }

        private void pictureBox5_Click(object sender, EventArgs e) //Last check of data completeness
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))  // Verify if all OfI's have a description present
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT Finding,Question FROM eAudit_LPAResults WHERE Result = 'OfI' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        if (string.IsNullOrEmpty(sqlReader["Finding"].ToString())) 
                        {
                            if (Storage.DefaultLanguage == "1") { MessageBox.Show("Pre zadanú príležitosť na zlepšenie (PnZ) v otázke:" + Environment.NewLine + Environment.NewLine + "//💡// " + sqlReader["Question"].ToString() + " //💡//" + Environment.NewLine + Environment.NewLine + "chýba popis návrhu na zlepšenie!"); }
                            else { MessageBox.Show("Opportunity for Improvement (OfI) in a question:" + Environment.NewLine + Environment.NewLine + "//💡// " + sqlReader["Question"].ToString() + " //💡//" + Environment.NewLine + Environment.NewLine + "is missing description!"); }
                            return;
                        }
                    }
                    sqlReader.Close();
                }


                QuestionCodeOfNOK = ""; QuestionNOK = "";
                using (SqlConnection sqlConn = new SqlConnection(Connection.ConnectionStringLocal)) // Verify if there's any NOK
                {
                    SqlCommand sqlCmd1 = new SqlCommand("SELECT QuestionCode,Question FROM eAudit_LPAResults WHERE Result = 'NOK' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConn);
                    sqlConn.Open(); SqlDataReader sqlReader1 = sqlCmd1.ExecuteReader();
                    while (sqlReader1.Read())
                    {
                        using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                        {
                            Conn.Open();
                            if (!string.IsNullOrEmpty(sqlReader1["QuestionCode"].ToString())) //There is an NOK
                            {
                                QuestionCodeOfNOK = sqlReader1["QuestionCode"].ToString();
                                QuestionNOK = sqlReader1["Question"].ToString();


                                if (!string.IsNullOrEmpty(QuestionCodeOfNOK))  // Verify if each NOK has at least 1 picture, If not do alert auditor
                                {
                                    using (SqlConnection Conn1 = new SqlConnection(Connection.ConnectionStringLocal))
                                    {
                                        Conn1.Open();
                                        string CheckDuplicity = "SELECT COUNT(*) FROM eAudit_LPAResults WHERE QuestionCode = '" + QuestionCodeOfNOK + "' AND PhotoFileName1 IS NULL AND PhotoFileName2 IS NULL AND PhotoFileName3 IS NULL AND AuditNum = '" + Storage.AuditNum + "' ";
                                        SqlCommand cmnd = new SqlCommand(CheckDuplicity, Conn1);
                                        cmnd.ExecuteNonQuery();
                                        int COUNT = Convert.ToInt32(cmnd.ExecuteScalar());

                                        if (COUNT > 0) // If one of NOK has no picture than alert
                                        {
                                            if (Storage.DefaultLanguage == "1") { MessageBox.Show("Pre zadanú nezhodu v otázke:" + Environment.NewLine + Environment.NewLine + "//✘// " + QuestionNOK + " //✘//" + Environment.NewLine + Environment.NewLine + "chýba fotka ako dôkaz!"); }
                                            else { MessageBox.Show("Nonconformity in a question:" + Environment.NewLine + Environment.NewLine + "//✘// " + QuestionNOK + " //✘//" + Environment.NewLine + Environment.NewLine + "is missing a photo as evidence!"); }
                                            return;
                                        }
                                    }
                                }


                            }
                        }
                    }
                    sqlReader1.Close(); 
                }
                AuditIsDoneStopTimer = 1;
                EndPage nextForm = new EndPage(); nextForm.ShowDialog();
            }
            catch (Exception ) { }
        }

        private void SetFontSizeForQuestion()
        {
            if (button1.Text.Length <= 130) { button1.Font = new Font("Microsoft Sans Serif", 20f, style: FontStyle.Bold); }
            if (button1.Text.Length > 130) { button1.Font = new Font("Microsoft Sans Serif", 16f, style: FontStyle.Bold); }
            if (button1.Text.Length > 155) { button1.Font = new Font("Microsoft Sans Serif", 14f, style: FontStyle.Bold); }
            if (button1.Text.Length > 210) { button1.Font = new Font("Microsoft Sans Serif", 12f, style: FontStyle.Bold); }
            if (button1.Text.Length > 300) { button1.Font = new Font("Microsoft Sans Serif", 10f, style: FontStyle.Bold); }
        }
        private void Record1PlusSecondSpendForActualQuestion(object sender, EventArgs e)
        {
            if (AuditIsDoneStopTimer == 0)
            {
                try
                {
                    using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))  // get actual duration
                    {
                        SqlCommand sqlCmd = new SqlCommand("SELECT TimeSpentOnQuestion FROM eAudit_LPAResults WHERE QuestionCode = '" + textBox2.Text + "' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                        sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                        while (sqlReader.Read())
                        {
                            using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                            {
                                Conn.Open();                                                                  // insert +1 second every second to actual opened question
                                string Insert = "UPDATE eAudit_LPAResults SET TimeSpentOnQuestion = '" + (Convert.ToInt32(sqlReader["TimeSpentOnQuestion"]) + 1) + "' WHERE QuestionCode = '" + textBox2.Text + "' AND AuditNum = '" + Storage.AuditNum + "' ";
                                SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                                Conn.Close();
                            }
                        }
                        sqlReader.Close();
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
