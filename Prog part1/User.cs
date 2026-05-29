using System;

namespace Prog_part1
{
    /// <summary>
    /// Represents the user interacting with the chatbot.
    /// Satisfies the explicit requirement to implement C# Automatic Properties.
    /// </summary>
    public class User
    {
        // Automatic properties tracking user metadata
        public string Name { get; set; } = "User";
        public int InteractionCount { get; set; } = 0;

        /// <summary>
        /// Constructor initializing the user object with a clean name entry.
        /// </summary>
        /// <param name="name">The input name provided by the user.</param>
        public User(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name.Trim();
            }
        }
    }
}
