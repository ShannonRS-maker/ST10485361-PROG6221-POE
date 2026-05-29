using System;

namespace Prog_part1
{
    public class MemoryStore
    {
        public string UserName { get; set; } = "";
        public string FavouriteTopic { get; set; } = "";

        // Store data dynamically by a key look-up string
        public void Store(string key, string value)
        {
            if (key.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                UserName = value;
            }
            else if (key.Equals("Topic", StringComparison.OrdinalIgnoreCase))
            {
                FavouriteTopic = value;
            }
        }

        // Retrieve data dynamically by a key look-up string
        public string Recall(string key)
        {
            if (key.Equals("Name", StringComparison.OrdinalIgnoreCase)) return UserName;
            if (key.Equals("Topic", StringComparison.OrdinalIgnoreCase)) return FavouriteTopic;
            return "";
        }

        // Prepend context strings based on what the user liked talking about earlier
        public string GetPersonalisedOpener()
        {
            if (!string.IsNullOrEmpty(FavouriteTopic))
            {
                return $"As someone interested in {FavouriteTopic}, you should know that ";
            }
            return "";
        }
    }
}