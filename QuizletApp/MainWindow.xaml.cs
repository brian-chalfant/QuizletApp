using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
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
    /// TODO: Add hotkeys for Quicker Testing DONE!!!!
    /// TODO: Add Scalability to the Interface NOT DONE, But did make interface Larger
    /// TODO: Eventually add Database and Quiz Creation
    /// TODO: Randomize Questions DONE!
    /// TODO: Slide up window to visualize HotKeys
    /// TODO: Change Checking to an Icon in the window, not a message box. Similarly When Preferences are saved use a statusbar at the bottom?
    public partial class MainWindow : Window
    {
        //Holds List of Questions for Current Quiz
        public required List<Question> questions;
        //Index of Current Question
        private int currentQuestionIndex = 0;
        //Holds user Answers <QuestionNumber, Answer> 
        private Dictionary<int, string> userAnswers = [];
        private Dictionary<int, bool> userChecks = [];
        //Holds List of Recent Files
        private List<string> recentFiles = [];
        //FilePath of Stored Recent Files
        private const string recentFilesPath = "recent.csv";
        public string hotk = "Right Arrow: Next \rLeft Arrow: Previous \rDown Arrow: Check Answer \r A, S, D, F: Radio Buttons";

        //Main Window Function  
        //Constructor
        public MainWindow()
        {

            InitializeComponent();
            DisplayQuestion(true);
            LoadRecentFiles();
            AddHotKeys();
            SetTheme();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
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
            userChecks.Clear();
            SubmitButton.Visibility = Visibility.Visible;
            QuestionNumberBlock.Visibility = Visibility.Visible;
            QuestionTextBlock.Visibility = Visibility.Visible;
            OptionA.Visibility = Visibility.Visible;
            OptionB.Visibility = Visibility.Visible;   
            OptionC.Visibility = Visibility.Visible;
            OptionD.Visibility = Visibility.Visible;
            LoadButton.Visibility = Visibility.Collapsed;
            ScoreTextBlock.Visibility = Visibility.Collapsed;
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
            SetTheme();

            //Shuffle that list so correct answers are not always in the same place
            if (Properties.Settings.Default.ARandomization)
            {
                Random rg = new();
                possibleAnswers.Shuffle<string>(rg);
            }


            QuestionTextBlock.Text = question.Text;
            OptionA.Content = possibleAnswers[0];
            OptionB.Content = possibleAnswers[1];
            OptionC.Content = possibleAnswers[2];
            OptionD.Content = possibleAnswers[3];
            QuestionNumberBlock.Text = (currentQuestionIndex + 1).ToString() + "/" + questions.Count.ToString();

            // Clear any existing radio button selection
            foreach (var radioButton in AnswersPanel.Children.OfType<RadioButton>())
            {
                radioButton.IsChecked = false;

                if (userChecks.ContainsKey(currentQuestionIndex) && Properties.Settings.Default.LockCheckedQuestions)
                {
                    radioButton.IsEnabled = false;
                }
                else
                {
                    radioButton.IsEnabled = true;
                }

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
                userAnswers[currentQuestionIndex] = radioButton.Content?.ToString()?.Trim();
            }
        }

        // Checks the User's Answer to see if it is correct.
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected answer
            var selectedOption = AnswersPanel.Children
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true)?.Content?.ToString();

            // Check if lock setting is enabled
            var isCheckLocked = Properties.Settings.Default.LockCheckedQuestions;
            userChecks[currentQuestionIndex] = true;

            // Alert the user if no answer is selected
            if (string.IsNullOrWhiteSpace(selectedOption))
            {
                MessageBox.Show("Please select an answer.");
                return;
            }

            // Highlight answers
            HighlightAnswers(selectedOption, isCheckLocked);
        }

        // Highlights the correct and incorrect answers
        private void HighlightAnswers(string selectedOption, bool isCheckLocked)
        {
            foreach (var radioButton in AnswersPanel.Children.OfType<RadioButton>())
            {
                var answerContent = radioButton.Content?.ToString()?.Trim();
                if (string.IsNullOrEmpty(answerContent)) continue;

                // Highlight the correct answer
                if (answerContent == questions[currentQuestionIndex].CorrectAnswer.Trim())
                {
                    radioButton.Foreground = new SolidColorBrush(Colors.Green);
                }
                // Highlight incorrect answers if selected
                else if (answerContent == selectedOption)
                {
                    radioButton.Foreground = new SolidColorBrush(Colors.Red);
                }

                // Lock answers if the setting is enabled
                if (isCheckLocked)
                {
                    radioButton.IsEnabled = false;
                }
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
                    Random rg = new();
                    questions.Shuffle<Question>(rg);
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
            double correctPercentage = ((double)correctAnswers / questions.Count) * 100;


            // Hide Questions
            QuestionTextBlock.Text = "";
            OptionA.Visibility = Visibility.Collapsed;
            OptionB.Visibility = Visibility.Collapsed;
            OptionC.Visibility = Visibility.Collapsed;
            OptionD.Visibility = Visibility.Collapsed;
            QuestionNumberBlock.Text = "";
            SubmitButton.Visibility = Visibility.Collapsed;
            PrevButton.Visibility = Visibility.Collapsed;
            ScoreTextBlock.Text = $"You Scored {correctPercentage:F2}%!";

            ScoreTextBlock.Visibility = Visibility.Visible;
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
            PreferencesWindows preferencesWindows = new();
            preferencesWindows.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            preferencesWindows.Show();
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            Window1 AboutWindow = new();
            AboutWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AboutWindow.Show();
        }

        private void ShowQuestionSet(object sender, RoutedEventArgs e)
        {

        }

        private void ShowHotkey_Click(object sender, RoutedEventArgs e)
        {
            Window2 HotkeyWindow = new();
            HotkeyWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            HotkeyWindow.Show();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var radioButtonMapping = new Dictionary<Key, RadioButton>
    {
        { Key.A, OptionA },
        { Key.S, OptionB },
        { Key.D, OptionC },
        { Key.F, OptionD }
    };

            if (radioButtonMapping.TryGetValue(e.Key, out RadioButton radioButton))
            {
                if (radioButton != null && radioButton.IsEnabled)
                {
                    radioButton.IsChecked = true;
                }
            }
        }

        //Keyboard Actions
        // Add Hotkeys for Next, Previous, and Check
        private void AddHotKeys()
        {
            try
            {
                AddHotKey(Key.Right, NextButton_Click, NextButton);
                AddHotKey(Key.Left, PrevButton_Click, PrevButton);
                AddHotKey(Key.Down, SubmitButton_Click, SubmitButton);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding hotkeys: {ex}");
            }
        }

        // Helper Method for Adding HotKeys
        private void AddHotKey(Key key, ExecutedRoutedEventHandler handler, Button associatedButton)
        {
            RoutedCommand command = new();
            command.InputGestures.Add(new KeyGesture(key, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(command, handler, (s, e) => e.CanExecute = associatedButton.IsEnabled && associatedButton.IsVisible));
        }


        public void SetTheme()
        {
            DropShadowEffect QuestionNumberBlockdse = (DropShadowEffect)QuestionNumberBlock.Effect;
            DropShadowEffect LoadButtondse = (DropShadowEffect)LoadButton.Effect;
            DropShadowEffect PrevButtondse = (DropShadowEffect)PrevButton.Effect;
            DropShadowEffect NextButtondse = (DropShadowEffect)NextButton.Effect;
            DropShadowEffect SubmitButtondse = (DropShadowEffect)SubmitButton.Effect;
            Color Background;
            Color QuestionNumberDropShadow;
            Color DropShadow;
            Color Foreground;
            Color QuestionNumber;
            Color SubmitBtnColor;
            Color OtherBtnColor;
            Color OtherBtnBorder;
            Color SubmitBtnBorder;
            Color SlideUpPanelColor;
            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };



            if (Properties.Settings.Default.DarkMode)
            {
                Background = Colors.Black; //Background Color
                DropShadow = Colors.Black;    //Drop Shadow
                Foreground = (Color)ColorConverter.ConvertFromString("#ADD8E6");  //Foreground
                QuestionNumber = Colors.LimeGreen;  //Question Number Color
                SubmitBtnColor = (Color)ColorConverter.ConvertFromString("#1F8A55"); //SubmitButton
                OtherBtnColor = (Color)ColorConverter.ConvertFromString("#0056B3"); //Other Buttons
                OtherBtnBorder = (Color)ColorConverter.ConvertFromString("#004085");  //Other Buttons Border
                SubmitBtnBorder = (Color)ColorConverter.ConvertFromString("#0B6C35"); //SubmitButton Border
                SlideUpPanelColor = (Color)ColorConverter.ConvertFromString("#2C3E50"); //SlideUpPanel

                // Add Gradient Stops
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 0, 0), 0.0)); // #FF0055A5
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 3, 2, 68), 1.0)); // #FF003D73



            }
            else
            {

                Background = Colors.White; //Background Color
                QuestionNumberDropShadow = Colors.DarkGreen;  //QuestionNumber DropShadow color
                DropShadow = Colors.Black;    //Drop Shadow
                Foreground = Colors.Black;  //Foreground
                QuestionNumber = Colors.Green;  //Question Number Color
                SubmitBtnColor = (Color)ColorConverter.ConvertFromString("#28A745"); //SubmitButton
                OtherBtnColor = (Color)ColorConverter.ConvertFromString("#007BFF"); //Other Buttons
                OtherBtnBorder = (Color)ColorConverter.ConvertFromString("#0069D9");  //Other Buttons Border
                SubmitBtnBorder = (Color)ColorConverter.ConvertFromString("#218838"); //SubmitButton Border
                SlideUpPanelColor = (Color)ColorConverter.ConvertFromString("#E3F2FD"); //SlideUpPanel
                                                                                        // Add Gradient Stops
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 85, 165), 0.0)); // #FF0055A5
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 0, 61, 115), 1.0)); // #FF003D73
            }

            QAppMainWindow.Background = new SolidColorBrush(Background);
            QAppMainWindow.Foreground = new SolidColorBrush(Foreground);
            QuestionNumberBlock.Foreground = new SolidColorBrush(QuestionNumber);
            //QuestionNumberBlockdse.Color = QuestionNumberDropShadow;
            //LoadButtondse.Color = DropShadow;
            //PrevButtondse.Color = DropShadow;
            //NextButtondse.Color = DropShadow;
            //SubmitButtondse.Color = DropShadow;
            OptionA.Foreground = new SolidColorBrush(Foreground);
            OptionB.Foreground = new SolidColorBrush(Foreground);
            OptionC.Foreground = new SolidColorBrush(Foreground);
            OptionD.Foreground = new SolidColorBrush(Foreground);
            LoadButton.Background = new SolidColorBrush(OtherBtnColor);
            LoadButton.Foreground = new SolidColorBrush(Foreground);
            LoadButton.BorderBrush = new SolidColorBrush(OtherBtnBorder);
            PrevButton.Background = new SolidColorBrush(OtherBtnColor);
            PrevButton.Foreground = new SolidColorBrush(Foreground);
            PrevButton.BorderBrush = new SolidColorBrush(OtherBtnBorder);
            NextButton.Background = new SolidColorBrush(OtherBtnColor);
            NextButton.Foreground = new SolidColorBrush(Foreground);
            NextButton.BorderBrush = new SolidColorBrush(OtherBtnBorder);
            SubmitButton.Background = new SolidColorBrush(SubmitBtnColor);
            SubmitButton.Foreground = new SolidColorBrush(Foreground);
            SubmitButton.BorderBrush = new SolidColorBrush(SubmitBtnBorder);

            // Set the Background of the Grid (assuming the Grid is named "MainGrid")
            MainGrid.Background = gradientBrush;


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