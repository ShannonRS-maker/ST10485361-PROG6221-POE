using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite; // Added for SQLite database functionality

namespace Prog_part1
{
    public partial class MainWindow : Window
    {
        private const string DbConnectionString = "Data Source=tasks.db"; // SQLite database file path
        private List<CybersecurityTask> _tasksList = new List<CybersecurityTask>();
        private List<string> _activityLogs = new List<string>();
        
        private List<QuizQuestion> _quizQuestions = new List<QuizQuestion>();
        private int _currentQuestionIndex = 0;
        private int _userScore = 0;

        private bool _nlpExpectingReminderValue = false;
        private string _nlpPendingTitle = "";
        private readonly Random _randomEngine = new Random();

        public MainWindow()
        {
            InitializeComponent();
            
            // Move UI component updates to fire cleanly after window renders
            Dispatcher.UIThread.Post(() =>
            {
                SetupQuiz();
                InitializeDatabase(); // Ensure the database table is created
                LoadTasksFromDatabase(); // Retrieve data directly from SQLite
                LogAction("Application initialized and GUI layers mounted.");
                DisplayQuizQuestion();
                
                var chatHistory = this.FindControl<ListBox>("LstChatHistory");
                if (chatHistory != null)
                {
                    chatHistory.Items.Add("🤖 ChatBot: Online. Ask me about security topics or manage tasks via chat commands.");
                }
            });
        }

        private void LogAction(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _activityLogs.Add(entry);
            UpdateLogDisplay(showAll: false);
        }

        private void UpdateLogDisplay(bool showAll)
        {
            var logBox = this.FindControl<ListBox>("LstActivityLog");
            if (logBox == null) return;

            logBox.Items.Clear();
            var logsToDisplay = showAll ? _activityLogs : _activityLogs.Skip(Math.Max(0, _activityLogs.Count - 8)).ToList();
            foreach (var log in logsToDisplay)
            {
                logBox.Items.Add(log);
            }
            logBox.SelectedIndex = logBox.Items.Count - 1;
        }

        public void OnShowFullLogClicked(object sender, RoutedEventArgs e)
        {
            UpdateLogDisplay(showAll: true);
            LogAction("User requested full system log dump.");
        }

        // --- SQLITE DATABASE OPERATIONS (CRUD) ---

