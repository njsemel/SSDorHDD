using System;
using System.Windows.Forms;

namespace SSDorHDD
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); // Starts the application with MainForm
        }
    }
}
