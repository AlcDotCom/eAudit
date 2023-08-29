using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace audit
{
    public partial class ByeByePage : Form
    {
        public ByeByePage()
        {
            InitializeComponent();
            timer1 = new Timer(); timer1.Tick += new EventHandler(Bye); timer1.Interval = 3000; timer1.Start();

            if (Storage.DefaultLanguage == "1") 
            {
                button1.Text = Environment.NewLine + Environment.NewLine + "Audit bol uložený" + Environment.NewLine + "...ukončujem LPA eAudit";
            }
            else 
            {
                button1.Text = Environment.NewLine + Environment.NewLine + "Audit was saved" + Environment.NewLine + "...closing LPA eAudit";
            }
        }
        private void Bye(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
