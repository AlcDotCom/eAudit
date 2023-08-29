using Audit;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace audit
{
    public partial class PictureViewer : Form
    {
        public PictureViewer()
        {
            InitializeComponent(); 
            if (Storage.DefaultLanguage == "1") { Text = "Fotky"; } else { Text = "Pictures"; }
        }

        private void PictureViewer_Load(object sender, EventArgs e)
        {
            LoadPhotosToCBX();
        }

        private void LoadPhotosToCBX()  
        {
            try   //Load photos to CBX for this particular question(code) for view    
            {
                using (SqlConnection sqlConnection = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    SqlCommand sqlCmd = new SqlCommand("SELECT PhotoFileName1,PhotoFileName2,PhotoFileName3,CreationTime1,CreationTime2,CreationTime3 FROM eAudit_LPAResults WHERE QuestionCode = '" + Storage.ActualQuestionCode + "' AND AuditNum = '" + Storage.AuditNum + "' ", sqlConnection);
                    sqlConnection.Open(); SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        if (!string.IsNullOrEmpty(sqlReader["PhotoFileName1"].ToString()) && Convert.ToDateTime(sqlReader["CreationTime1"].ToString()) > Storage.AuditStart)
                        { comboBox1.Items.Add(sqlReader["PhotoFileName1"].ToString()); comboBox1.SelectedIndex = 0; }
                        if (!string.IsNullOrEmpty(sqlReader["PhotoFileName2"].ToString()) && Convert.ToDateTime(sqlReader["CreationTime2"].ToString()) > Storage.AuditStart)
                        { comboBox1.Items.Add(sqlReader["PhotoFileName2"].ToString()); comboBox1.SelectedIndex = 0; }
                        if (!string.IsNullOrEmpty(sqlReader["PhotoFileName3"].ToString()) && Convert.ToDateTime(sqlReader["CreationTime3"].ToString()) > Storage.AuditStart)
                        { comboBox1.Items.Add(sqlReader["PhotoFileName3"].ToString()); comboBox1.SelectedIndex = 0; }

                        label1.Text = (comboBox1.SelectedIndex + 1).ToString() + "/" + comboBox1.Items.Count.ToString();
                    }
                    sqlConnection.Close();
                }
            }
            catch (Exception) { }
            if (comboBox1.Items.Count < 2) { pictureBox3.Visible = false; pictureBox4.Visible = false; } else { pictureBox3.Visible = true; pictureBox4.Visible = true; }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try { pictureBox1.Image = Image.FromFile(Storage.PhotoPath + "/" + comboBox1.SelectedItem); } catch (Exception) { }
            label1.Text = (comboBox1.SelectedIndex + 1).ToString() + "/" + comboBox1.Items.Count.ToString();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Dispose();Close();
        }
        public new void Dispose()
        {
                pictureBox2.Image.Dispose();
                pictureBox2.Image = null;
                pictureBox5.Image.Dispose();
                pictureBox5.Image = null;
                pictureBox3.Image.Dispose();
                pictureBox3.Image = null;
                pictureBox4.Image.Dispose();
                pictureBox4.Image = null;
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
        }

        private void pictureBox5_Click(object sender, EventArgs e) //DELETE pic
        {
            if (comboBox1.SelectedIndex != -1)
            {
                if (Storage.DefaultLanguage == "1") 
                {
                    DialogResult dialogResult = MessageBox.Show("Vymazať fotku?", "VÝMAZ FOTKY", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes) { DeletePic();}
                    else if (dialogResult == DialogResult.No) { return;}
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Delete this photo?", "DELETE PHOTO", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes) {DeletePic();}
                    else if (dialogResult == DialogResult.No) {return;}
                }
            }
        }
        private void DeletePic() //DELETE pic
        {
            try
            {
                using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    Conn.Open();
                    string Insert = "UPDATE eAudit_LPAResults SET PhotoFileName1 = '' WHERE PhotoFileName1 = '" + comboBox1.SelectedItem + "' AND QuestionCode = '" + Storage.ActualQuestionCode + "'  ";
                    SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                }
                using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    Conn.Open();
                    string Insert = "UPDATE eAudit_LPAResults SET PhotoFileName2 = '' WHERE PhotoFileName2 = '" + comboBox1.SelectedItem + "' AND QuestionCode = '" + Storage.ActualQuestionCode + "'  ";
                    SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                }
                using (SqlConnection Conn = new SqlConnection(Connection.ConnectionStringLocal))
                {
                    Conn.Open();
                    string Insert = "UPDATE eAudit_LPAResults SET PhotoFileName3 = '' WHERE PhotoFileName3 = '" + comboBox1.SelectedItem + "' AND QuestionCode = '" + Storage.ActualQuestionCode + "'  ";
                    SqlCommand cmd = new SqlCommand(Insert, Conn); cmd.ExecuteNonQuery();
                }
            }
            catch (Exception) { }


            if (comboBox1.Items.Count > 1) //Load remaining photos or close if none left
            {
                comboBox1.Items.Clear();    
                LoadPhotosToCBX();
            }
            else { Close();}    
        }

        private void pictureBox4_Click(object sender, EventArgs e) //NEXT pic
        {
            if (comboBox1.SelectedIndex < comboBox1.Items.Count - 1)
            { comboBox1.SelectedIndex = comboBox1.SelectedIndex + 1; }
        }

        private void pictureBox3_Click(object sender, EventArgs e) //PREVIOUS pic
        {
            if (comboBox1.SelectedIndex > 0)
            { comboBox1.SelectedIndex = comboBox1.SelectedIndex - 1; }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int borderRadius = 36;  //param
            RectangleF Rect = new RectangleF(0, 0, pictureBox1.Width, pictureBox1.Height); 
            GraphicsPath GraphPath = RoundedButton.GetRoundPath(Rect, borderRadius);
            pictureBox1.Region = new Region(GraphPath);  //object
        }


    }
}
