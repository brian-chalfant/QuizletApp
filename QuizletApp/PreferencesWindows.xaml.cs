﻿using System;
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
            chkAnswers.IsChecked = Properties.Settings.Default.ARandomization;
            chkQuestions.IsChecked = Properties.Settings.Default.QRandomization;
            chkLockedQuestions.IsChecked = Properties.Settings.Default.LockCheckedQuestions;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save preferences logic
            bool isDarkModeEnabled = chkDarkMode.IsChecked ?? false;
            bool areQuestionsRandomized = chkQuestions.IsChecked ?? false;
            bool areAnswersRandomized = chkAnswers.IsChecked ?? false;
            bool areCheckedQuestionsLocked = chkLockedQuestions.IsChecked ?? false;

            // Example: Save to a settings file or application-level settings
            Properties.Settings.Default.DarkMode = isDarkModeEnabled;
            Properties.Settings.Default.ARandomization = areAnswersRandomized;
            Properties.Settings.Default.QRandomization = areAnswersRandomized;
            Properties.Settings.Default.LockCheckedQuestions = areCheckedQuestionsLocked;
            Properties.Settings.Default.Save();
            var mainwindow = (MainWindow)Application.Current.MainWindow;
            mainwindow.SetTheme();
            this.Close();
        }
    }
}
