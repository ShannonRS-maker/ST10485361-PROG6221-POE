using System;

namespace Prog_part1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Clear();

            // 1. Render your awesome ASCII Art Header directly to the terminal
            Console.WriteLine("=======================================================================");
            Console.WriteLine("   ______              __                 ____            __     ");
            Console.WriteLine("  / ____/_  __  ______/ /__  __________  / __ )____  ____/ /_    ");
            Console.WriteLine(" / /   / / / / / __  / _  / / ___/ ___/ / __  / __ \\/ __  /(_)   ");
            Console.WriteLine("/ /___/ /_/ / / /_/ /  __/ / /  (__  ) / /_/ / /_/ / /_/ / _     ");
            Console.WriteLine("\\____/\\__, /  \\__,_/\\___/_/ /  /____/ /_____/\\____/\\__,_/(_)     ");
            Console.WriteLine("     /____/                                                      ");
            Console.WriteLine("=======================================================================");
            Console.WriteLine("[SYSTEM] Cybersecurity Core Initialized. Standing by for queries...\n");

            // 2. Instantiate your actual backend logic classes
            ChatBot chatBot = new ChatBot();
            
            try 
            {
                AudioManager.PlayGreeting();
            }
            catch { /* Fallback if audio driver isn't loaded on terminal */ }

            // Display the bot's initial greeting text
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"CORE > {chatBot.GetGreeting()}\n");

            // 3. Keep the conversation looping until you type exit
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("USER > ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.ToLower() == "exit" || input.ToLower() == "quit") break;

                // Process input through your correct brain logic method
                Console.ForegroundColor = ConsoleColor.Green;
                
                // FIXED: Direct call to ProcessInput so your name state registers perfectly!
                string response = chatBot.ProcessInput(input);

                Console.WriteLine($"{response}\n");
            }
        }
    }
}