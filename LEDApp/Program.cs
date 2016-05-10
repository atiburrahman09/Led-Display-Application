using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LEDApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ///**
            //* The user is the administrator of time, direct start application
            // * If not the administrator, use the startup object start program, using run as administrator to ensure
            //  */
            ////Get the current logged on user labeled Windows
            //System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            //System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            ////Judge whether the currently logged in user.
            //if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            //{
            //    //If the administrator, is directly run
            //    Application.Run(new HomePage());
            //}
            //else
            //{
            //    //Create a startup object

            //    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            //    startInfo.UseShellExecute = true;

            //    startInfo.WorkingDirectory = Environment.CurrentDirectory;

            //    startInfo.FileName = Application.ExecutablePath;
            //    //Set the start action, make sure to run as Administrator

            //    startInfo.Verb = "runas";

            //    try
            //    {

            //        System.Diagnostics.Process.Start(startInfo);

            //    }
            //    catch
            //    {

            //        return;

            //    }
            //    //Sign out

            //    //Application.Exit();
            //}
            Application.Run(new HomePage());
        }
    }
}
