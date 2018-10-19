using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace K_Relay
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            SetErrorMode(ErrorModes.SEM_NOGPFAULTERRORBOX | ErrorModes.SEM_NOOPENFILEERRORBOX); // this funtion prevents error dialog box to show up after application crash

            EmbeddedAssembly.Load("K_Relay.MetroFramework.dll", "K_Relay.MetroFramework.dll");
            EmbeddedAssembly.Load("K_Relay.MetroFramework.Design.dll", "K_Relay.MetroFramework.Design.dll");
            EmbeddedAssembly.Load("K_Relay.MetroFramework.Fonts.dll", "K_Relay.MetroFramework.Fonts.dll");


            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            DoAppSetup();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        private static void DoAppSetup()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMainMetro());
        }
    }
}
