using System.Configuration;
using System.Data;
using System.Windows;
using Syncfusion.Licensing;

namespace QuizletApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var test = Environment.GetEnvironmentVariables();
            // test enumerates all the Env variables, don't see it there
            var key = Environment.GetEnvironmentVariable("LICENSE_KEY");
            if (string.IsNullOrWhiteSpace(key)) // so this is obviously null
                throw new ArgumentNullException("LICENSE_KEY");
            SyncfusionLicenseProvider.RegisterLicense(key);
            InitializeComponent();
        }
    }

}
