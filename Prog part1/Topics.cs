using System;

namespace Prog_part1
{
    /// <summary>
    /// Provides standardized output text blocks detailing structural cybersecurity concepts.
    /// </summary>
    static class Topics
    {
        public static void LearnAboutPasswords()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n--- 🔐 Topic: Password Safety ---");
            Console.WriteLine("- Use long passphrases (12+ characters) combining words, numbers, and symbols.");
            Console.WriteLine("- Avoid reusing identical passwords across multiple online applications or systems.");
            Console.WriteLine("- Use a reputable password manager to generate and securely encrypt unique keys.");
            Console.WriteLine("- Enable multi-factor authentication (MFA) to form defensive perimeters.");
            Console.ResetColor();
        }

        public static void LearnAboutPhishing()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n--- 🎣 Topic: Phishing Awareness ---");
            Console.WriteLine("- Phishing is a social engineering attack tricking you into yielding critical data access.");
            Console.WriteLine("- Thoroughly inspect sender addresses and look for grammatical discrepancies.");
            Console.WriteLine("- Hover your mouse pointer over hyperlinks to preview their actual destination routing.");
            Console.WriteLine("- Never type sensitive profile data into windows reached via unverified embedded web links.");
            Console.ResetColor();
        }

        public static void LearnAboutSafeBrowsing()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n--- 🌐 Topic: Safe Browsing Guidelines ---");
            Console.WriteLine("- Validate that website domains run over HTTPS encryption protocols before submitting logs.");
            Console.WriteLine("- Keep your operating system web engines and local browsers fully updated to block exploits.");
            Console.WriteLine("- Do not download software setups from unverified third-party mirroring networks.");
            Console.WriteLine("- Utilize defensive plugins such as script blockers to prevent background execution payloads.");
            Console.ResetColor();
        }
    }
}