using audit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Process ThisProcess = Process.GetCurrentProcess();  //avoid multiple run
            Process[] AllProcesses = Process.GetProcessesByName(ThisProcess.ProcessName);
            if (AllProcesses.Length > 1)
            {
                return;
            }

            Process[] processlist = Process.GetProcesses(); //avoid eAudit multiple run
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName == "5S eAudit")
                {
                    DialogResult dialogResult = MessageBox.Show("Iba jedna eAudit aplikácia môže byť spustená. Ukončiť 5S eAudit a pokračovať s LPA eAudit?" + Environment.NewLine + Environment.NewLine + "Only one eAudit instance can be running at a time. Close 5S eAudit and continue with LPA eAudit?", "eAudit", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes){theprocess.Kill();}
                    else{ThisProcess.Kill();}
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartPage());
        }
    }
}
