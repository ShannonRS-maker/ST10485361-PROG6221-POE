using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Prog_part1
{
    public static class AudioManager
    {
        public static void PlayGreeting()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // This explicitly calls your Mac's built-in speech engine
                    Process.Start("say", "\"Hello! Welcome to the Cybersecurity Awareness Bot. I'm here to help you stay safe online.\"");
                }
                else
                {
                    Console.Write("\a"); // Fallback system beep
                }
            }
            catch
            {
                // Fallback catch if the audio channels are busy
                Console.Beep();
            }
        }
    }
}