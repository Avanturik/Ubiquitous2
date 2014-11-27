using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ubiquitous2Certificate
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

            InstallCertificate();
        }

        private static void InstallCertificate()
        {
            var cerResource = new ResourceFile("xedoc.cer");
            var fileName = Path.GetTempPath() + @"xedoc.cer";
            cerResource.SaveToFile(fileName, (err) => {
                if (String.IsNullOrWhiteSpace(err))
                    MessageBox.Show(String.Format("Error saving certificate to {0}: {1}", fileName, err));

                return;
            });


            try
            {
                X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                rootStore.Open(OpenFlags.ReadWrite);
                rootStore.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(fileName)));
                rootStore.Close();
                MessageBox.Show("Certificate successfuly installed!");
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Error installing certificate from:{0}, {1}", fileName, e.Message));
            }

        }
    }
}
