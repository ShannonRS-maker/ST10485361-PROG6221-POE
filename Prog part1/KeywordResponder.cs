using System;
using System.Collections.Generic;

namespace Prog_part1
{
    public class KeywordResponder
    {
        private Dictionary<string, List<string>> responses;
        private Random random = new Random();

        public KeywordResponder()
        {
            responses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            PopulateKeywords();
        }

        private void PopulateKeywords()
        {
            // Topic 1: Passwords
            responses["password"] = new List<string> {
                "Make sure to use unique phrases for each account. Avoid reusing old credentials!",
                "Try incorporating mixed uppercase letters, numbers, and symbols to maximize security.",
                "Using a dedicated password manager is an excellent way to safely keep track of complex login keys."
            };

            // Topic 2: Phishing
            responses["phishing"] = new List<string> {
                "Always check the sender's full email address. Attackers often alter single characters to mimic real brands.",
                "Be suspicious of urgent language or odd link requests from high-profile accounts.",
                "Never log into sensitive accounts through direct email links; go to the official website instead."
            };

            // Topic 3: Privacy
            responses["privacy"] = new List<string> {
                "Review your active social media accounts and limit what information is visible to the general public.",
                "Using a reliable virtual private network (VPN) keeps your tracking data encrypted on public networks.",
                "Declining cookies on unfamiliar landing sites is a great habit to safeguard your digital footprint."
            };

            // Topic 4: Scams
            responses["scam"] = new List<string> {
                "If an online deal or cash reward feels too good to be true, it is almost certainly a trick.",
                "Verify random phone support messages directly with the company's official public helpline numbers.",
                "Never share temporary one-time pins (OTP) or authentication codes with anyone who contacts you."
            };

            // Topic 5: Malware
            responses["malware"] = new List<string> {
                "Keep your native operating system updated regularly so critical defense exploits stay patched.",
                "Avoid downloading software extensions from unverified external file mirrors.",
                "Running periodic local system anti-virus scans keeps unauthorized system loops away from your data."
            };
        }

        // Upgraded to find and combine ALL matching keywords found in the user's sentence
        public string GetResponse(string input, out string matchedKeywordsSummary)
        {
            string lowerInput = input.ToLower();
            List<string> foundKeywords = new List<string>();
            List<string> selectedTips = new List<string>();

            // Scan through all available keywords
            foreach (var key in responses.Keys)
            {
                if (lowerInput.Contains(key))
                {
                    foundKeywords.Add(key);
                    int index = random.Next(responses[key].Count);
                    
                    // Format the tip with a clean bullet point header
                    selectedTips.Add($"\n📍 Regarding [{key.ToUpper()}]: {responses[key][index]}");
                }
            }

            // If we found any matches, join them together into one seamless response
            if (foundKeywords.Count > 0)
            {
                matchedKeywordsSummary = string.Join(", ", foundKeywords);
                return string.Join("\n", selectedTips);
            }

            matchedKeywordsSummary = "";
            return "";
        }

        public List<string> GetAllKeywords()
        {
            return new List<string>(responses.Keys);
        }
    }
}