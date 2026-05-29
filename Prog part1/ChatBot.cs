using System;
using System.Collections.Generic;
using System.Linq;

namespace Prog_part1
{
    public class ChatBot
    {
        private KeywordResponder keywords;
        private SentimentDetector sentiment;
        private MemoryStore memory;
        private bool awaitingName = true;
        private string lastTopic = "";
        private List<string> conversationLog = new List<string>(); // Tracks session history length

        public ChatBot()
        {
            keywords = new KeywordResponder();
            sentiment = new SentimentDetector();
            memory = new MemoryStore();
        }

        public string GetGreeting()
        {
            return "🤖 Bot: Welcome user! Please type your name to begin account initialization.";
        }

        public string ProcessInput(string input)
        {
            input = input.Trim();

            // --- FEATURE 2: INPUT VALIDATION DEFENDER (Protects against error crashes) ---
            if (string.IsNullOrEmpty(input))
            {
                return "🤖 Bot: ⚠️ Empty input transmission detected. Please provide a clear query string.";
            }
            if (input.Length > 1 && input.Distinct().Count() == 1)
            {
                return "🤖 Bot: ⚠️ Repetitive character spam detected. Please enter a valid conversational phrase.";
            }

            // Log user message to session history
            conversationLog.Add($"User: {input}");

            // 1. Capture user name and present dynamic menu
            if (awaitingName)
            {
                // Basic validation for name
                if (input.Any(char.IsDigit))
                {
                    return "🤖 Bot: ⚠️ Invalid name structure. Please use alphabetic characters for account identity setup.";
                }

                // FIX: Store the name explicitly in your memory manager
                memory.Store("Name", input);
                awaitingName = false;

                // Grab the name safely back out of the memory store variable
                string savedName = !string.IsNullOrEmpty(memory.UserName) ? memory.UserName : input;

                List<string> topics = keywords.GetAllKeywords();
                string topicList = string.Join(", ", topics);

                string welcomeMessage = $"🤖 Bot: Hello {savedName}! Account linked successfully.\n" +
                       $"       How can I protect your network system workspace today?\n\n" +
                       $"       📋 AVAILABLE CYBERSECURITY MODULES TO DISCUSS:\n" +
                       $"       ------------------------------------------------\n" +
                       $"       👉 {topicList}\n" +
                       $"       ------------------------------------------------\n" +
                       $"       💡 SPECIAL UTILITY UTILS COMMANDS AVAILABLE:\n" +
                       $"       Type '/history' to see logs | Type '/modules' to see topics \n" +
                       $"       Type 'tell me more' to expand on last topic discussed.\n" +
                       $"       Please note to type 'exit' to safely disconnect from the terminal.";
                
                conversationLog.Add(welcomeMessage);
                return welcomeMessage;
            }

            // --- FEATURE 1: ADVANCED COMMAND ACTIONS (Conversation Flow Expansion) ---
            if (input.Equals("/history", StringComparison.OrdinalIgnoreCase))
            {
                return $"🤖 Bot: 📦 Current Active Session Log Info ({conversationLog.Count} turns executed):\n" + 
                       string.Join("\n", conversationLog.TakeLast(6));
            }
            if (input.Equals("/modules", StringComparison.OrdinalIgnoreCase))
            {
                return $"🤖 Bot: Current secure modules open for analysis: {string.Join(", ", keywords.GetAllKeywords())}.";
            }

            // 2. Check for conversation follow-up flow phrases
            if (input.Equals("tell me more", StringComparison.OrdinalIgnoreCase) || 
                input.Equals("explain more", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    memory.Store("Topic", lastTopic);
                    string followUpResponse = $"🤖 Bot: Expanding on '{lastTopic}'... {keywords.GetResponse(lastTopic, out _)}";
                    conversationLog.Add(followUpResponse);
                    return followUpResponse;
                }
                return "🤖 Bot: We haven't started a specific security topic yet! Try asking me about 'phishing' or 'malware' first.";
            }

            // 3 & 4. Run Sentiment Detection & Keyword Recognition
            Sentiment detectedSentiment = sentiment.Detect(input);
            string sentimentOpener = sentiment.GetSentimentResponse(detectedSentiment);

            string riskMeter = "";
            if (detectedSentiment == Sentiment.Worried || detectedSentiment == Sentiment.Frustrated)
            {
                riskMeter = "\n⚠️  [SYSTEM STATUS: ANXIETY ALERT DETECTED - DE-ESCALATION PROTOCOL ACTIVE]\n";
            }
            else if (detectedSentiment == Sentiment.Happy)
            {
                riskMeter = "\n🛡️  [SYSTEM STATUS: SECURE OPERATIONS / POSITIVE USER COGNITION]\n";
            }

            string matchedKeyword;
            string keywordTip = keywords.GetResponse(input, out matchedKeyword);

            if (!string.IsNullOrEmpty(keywordTip))
            {
                // Capture the first matched keyword in a compound string for followups
                lastTopic = matchedKeyword.Split(',')[0].Trim();
                string memoryPrefix = memory.GetPersonalisedOpener();

                string combinedResponse;
                if (!string.IsNullOrEmpty(memory.FavouriteTopic) && matchedKeyword.Contains(memory.FavouriteTopic))
                {
                    combinedResponse = $"🤖 Bot: {riskMeter}{sentimentOpener}{memoryPrefix}{keywordTip}";
                }
                else
                {
                    combinedResponse = $"🤖 Bot: {riskMeter}{sentimentOpener}{keywordTip}";
                }

                conversationLog.Add(combinedResponse);
                return combinedResponse;
            }

            // 5. Handle special custom general phrases
            string lowerInput = input.ToLower();
            if (lowerInput.Contains("how are you"))
            {
                string r = "🤖 Bot: I am running at optimal efficiency with all firewall protocols secure! How can I help you stay safe online?";
                conversationLog.Add(r);
                return r;
            }
            if (lowerInput.Contains("purpose") || lowerInput.Contains("what can you do"))
            {
                string r = $"🤖 Bot: My purpose is to assist you with cybersecurity awareness. You can analyze: {string.Join(", ", keywords.GetAllKeywords())}!";
                conversationLog.Add(r);
                return r;
            }

            // 6. Fall through to a random fallback phrase if nothing matches
            Random rand = new Random();
            string[] fallbacks = new string[]
            {
                "🤖 Bot: I'm not sure I quite understand that sentence pattern. Can you try rephrasing it using keywords like 'password', 'scam', or 'privacy'?",
                "🤖 Bot: That input does not match my security dictionary definitions. Try asking about 'malware' or 'phishing' guidelines.",
                "🤖 Bot: I'm still learning! To get the best guidance, use clear terms like 'privacy' or 'password'."
            };

            string pickedFallback = fallbacks[rand.Next(fallbacks.Length)];
            conversationLog.Add(pickedFallback);
            return pickedFallback;
        }
    }
}