using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Principal;

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

            /**
            * The user is the administrator of time, direct start application
            * If not the administrator, use the startup object start program, using run as administrator to ensure
            */
            //Get the current logged on user labeled Windows
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //Judge whether the currently logged in user.
            if (!principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                //If the administrator, is directly run
                //Application.Run(new HomePage());
                //Create a startup object
                Process p = new Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                p.StartInfo.FileName = Application.ExecutablePath;
                p.StartInfo.FileName = Assembly.GetExecutingAssembly().CodeBase;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //Set the start action, make sure to run as Administrator
                p.StartInfo.Verb = "runas";
                try
                {
                    p.Start();
                    p.EnableRaisingEvents = true;
                    //Application.Exit();
                    //System.Windows.Forms.Application.ExitThread();


                }
                catch
                {
                    return;
                }

                //Sign out
                //Process.GetCurrentProcess().Kill();
                Application.Run(new HomePage());
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                //Application.Exit();
            }
            //else
            //{

            //    Application.Run(new HomePage());

            //}


            //Working Code
            //WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            //bool administrativeMode = principal.IsInRole(WindowsBuiltInRole.Administrator);

            //if (!administrativeMode)
            //{
            //    Process p = new Process();
            //    p.StartInfo.UseShellExecute = true;
            //    p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            //    p.StartInfo.Verb = "runas";
            //    p.StartInfo.FileName = Assembly.GetExecutingAssembly().CodeBase;
            //    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //    try
            //    {
            //        p.Start();
            //        //Application.Exit();
            //        //alreadyAskedPermission = true;
            //        p.EnableRaisingEvents = true;
            //       // p.Exited += new EventHandler(p_Exited);

            //    }
            //    catch
            //    {
            //        //User denied access
            //        return;
            //    }
            //    Application.Exit();
            //}
            //Application.Run(new HomePage());


           
        }

        

    }
}
