
using DVLD.Login;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace DVLD
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Read the saved language.
            string language =
                Properties.Settings.Default.UiLanguage;

            // Use English if no language is saved.
            if (language != "ar-PS" && language != "en")
            {
                language = "en";
            }

            // Set the UI language before creating forms.
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(language);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new frmLogin());
        }
    }
}
