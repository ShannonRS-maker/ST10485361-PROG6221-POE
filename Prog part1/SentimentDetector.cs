using System;
using System.Collections.Generic;

namespace Prog_part1
{
    // The exact enum categories required by the brief
    public enum Sentiment { Neutral, Worried, Curious, Frustrated, Happy }

    public class SentimentDetector
    {
        private Dictionary<Sentiment, List<string>> sentimentTriggers;

        public SentimentDetector()
        {
            sentimentTriggers = new Dictionary<Sentiment, List<string>>();
            InitializeTriggers();
        }

        private void InitializeTriggers()
        {
            // Populating emotional trigger keywords from the assignment guide
            sentimentTriggers[Sentiment.Worried] = new List<string> { "worried", "scared", "afraid", "anxious", "nervous", "unsafe" };
            sentimentTriggers[Sentiment.Curious] = new List<string> { "curious", "wondering", "interested", "want to know", "how does" };
            sentimentTriggers[Sentiment.Frustrated] = new List<string> { "frustrated", "annoyed", "confused", "don't understand" };
            sentimentTriggers[Sentiment.Happy] = new List<string> { "great", "thanks", "helpful", "awesome", "love it" };
        }

        // Analyzes the input sentence to pick out matching emotional keywords
        public Sentiment Detect(string input)
        {
            string lowerInput = input.ToLower();
            foreach (var pair in sentimentTriggers)
            {
                foreach (string trigger in pair.Value)
                {
                    if (lowerInput.Contains(trigger))
                    {
                        return pair.Key;
                    }
                }
            }
            return Sentiment.Neutral;
        }

        // Returns an empathetic prefix sentence tailored to their specific mood
        public string GetSentimentResponse(Sentiment s)
        {
            return s switch
            {
                Sentiment.Worried => "It's completely understandable to feel anxious about security. Let's make sure you're protected: ",
                Sentiment.Curious => "I love that you want to learn more! Here is how it works: ",
                Sentiment.Frustrated => "Cybersecurity can be completely overwhelming, but we can break it down together: ",
                Sentiment.Happy => "Fantastic! Staying positive is a great way to build secure habits. Keep this in mind: ",
                _ => "" // Neutral returns an empty string as requested by the brief
            };
        }
    }
}
