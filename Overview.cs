using PdfSharp.Charting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace audit
{
    public partial class Overview : Form
    {
        private string Week1start;
        private string Week2start;
        private string Week3start;
        private string Week4start;
        private string RightNow;

        public Overview()
        {
            InitializeComponent();
            if (Storage.DefaultLanguage == "1") { Text = "Prehľad vykonaných LPA auditov"; } else { Text = "Performed LPA audits overview"; }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Overview_Load(object sender, EventArgs e)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum1 = ciCurr.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (Storage.DefaultLanguage == "1") { groupBox4.Text = "Týždeň " + weekNum1.ToString(); } else { groupBox4.Text = "Week " + weekNum1.ToString(); }  
            int weekNum2 = ciCurr.Calendar.GetWeekOfYear(DateTime.Now.AddDays(-7), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (Storage.DefaultLanguage == "1") { groupBox3.Text = "Týždeň " + weekNum2.ToString(); } else { groupBox3.Text = "Week " + weekNum2.ToString(); }
            int weekNum3 = ciCurr.Calendar.GetWeekOfYear(DateTime.Now.AddDays(-14), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (Storage.DefaultLanguage == "1") { groupBox2.Text = "Týždeň " + weekNum3.ToString(); } else { groupBox2.Text = "Week " + weekNum3.ToString(); }
            int weekNum4 = ciCurr.Calendar.GetWeekOfYear(DateTime.Now.AddDays(-21), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (Storage.DefaultLanguage == "1") { groupBox1.Text = "Týždeň " + weekNum4.ToString(); } else { groupBox1.Text = "Week " + weekNum4.ToString(); }

            DateTime Now = DateTime.Now.AddDays(1);
            RightNow = Now.ToString("yyyy-MM-dd 00:00:00");
            DateTime weekstart = DateTime.Now.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek);
            Week1start = weekstart.ToString("yyyy-MM-dd 00:00:00");
            DateTime weekstart2 = weekstart.AddDays(-7);
            Week2start = weekstart2.ToString("yyyy-MM-dd 00:00:00");
            DateTime weekstart3 = weekstart.AddDays(-14);
            Week3start = weekstart3.ToString("yyyy-MM-dd 00:00:00");
            DateTime weekstart4 = weekstart.AddDays(-21);
            Week4start = weekstart4.ToString("yyyy-MM-dd 00:00:00");

            try   // THIS WEEK
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal)) 
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT MachineOrProduct FROM eAudit_LPAoverview WHERE Area = '" + Storage.Area + "' AND DateOfAudit BETWEEN '" + Week1start + "' AND '" + RightNow + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        textBox1.Text += sqlReader["MachineOrProduct"].ToString() + Environment.NewLine;
                    }
                    sqlReader.Close();
                }
            }
            catch (Exception) { }

            try   // LAST WEEK
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT MachineOrProduct FROM eAudit_LPAoverview WHERE Area = '" + Storage.Area + "' AND DateOfAudit BETWEEN '" + Week2start + "' AND '" + Week1start + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        textBox2.Text += sqlReader["MachineOrProduct"].ToString() + Environment.NewLine;
                    }
                    sqlReader.Close();
                }
            }
            catch (Exception) { }

            try   // 2 WEEKS AGO
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT MachineOrProduct FROM eAudit_LPAoverview WHERE Area = '" + Storage.Area + "' AND DateOfAudit BETWEEN '" + Week3start + "' AND '" + Week2start + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        textBox3.Text += sqlReader["MachineOrProduct"].ToString() + Environment.NewLine;
                    }
                    sqlReader.Close();
                }
            }
            catch (Exception) { }

            try   // 3 WEEKS AGO
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT MachineOrProduct FROM eAudit_LPAoverview WHERE Area = '" + Storage.Area + "' AND DateOfAudit BETWEEN '" + Week4start + "' AND '" + Week3start + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        textBox4.Text += sqlReader["MachineOrProduct"].ToString() + Environment.NewLine;
                    }
                    sqlReader.Close();
                }
            }
            catch (Exception) { }

            if (textBox1.Lines.Count() > 24) { textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f,style:FontStyle.Bold); }
            if (textBox1.Lines.Count() > 27) { textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, style: FontStyle.Bold); }
            if (textBox1.Lines.Count() > 30) { textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, style: FontStyle.Bold); }
            if (textBox1.Lines.Count() > 33) { textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, style: FontStyle.Bold); }
            if (textBox1.Lines.Count() > 37) { textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f, style: FontStyle.Bold); }
            if (textBox1.Lines.Count() > 40) { textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6f, style: FontStyle.Bold); }

            if (textBox2.Lines.Count() > 24) { textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, style: FontStyle.Bold); }
            if (textBox2.Lines.Count() > 27) { textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, style: FontStyle.Bold); }
            if (textBox2.Lines.Count() > 30) { textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, style: FontStyle.Bold); }
            if (textBox2.Lines.Count() > 33) { textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, style: FontStyle.Bold); }
            if (textBox2.Lines.Count() > 37) { textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f, style: FontStyle.Bold); }
            if (textBox2.Lines.Count() > 40) { textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6f, style: FontStyle.Bold); }

            if (textBox3.Lines.Count() > 24) { textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, style: FontStyle.Bold); }
            if (textBox3.Lines.Count() > 27) { textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, style: FontStyle.Bold); }
            if (textBox3.Lines.Count() > 30) { textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, style: FontStyle.Bold); }
            if (textBox3.Lines.Count() > 33) { textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, style: FontStyle.Bold); }
            if (textBox3.Lines.Count() > 37) { textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f, style: FontStyle.Bold); }
            if (textBox3.Lines.Count() > 40) { textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6f, style: FontStyle.Bold); }

            if (textBox4.Lines.Count() > 24) { textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, style: FontStyle.Bold); }
            if (textBox4.Lines.Count() > 27) { textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, style: FontStyle.Bold); }
            if (textBox4.Lines.Count() > 30) { textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, style: FontStyle.Bold); }
            if (textBox4.Lines.Count() > 33) { textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, style: FontStyle.Bold); }
            if (textBox4.Lines.Count() > 37) { textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f, style: FontStyle.Bold); }
            if (textBox4.Lines.Count() > 40) { textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6f, style: FontStyle.Bold); }
        }
    }
}
