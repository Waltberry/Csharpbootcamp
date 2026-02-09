// ===============================
// Imports (Libraries)
// ===============================

using System;              // Console, DateTime, Array, StringComparison, etc.
using System.Globalization; // Culture-aware number parsing (3.14 vs 3,14)


// ===============================
// Namespace (logical grouping)
// ===============================
namespace SimpleCliApp
{
    internal class Program
    {
        // ==================================================
        // ENTRY POINT (Main)
        // ==================================================
        // NOTE: This version uses "static int Main" so we can
        // return an exit code to the operating system:
        //   0 = success
        //   non-zero = failure (common convention in CLI tools)
        // ==================================================
        static int Main(string[] args)
        {
            // --------------------------------------------------
            // args = tokens typed after executable name.
            // Example:
            //   simplecli greet Walter
            // args = ["greet", "Walter"]
            // --------------------------------------------------

            // If user provided no args OR asked for help, print help and exit successfully.
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintHelp();
                return 0;
            }

            // --------------------------------------------------
            // COMMAND PARSING
            // --------------------------------------------------

            // The first token is the command.
            // Normalize to lowercase so user can type: Greet / greet / GREET.
            string command = args[0].Trim().ToLowerInvariant();

            // Everything after the command is the command "tail" (parameters).
            // Example:
            //   args = ["add", "2", "3"]
            //   tail = ["2", "3"]
            string[] tail = Slice(args, startIndex: 1);

            // --------------------------------------------------
            // COMMAND ROUTING
            // --------------------------------------------------
            // switch routes the command to the correct function.
            // Each command function returns an exit code.
            switch (command)
            {
                case "greet":
                    // Usage: simplecli greet <name>
                    return CmdGreet(tail);

                case "add":
                    // Usage: simplecli add <a> <b>
                    return CmdAdd(tail);

                case "now":
                    // Usage: simplecli now
                    return CmdNow(tail);

                default:
                    // Unknown command => show message + hint, exit with error code.
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Run with --help to see available commands.");
                    return 2; // Non-zero means "error" in CLI conventions
            }
        }


        // ==================================================
        // COMMANDS
        // Each command is a small function that:
        // 1) validates its arguments
        // 2) performs the work
        // 3) returns an exit code
        // ==================================================

        static int CmdGreet(string[] tail)
        {
            // --------------------------------------------------
            // greet <name>
            // --------------------------------------------------

            // Validate argument count: greet expects EXACTLY 1 parameter.
            if (tail.Length != 1)
            {
                Console.WriteLine("Usage: greet <name>");
                return 2;
            }

            // Parameter: name
            string name = tail[0];

            // Print greeting
            Console.WriteLine($"Hello, {name}!");

            // Success
            return 0;
        }

        static int CmdAdd(string[] tail)
        {
            // --------------------------------------------------
            // add <a> <b>
            // --------------------------------------------------

            // Validate argument count: add expects EXACTLY 2 parameters.
            if (tail.Length != 2)
            {
                Console.WriteLine("Usage: add <a> <b>");
                return 2;
            }

            // Try parsing both numbers.
            // If parsing fails, show error and usage example.
            if (!TryParseDouble(tail[0], out double a) || !TryParseDouble(tail[1], out double b))
            {
                Console.WriteLine("Error: add expects two numbers.");
                Console.WriteLine("Example: add 2 3");
                return 2;
            }

            // Compute sum
            double sum = a + b;

            // Print output (for CLI tools, printing just the result is common)
            Console.WriteLine(sum);

            // Success
            return 0;
        }

        static int CmdNow(string[] tail)
        {
            // --------------------------------------------------
            // now
            // --------------------------------------------------

            // Validate argument count: now expects NO parameters.
            if (tail.Length != 0)
            {
                Console.WriteLine("Usage: now");
                return 2;
            }

            // Print current local time in a stable format:
            // yyyy-MM-dd HH:mm:ss
            // Example: 2026-02-05 15:20:13
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Success
            return 0;
        }


        // ==================================================
        // HELPERS
        // Small utility functions to keep Main() clean
        // ==================================================

        static bool IsHelp(string token)
        {
            // --------------------------------------------------
            // Common help flags in CLI tools:
            // --help  (GNU style)
            // -h      (short form)
            // /?      (Windows style)
            // --------------------------------------------------
            return token.Equals("--help", StringComparison.OrdinalIgnoreCase)
                || token.Equals("-h", StringComparison.OrdinalIgnoreCase)
                || token.Equals("/?", StringComparison.OrdinalIgnoreCase);
        }

        static void PrintHelp()
        {
            // --------------------------------------------------
            // Help text should be short, consistent, and example-driven.
            // --------------------------------------------------
            Console.WriteLine("SimpleCliApp");
            Console.WriteLine("Usage:");
            Console.WriteLine("  simplecli greet <name>");
            Console.WriteLine("  simplecli add <a> <b>");
            Console.WriteLine("  simplecli now");
            Console.WriteLine("  simplecli --help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  simplecli greet Walter");
            Console.WriteLine("  simplecli add 2 3");
            Console.WriteLine("  simplecli now");
        }

        static string[] Slice(string[] source, int startIndex)
        {
            // --------------------------------------------------
            // Returns a new array containing source[startIndex..end]
            //
            // Example:
            // source = ["add","2","3"], startIndex=1 => ["2","3"]
            // --------------------------------------------------

            // If startIndex is out of range, return an empty array safely.
            if (startIndex >= source.Length)
                return Array.Empty<string>();

            // Calculate how many items we want to copy
            int length = source.Length - startIndex;

            // Create destination array
            string[] result = new string[length];

            // Copy the tail portion
            Array.Copy(source, startIndex, result, 0, length);

            return result;
        }

        static bool TryParseDouble(string s, out double value)
        {
            // --------------------------------------------------
            // Tries to parse a number in a user-friendly way:
            // - CultureInfo.CurrentCulture supports local decimal separators (e.g., 3,14)
            // - CultureInfo.InvariantCulture supports dot decimal (e.g., 3.14)
            // --------------------------------------------------
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}


// ==================================================
// PROGRAM FLOW (High-Level View)
// ==================================================
//
// Start
//   ↓
// Read command-line args (args[])
//   ↓
// If no args OR first arg is help flag (--help / -h / /?)
//   → PrintHelp()
//   → Exit code 0
//   ↓
// command = args[0] (lowercased)
// tail = args[1..end]
//   ↓
// switch(command)
//   ├─ "greet" → CmdGreet(tail)
//   ├─ "add"   → CmdAdd(tail)
//   ├─ "now"   → CmdNow(tail)
//   └─ default → print "Unknown command" + hint → exit code 2
//   ↓
// Return exit code to OS
//
