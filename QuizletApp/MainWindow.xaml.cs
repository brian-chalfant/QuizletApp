using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using static System.Formats.Asn1.AsnWriter;

namespace QuizletApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Holds List of Questions for Current Quiz
        private List<Question> questions;
        //Index of Current Question
        private int currentQuestionIndex = 0;
        //Holds user Answers <QuestionNumber, Answer> 
        private Dictionary<int, string> userAnswers = new Dictionary<int, string>();
        //Holds List of Recent Files
        private List<string> recentFiles = new List<string>();
        //FilePath of Stored Recent Files
        private const string recentFilesPath = "recent.csv";

        //Main Window Function  
        //Constructor
        public MainWindow()
        {
            InitializeComponent();
            DisplayQuestion(true);
            LoadRecentFiles();
        }


        //RECENT FILE UTILITY FUNCTIONS----------------------------------------------------------------------------

        //If there are Values in the RecentFilesPath Global Variable Then Call UPdateRecentFilesMenu
        private void LoadRecentFiles()
        {
            if (File.Exists(recentFilesPath))
            {
                recentFiles = File.ReadAllLines(recentFilesPath).ToList();
            }
            UpdateRecentFilesMenu();
        }

        //Updates the Recent Files Menu with the Contents of recentFilesPath or Displays No Recent Files if Empty
        private void UpdateRecentFilesMenu()
        {
            RecentFilesMenu.Items.Clear();

            if (recentFiles.Count > 0)
            {
                foreach (var file in recentFiles)
                {
                    var menuItem = new MenuItem { Header = file };
                    menuItem.Click += (s, e) => OpenRecentFile(file);
                    RecentFilesMenu.Items.Add(menuItem);
                }
            }
            else
            {
                RecentFilesMenu.Items.Add(new MenuItem { Header = "No recent files", IsEnabled = false });
            }
        }

        //When Passed the filepath of a file, Open it and try to load the questions 
        private void OpenRecentFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Call your method to load the CSV file
                LoadQuestions(filePath);
            }
            else
            {
                MessageBox.Show("File not found: " + filePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                recentFiles.Remove(filePath);
                SaveRecentFiles();
                UpdateRecentFilesMenu();
            }
        }

        private void SaveRecentFiles()
        {
            File.WriteAllLines(recentFilesPath, recentFiles);
        }


        //INITIALIZATION METHOD-----------------------------------------------------------------------------


        //Sets the quiz components to Visible, hides the Load button, Removes any text from the components.
        private void InitializeQuiz()
        {
            currentQuestionIndex = 0;  // Reset to first question
            userAnswers.Clear(); // Clear the stored user answers
                                 // Set the progress value for the circular progress bar
            sfCircularProgressBar.Progress = 0;
            sfCircularProgressBar.Visibility = Visibility.Collapsed;
            SubmitButton.Visibility = Visibility.Visible;
            QuestionNumberBlock.Visibility = Visibility.Visible;
            QuestionTextBlock.Visibility = Visibility.Visible;
            OptionA.Visibility = Visibility.Visible;
            OptionB.Visibility = Visibility.Visible;   
            OptionC.Visibility = Visibility.Visible;
            OptionD.Visibility = Visibility.Visible;
            LoadButton.Visibility = Visibility.Collapsed;
            NextButton.Visibility = Visibility.Visible;
            NextButton.Content = "Next";
            PrevButton.Visibility = Visibility.Visible;
            PrevButton.IsEnabled = false;
            SubmitButton.Visibility = Visibility.Visible;
            QuestionTextBlock.Text = string.Empty;
            QuestionNumberBlock.Text = string.Empty;
            OptionA.Content = string.Empty;
            OptionB.Content = string.Empty;
            OptionC.Content = string.Empty;
            OptionD.Content = string.Empty;

        }


        //Loads the next question onto the page.
        private void DisplayQuestion(bool init)
        {
            if (questions == null || currentQuestionIndex >= questions.Count)
            {
                return;
            }
            
            if(init)
            {
                InitializeQuiz();
            }
            if(currentQuestionIndex > 0)
            {
                PrevButton.IsEnabled = true;
            }
            var question = questions[currentQuestionIndex];

            //Add the possible answers to a list
            var possibleAnswers = new List<string>();
            possibleAnswers.Add(question.OptionA);
            possibleAnswers.Add(question.OptionB);
            possibleAnswers.Add(question.OptionC);
            possibleAnswers.Add(question.OptionD);

            //Shuffle that list so correct answers are not always in the same place
            if (Properties.Settings.Default.Randomization)
            {
                Random rg = new Random();
                possibleAnswers.Shuffle<string>(rg);
            }


            QuestionTextBlock.Text = question.Text;
            OptionA.Content = possibleAnswers[0];
            OptionB.Content = possibleAnswers[1];
            OptionC.Content = possibleAnswers[2];
            OptionD.Content = possibleAnswers[3];
            QuestionNumberBlock.Text = (currentQuestionIndex + 1).ToString();

            // Clear any existing radio button selection
            foreach (var radioButton in AnswersPanel.Children.OfType<RadioButton>())
            {
                radioButton.IsChecked = false;
            }

            // Check if the user already answered this question
            if (userAnswers.ContainsKey(currentQuestionIndex))
            {
                string selectedAnswer = userAnswers[currentQuestionIndex];
                // Check the radio button corresponding to the user's answer
                foreach (var radioButton in AnswersPanel.Children.OfType<RadioButton>())
                {
                    if (radioButton.Content.ToString() == selectedAnswer)
                    {
                        radioButton.IsChecked = true;
                    }
                }
            }
        }


        //BUTTONS--------------------------------------------------------------------------------


        //Saves the User's Answer when Radio Button is Pressed
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                // Save the selected answer for the current question
                userAnswers[currentQuestionIndex] = radioButton.Content.ToString();
            }
        }

        //Checks the User's Answer to see if it is correct.
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected answer
            var selectedOption = AnswersPanel.Children.OfType<RadioButton>()
                                    .FirstOrDefault(r => r.IsChecked == true)?.Content.ToString();

            if (selectedOption == null)
            {
                MessageBox.Show("Please select an answer.");
                return;
            }

            // Compare the selected answer to the correct answer
            if (selectedOption.Trim().Equals(questions[currentQuestionIndex].CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Correct!");
            }
            else
            {
                MessageBox.Show($"Incorrect! The correct answer is: {questions[currentQuestionIndex].CorrectAnswer}");
            }
        }

        //Next Question
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {


            // If the Next Button's content is "Finish" and clicked, show the score and change button text to "Restart"
            if (NextButton.Content.ToString() == "Finish")
            {
                if (MessageBox.Show("Do you want to end this quiz?",
                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ShowScore();
                    NextButton.Content = "Restart";

                }

            }
            else if (NextButton.Content.ToString() == "Restart")
            {
                NextButton.Content = "Next";  // Change back to "Next"
                InitializeQuiz();
                DisplayQuestion(true);


            }
            // If we're at the last question, change button text to "Finish"
            else if (currentQuestionIndex >= questions.Count - 2)
            {
                if (NextButton.Content.ToString() == "Next")
                {
                    // Change the button text to "Finish" when it's the last question
                    NextButton.Content = "Finish";
                }
                currentQuestionIndex++;
                DisplayQuestion(false);
                // Proceed to the next question if it's not the last one
            }
            else
            {
                currentQuestionIndex++;
                DisplayQuestion(false);
            }

        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if(NextButton.Content.ToString() == "Finish")
            {
                NextButton.Content = "Next";
            }
            currentQuestionIndex--;
            if (currentQuestionIndex == 0)
            {
                PrevButton.IsEnabled = false;
            }
            DisplayQuestion(false);
        }


        //MENU BUTTONS -------------------------------------------------------------

        //Load a CSV File from a File Dialog Box
        private void LoadCSV_Click(object sender, RoutedEventArgs e) 
        {
            // Open File Dialog to Select a CSV File
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Update recent files
                if (!recentFiles.Contains(openFileDialog.FileName))
                {
                    recentFiles.Insert(0, openFileDialog.FileName); // Add to the top
                    if (recentFiles.Count > 10) // Limit to 10 recent files
                        recentFiles.RemoveAt(recentFiles.Count - 1);
                }

                SaveRecentFiles();
                UpdateRecentFilesMenu();

                LoadQuestions(openFileDialog.FileName);
                currentQuestionIndex = 0; // Reset to first question
                foreach (var radioButton in AnswersPanel.Children.OfType<RadioButton>())
                {
                    radioButton.IsChecked = false;
                }
                DisplayQuestion(true);
            }
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        //Load Questions from a given Filepath
        private void LoadQuestions(string filePath)
        {
            InitializeQuiz();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                }))
                {
                    questions = csv.GetRecords<Question>().ToList();
                    MessageBox.Show($"Successfully loaded {questions.Count} questions.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}");
            }
            DisplayQuestion(true);
        }


        //Quiz is over, Show the Score
        private void ShowScore()
        {
            int correctAnswers = 0;

            // Loop through each question and check if the selected answer is correct
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                if (userAnswers.ContainsKey(i))
                {
                    var selectedOption = userAnswers[i];

                    // Compare selected answer to correct answer
                    if (selectedOption != null && selectedOption.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        correctAnswers++;
                    }
                }
            }

            // Calculate and display the score
            // Calculate the percentage of correct and incorrect answers
            double correctPercentage = (double)correctAnswers / questions.Count;

            // Set the progress value for the circular progress bar
            sfCircularProgressBar.Progress = correctPercentage * 100;
            sfCircularProgressBar.Visibility = Visibility.Visible;

            // Hide Questions
            QuestionTextBlock.Text = "";
            OptionA.Visibility = Visibility.Collapsed;
            OptionB.Visibility = Visibility.Collapsed;
            OptionC.Visibility = Visibility.Collapsed;
            OptionD.Visibility = Visibility.Collapsed;
            QuestionNumberBlock.Text = "";
            SubmitButton.Visibility = Visibility.Collapsed;
            PrevButton.Visibility = Visibility.Collapsed;   
            NextButton.Content = "Restart";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(recentFilesPath, string.Empty);
            recentFiles.Clear();
            UpdateRecentFilesMenu();
        }

        private void PreferencesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PreferencesWindows preferencesWindows = new PreferencesWindows();
            preferencesWindows.Show();
        }
    }


    //Thanks Jason on StackOverflow for this handly little Shuffle Function. Super Handy.
    static class IListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random rg)
        {
            for (int i = list.Count; i > 1; i--)
            {
                int k = rg.Next(i);
                T temp = list[k];
                list[k] = list[i - 1];
                list[i - 1] = temp;
            }
        }
    }
}