        private void InitializeDatabase()
        {
            try
            {
                using (var connection = new SqliteConnection(DbConnectionString))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Tasks (
                            Id TEXT PRIMARY KEY,
                            Title TEXT NOT NULL,
                            Description TEXT,
                            Reminder TEXT,
                            Status TEXT NOT NULL
                        );";
                    using (var command = new SqliteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                LogAction("SQLite Database structure validated successfully.");
            }
            catch (Exception ex)
            {
                LogAction($"Database initialization error: {ex.Message}");
            }
        }

        private void LoadTasksFromDatabase()
        {
            try
            {
                _tasksList.Clear();
                using (var connection = new SqliteConnection(DbConnectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT Id, Title, Description, Reminder, Status FROM Tasks;";
                    using (var command = new SqliteCommand(selectQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var task = new CybersecurityTask
                            {
                                Id = reader.GetString(0),
                                Title = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Reminder = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Status = reader.GetString(4)
                            };
                            _tasksList.Add(task);
                        }
                    }
                }
                UpdateTasksUI();
                LogAction("Tasks synchronized successfully from local SQLite database.");
            }
            catch (Exception ex)
            {
                LogAction($"Database retrieval failure: {ex.Message}");
            }
        }

        private void AddTaskToDatabase(CybersecurityTask task)
        {
            try
            {
                using (var connection = new SqliteConnection(DbConnectionString))
                {
                    connection.Open();
                    string insertQuery = @"
                        INSERT INTO Tasks (Id, Title, Description, Reminder, Status)
                        VALUES (@Id, @Title, @Description, @Reminder, @Status);";
                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", task.Id ?? Guid.NewGuid().ToString());
                        command.Parameters.AddWithValue("@Title", task.Title ?? "");
                        command.Parameters.AddWithValue("@Description", task.Description ?? "");
                        command.Parameters.AddWithValue("@Reminder", task.Reminder ?? "");
                        command.Parameters.AddWithValue("@Status", task.Status ?? "Pending");
                        command.ExecuteNonQuery();
                    }
                }
                LogAction($"[DB INSERT SUCCESS] Created: \"{task.Title}\"");
            }
            catch (Exception ex)
            {
                LogAction($"Database write operation failed: {ex.Message}");
            }
        }

        private void CompleteTaskInDatabase(string targetId)
        {
            try
            {
                using (var connection = new SqliteConnection(DbConnectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Tasks SET Status = 'Completed' WHERE Id = @Id;";
                    using (var command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", targetId);
                        command.ExecuteNonQuery();
                    }
                }
                LogAction($"[DB UPDATE SUCCESS] Task marked complete.");
            }
            catch (Exception ex)
            {
                LogAction($"Database status update failed: {ex.Message}");
            }
        }

        private void DeleteTaskFromDatabase(string targetId)
        {
            try
            {
                using (var connection = new SqliteConnection(DbConnectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM Tasks WHERE Id = @Id;";
                    using (var command = new SqliteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", targetId);
                        command.ExecuteNonQuery();
                    }
                }
                LogAction($"[DB DELETE SUCCESS] Purged record matching key constraint.");
            }
            catch (Exception ex)
            {
                LogAction($"Database delete command failed: {ex.Message}");
            }
        }

        private void UpdateTasksUI()
        {
            var tasksBox = this.FindControl<ListBox>("LstTasks");
            if (tasksBox == null) return;

            tasksBox.Items.Clear();
            foreach (var task in _tasksList)
            {
                tasksBox.Items.Add(task);
            }
        }

        public void OnAddTaskClicked(object sender, RoutedEventArgs e)
        {
            var titleBox = this.FindControl<TextBox>("TxtTaskTitle");
            var descBox = this.FindControl<TextBox>("TxtTaskDesc");
            var reminderBox = this.FindControl<TextBox>("TxtTaskReminder");

            if (titleBox == null || descBox == null || reminderBox == null) return;
            
            string title = titleBox.Text ?? "";
            string desc = descBox.Text ?? "";
            string reminder = reminderBox.Text ?? "";

            if (string.IsNullOrWhiteSpace(title)) return;

            var newTask = new CybersecurityTask 
            { 
                Id = Guid.NewGuid().ToString(), // Establish unique identifier
                Title = title, 
                Description = desc, 
                Reminder = reminder,
                Status = "Pending"
            };

            AddTaskToDatabase(newTask);
            LoadTasksFromDatabase(); // Refresh local list state

            titleBox.Text = ""; descBox.Text = ""; reminderBox.Text = "";
        }

        public void OnCompleteTaskClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string targetId)
            {
                CompleteTaskInDatabase(targetId);
                LoadTasksFromDatabase(); // Refresh local list state
            }
        }

        public void OnDeleteTaskClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string targetId)
            {
                DeleteTaskFromDatabase(targetId);
                LoadTasksFromDatabase(); // Refresh local list state
            }
        }

