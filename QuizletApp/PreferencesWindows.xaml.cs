using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuizletApp
{
    /// <summary>
    /// Interaction logic for PreferencesWindows.xaml
    /// </summary>
    public partial class PreferencesWindows : Window
    {
        public PreferencesWindows()
        {
            InitializeComponent();
            chkDarkMode.IsChecked = Properties.Settings.Default.DarkMode;
            chkNotifications.IsChecked = Properties.Settings.Default.Randomization;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save preferences logic
            bool isDarkModeEnabled = chkDarkMode.IsChecked ?? false;
            bool areNotificationsEnabled = chkNotifications.IsChecked ?? false;

            // Example: Save to a settings file or application-level settings
            Properties.Settings.Default.DarkMode = isDarkModeEnabled;
            Properties.Settings.Default.Randomization = areNotificationsEnabled;
            Properties.Settings.Default.Save();

            MessageBox.Show("Preferences saved successfully.");
            this.Close();
        }
    }
}