        private void SetupQuiz()
        {
            _quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion { QuestionText = "What indicates an unexpected email is a phishing attempt?", Options = new List<string>{"Urgent requests for logins", "Standard sender address", "High graphic styling"}, CorrectIndex = 0, Explanation = "Phishing uses artificial urgency to panic targets into logging into spoofed interfaces." },
                new QuizQuestion { QuestionText = "True or False: Multi-Factor Authentication blocks the vast majority of automated access attempts.", Options = new List<string>{"True", "False"}, CorrectIndex = 0, Explanation = "MFA requires an independent second verification channel." },
                new QuizQuestion { QuestionText = "Which option represents the strongest default home router protection algorithm?", Options = new List<string>{"WEP", "WPA2", "WPA3"}, CorrectIndex = 2, Explanation = "WPA3 implements randomized cryptographic key generation schemes to prevent passive decryption." },
                new QuizQuestion { QuestionText = "What constitutes bad password hygiene?", Options = new List<string>{"Reusing your main password across accounts", "Using passphrase lines", "Using random salts"}, CorrectIndex = 0, Explanation = "If a single node leaks, credential attacks immediately target your other accounts." },
                new QuizQuestion { QuestionText = "True or False: Using public Wi-Fi leaves data streams vulnerable to local packet snooping.", Options = new List<string>{"True", "False"}, CorrectIndex = 0, Explanation = "Public links let unauthenticated intercept terminals run packet trace tools." },
                new QuizQuestion { QuestionText = "What represents the core tactic vector of social engineering?", Options = new List<string>{"Brute forcing credentials", "Manipulating human psychology", "Compiling buffer exploits"}, CorrectIndex = 1, Explanation = "Social engineers target user behavior directly rather than system code flaws." },
                new QuizQuestion { QuestionText = "How should firmware updates be handled on home router endpoints?", Options = new List<string>{"Ignore updates completely", "Install patches regularly", "Update once every five years"}, CorrectIndex = 1, Explanation = "Up-to-date firmware patches known zero-day vulnerabilities." },
                new QuizQuestion { QuestionText = "True or False: Antivirus packages catch every single malicious file threat signature.", Options = new List<string>{"True", "False"}, CorrectIndex = 1, Explanation = "Polymorphic zero-day payloads look completely clean until runtime execution fires." },
                new QuizQuestion { QuestionText = "What is the primary action goal of hidden spyware?", Options = new List<string>{"Formatting localized drives", "Secretly harvesting keystrokes and logs", "Spamming popups"}, CorrectIndex = 1, Explanation = "Spyware monitors background user configurations to steal credentials silently." },
                new QuizQuestion { QuestionText = "True or False: Reputable Virtual Private Networks fully encrypt egress traffic out from your machine.", Options = new List<string>{"True", "False"}, CorrectIndex = 0, Explanation = "VPNs establish encrypted tunneling protocols shielding traffic details from your ISP." },
                new QuizQuestion { QuestionText = "What specifies that your session connection path to a website is fully encrypted?", Options = new List<string>{"The address bar begins with HTTPS", "The domain uses a .org extension", "The site loads text incredibly fast"}, CorrectIndex = 0, Explanation = "The HTTPS string verifies cryptographic SSL/TLS handshake certificates." }
            };
        }

        private void DisplayQuizQuestion()
        {
            var progressText = this.FindControl<TextBlock>("TxtQuizProgress");
            var questionText = this.FindControl<TextBlock>("TxtQuestionText");
            var optionsPanel = this.FindControl<StackPanel>("PnlQuizOptions");
            var feedbackText = this.FindControl<TextBlock>("TxtQuizFeedback");
            var nextBtn = this.FindControl<Button>("BtnNextQuestion");

            if (progressText == null || questionText == null || optionsPanel == null || feedbackText == null || nextBtn == null) return;

            if (_currentQuestionIndex < _quizQuestions.Count)
            {
                var q = _quizQuestions[_currentQuestionIndex];
                progressText.Text = $"Question {_currentQuestionIndex + 1} of {_quizQuestions.Count}";
                questionText.Text = q.QuestionText;
                feedbackText.Text = "Select an option below to check your answer:";
                nextBtn.IsEnabled = false;

                optionsPanel.Children.Clear();
                for (int i = 0; i < q.Options.Count; i++)
                {
                    var btn = new Button 
                    { 
                        Content = q.Options[i], 
                        HorizontalAlignment = HorizontalAlignment.Stretch, 
                        HorizontalContentAlignment = HorizontalAlignment.Center, 
                        Background = Brush.Parse("#2C2C2E"), 
                        Padding = new Thickness(10), 
                        Tag = i 
                    };
                    btn.Click += OnQuizOptionClicked;
                    optionsPanel.Children.Add(btn);
                }
            }
        }

        private void OnQuizOptionClicked(object? sender, RoutedEventArgs e)
        {
            var optionsPanel = this.FindControl<StackPanel>("PnlQuizOptions");
            var feedbackText = this.FindControl<TextBlock>("TxtQuizFeedback");
            var nextBtn = this.FindControl<Button>("BtnNextQuestion");

            if (optionsPanel == null || feedbackText == null || nextBtn == null) return;
            if (sender is Button sourceBtn && sourceBtn.Tag is int selection)
            {
                var q = _quizQuestions[_currentQuestionIndex];
                foreach (Control c in optionsPanel.Children) if (c is Button b) b.IsEnabled = false;

                if (selection == q.CorrectIndex)
                {
                    _userScore++;
                    sourceBtn.Background = Brush.Parse("#34C759");
                    feedbackText.Text = $"✅ Correct! {q.Explanation}";
                }
                else
                {
                    sourceBtn.Background = Brush.Parse("#FF453A");
                    feedbackText.Text = $"❌ Incorrect choice. {q.Explanation}";
                }
                nextBtn.IsEnabled = true;
            }
        }

        public void OnNextQuestionClicked(object sender, RoutedEventArgs e)
        {
            var nextBtn = this.FindControl<Button>("BtnNextQuestion");
            if (nextBtn == null) return;

            // Handle the restart scenario cleanly
            if (nextBtn.Content != null && nextBtn.Content.ToString().Contains("Restart"))
            {
                _currentQuestionIndex = 0;
                _userScore = 0;
                nextBtn.Content = "Next Question ➡️";
                DisplayQuizQuestion();
                LogAction("[QUIZ STATE METRICS RESET] User restarted quiz loop.");
                return;
            }

            if (_currentQuestionIndex < _quizQuestions.Count - 1)
            {
                _currentQuestionIndex++;
                DisplayQuizQuestion();
            }
            else if (_currentQuestionIndex == _quizQuestions.Count - 1)
            {
                _currentQuestionIndex++;

                // Dynamic gamification tier logic
                string rankTitle;
                string rankAdvice;

                if (_userScore >= 9)
                {
                    rankTitle = "🛡️ RANK: CYBER SENTINEL (Optimal Hygiene)";
                    rankAdvice = "Excellent work! Your defensive knowledge is sharp and deployment-ready.";
                }
                else if (_userScore >= 5)
                {
                    rankTitle = "⚠️ RANK: STANDARD USER (Basic Protection Active)";
                    rankAdvice = "Review your basic cyber hygiene protocols to eliminate remaining vulnerabilities.";
                }
                else
                {
                    rankTitle = "🛑 RANK: SECURITY RISK (Review Required)";
                    rankAdvice = "Critical gaps detected. Re-run the modules to reinforce core asset protection.";
                }

                var progressText = this.FindControl<TextBlock>("TxtQuizProgress");
                var questionText = this.FindControl<TextBlock>("TxtQuestionText");
                var optionsPanel = this.FindControl<StackPanel>("PnlQuizOptions");
                var feedbackText = this.FindControl<TextBlock>("TxtQuizFeedback");

                if (progressText != null) progressText.Text = "Quiz Finished! 🏁";
                if (questionText != null)
                {
                    questionText.Text = $"{rankTitle}\n\nYour final score tracking reads: {_userScore} / {_quizQuestions.Count}\n\n{rankAdvice}";
                }
                if (optionsPanel != null) optionsPanel.Children.Clear();
                if (feedbackText != null) feedbackText.Text = "Review your rank above.";

                nextBtn.Content = "Restart Assessment 🔄";
                nextBtn.IsEnabled = true;

                LogAction($"[QUIZ ATTEMPT COMPLETED] Final calculation tracking score: {_userScore}/{_quizQuestions.Count}");
            }
        }

        private void RunNLPAnalysis()
        {
            var chatInput = this.FindControl<TextBox>("TxtChatInput");
            var chatHistory = this.FindControl<ListBox>("LstChatHistory");

            if (chatInput == null || chatHistory == null) return;
            
            string prompt = chatInput.Text ?? "";
            if (string.IsNullOrWhiteSpace(prompt)) return;

            chatHistory.Items.Add($"👤 You: {prompt}");
            chatInput.Text = "";

            string rawLower = prompt.ToLower();
            LogAction($"[NLP SYSTEM ENGINE] Parsing input string: \"{prompt}\"");

            // --- MEMORY & CONVERSATION FLOW HANDLING ---
            if (_nlpExpectingReminderValue)
            {
                _nlpExpectingReminderValue = false;
                var task = new CybersecurityTask 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Title = _nlpPendingTitle, 
                    Description = "Generated via NLP input stream parsing.", 
                    Reminder = prompt,
                    Status = "Pending"
                };
                
                AddTaskToDatabase(task);
                LoadTasksFromDatabase(); // Keep SQLite backend completely synced
                
                chatHistory.Items.Add($"🤖 ChatBot: Task committed successfully. I've locked the tracking window alert context to: \"{prompt}\". Let me know if you need to schedule another.");
                LogAction($"[NLP EXECUTION SUCCESS] Committed SQLite record via conversational flow.");
                return;
            }

            // --- RANDOMIZED VARIATION POOLS ---
            var passwordTips = new List<string> {
                "🤖 ChatBot: Strong password structures use mixed passphrases over 12 characters. Never cross-use identical key strings.",
                "🤖 ChatBot: Security Check: Consider adopting a dedicated local password manager to isolate your master cryptographic keys.",
                "🤖 ChatBot: Tip: Replace letters with passphrase sentences! 'I Love Security!' becomes a multi-decade crack barrier."
            };

            var phishingTips = new List<string> {
                "🤖 ChatBot: Phishing relies on urgent messaging contexts to hijack systems. Always look directly at the sender header strings.",
                "🤖 ChatBot: Alert! If an email contains sub-links requesting instant credential audits, treat it as a malicious zero-day pivot.",
                "🤖 ChatBot: Countermeasure: Verify suspicious communication requests using completely independent external channels."
            };

            // --- KEYWORD & SENTIMENT PARSING ---
            if (rawLower.Contains("show activity log") || rawLower.Contains("what have you done for me"))
            {
                chatHistory.Items.Add("🤖 ChatBot: Displaying up to the last 5 operational tracking log records:");
                var lastFive = _activityLogs.Skip(Math.Max(0, _activityLogs.Count - 5)).ToList();
                int idx = 1;
                foreach (var log in lastFive) chatHistory.Items.Add($"   {idx++}. {log}");
            }
            else if (rawLower.Contains("add task") || rawLower.Contains("remind me to"))
            {
                string extractedTitle = prompt;
                if (rawLower.StartsWith("add task to ")) extractedTitle = prompt.Substring(12);
                else if (rawLower.StartsWith("add task ")) extractedTitle = prompt.Substring(9);
                else if (rawLower.StartsWith("remind me to ")) extractedTitle = prompt.Substring(13);

                _nlpPendingTitle = extractedTitle;
                _nlpExpectingReminderValue = true;

                chatHistory.Items.Add("🤖 ChatBot: Core intent captured! Where or when should I track this reminder alert timeline?");
            }
            else if (rawLower.Contains("password") || rawLower.Contains("security"))
            {
                chatHistory.Items.Add(passwordTips[_randomEngine.Next(passwordTips.Count)]);
            }
            else if (rawLower.Contains("phishing") || rawLower.Contains("email"))
            {
                chatHistory.Items.Add(phishingTips[_randomEngine.Next(phishingTips.Count)]);
            }
            else if (rawLower.Contains("quiz") || rawLower.Contains("game"))
            {
                chatHistory.Items.Add("🤖 ChatBot: Head over to the 'Cyber Quiz Game' tab at the top of your dashboard layout to launch the assessment!");
            }
            else
            {
                chatHistory.Items.Add("🤖 ChatBot: I'm tracking your conversational data nodes, but I didn't catch that specific intent. Try 'password help' or 'add task'.");
            }

            chatHistory.SelectedIndex = chatHistory.Items.Count - 1;
        }

        private void OnChatInputKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RunNLPAnalysis();
                e.Handled = true; 
            }
        }

        private void OnSendChatClicked(object? sender, RoutedEventArgs e)
        {
            RunNLPAnalysis();
        }
    }
}