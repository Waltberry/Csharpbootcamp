// ===============================
// Imports (Libraries)
// ===============================

using System;      // Console, Exception, StringComparison, etc.
using System.IO;   // File, FileInfo, StreamReader, Path
using System.Linq; // Skip(), ToArray() for working with args


// ===============================
// Namespace (logical grouping)
// ===============================
namespace FileReaderApp
{
    internal class Program
    {
        // ==================================================
        // ENTRY POINT (Main)
        // ==================================================
        // This app is a CLI tool, so it reads input from "args"
        // (tokens typed after the executable name).
        //
        // Returning int = exit code:
        //   0 = success
        //   1 = unexpected/internal error
        //   2 = user/usage error (wrong command/args/path)
        // ==================================================
        static int Main(string[] args)
        {
            // --------------------------------------------------
            // Expected CLI patterns:
            //   filereader read  <path>
            //   filereader stats <path>
            //   filereader head  <path> <n>
            //   filereader --help
            // --------------------------------------------------

            // If no arguments were provided OR user asked for help,
            // print help text and exit successfully.
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintHelp();
                return 0;
            }

            // --------------------------------------------------
            // COMMAND PARSING
            // --------------------------------------------------

            // First token is the command.
            // Normalize (trim + lowercase) so user can type:
            //   READ / Read / read
            string command = args[0].Trim().ToLowerInvariant();

            // Everything after the first token is the command parameters (tail).
            // Example:
            //   args = ["head", "data.txt", "5"]
            //   tail = ["data.txt", "5"]
            string[] tail = args.Skip(1).ToArray();

            // --------------------------------------------------
            // ERROR HANDLING STRATEGY
            // --------------------------------------------------
            // Wrap the command execution in try/catch so the app
            // does not crash with an unhandled exception.
            // Instead, we print a clean message and return an error code.
            try
            {
                // --------------------------------------------------
                // COMMAND ROUTING (Dispatcher)
                // --------------------------------------------------
                switch (command)
                {
                    case "read":
                        // Print entire file
                        return CmdRead(tail);

                    case "stats":
                        // Print bytes, lines, and word count
                        return CmdStats(tail);

                    case "head":
                        // Print first N lines
                        return CmdHead(tail);

                    default:
                        // Unknown command: user error
                        Console.WriteLine($"Unknown command: {command}");
                        Console.WriteLine("Run with --help to see available commands.");
                        return 2;
                }
            }
            catch (Exception ex)
            {
                // Catch-all so the app never “crashes ugly”.
                // In a real production CLI tool you might:
                // - log a full stack trace
                // - return different codes for different failures
                Console.WriteLine("Unexpected error:");
                Console.WriteLine(ex.Message);
                return 1;
            }
        }


        // ==================================================
        // COMMANDS
        // Each command:
        //  1) validates its args
        //  2) validates file existence
        //  3) does the work
        //  4) returns an exit code
        // ==================================================

        static int CmdRead(string[] tail)
        {
            // --------------------------------------------------
            // read <path>
            // Prints the entire file line-by-line.
            // --------------------------------------------------

            // Validate argument count
            if (tail.Length != 1)
            {
                Console.WriteLine("Usage: read <path>");
                return 2;
            }

            // Convert relative path to absolute path for clearer messages
            string path = ResolvePath(tail[0]);

            // Check file exists before opening it
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return 2;
            }

            // StreamReader reads text files efficiently (streaming).
            // "using var" ensures the file handle is always closed,
            // even if something throws an exception later.
            using var reader = new StreamReader(path);

            // Read the file line-by-line until ReadLine returns null (EOF)
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }

            return 0; // success
        }

        static int CmdStats(string[] tail)
        {
            // --------------------------------------------------
            // stats <path>
            // Prints:
            //  - Bytes (file size)
            //  - Lines
            //  - Words (simple whitespace-based counter)
            // --------------------------------------------------

            // Validate argument count
            if (tail.Length != 1)
            {
                Console.WriteLine("Usage: stats <path>");
                return 2;
            }

            // Normalize/resolve path
            string path = ResolvePath(tail[0]);

            // Ensure file exists
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return 2;
            }

            // FileInfo gives metadata about the file (size, timestamps, etc.)
            long bytes = new FileInfo(path).Length;

            // Counters
            int lineCount = 0;
            int wordCount = 0;

            // Stream through file line-by-line to avoid loading whole file in memory
            using var reader = new StreamReader(path);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                // Count lines
                lineCount++;

                // Split on whitespace:
                // Passing null char[] means "split on whitespace"
                // RemoveEmptyEntries prevents counting multiple spaces as empty “words”
                var words = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                // Add word count for this line
                wordCount += words.Length;
            }

            // Print stats
            Console.WriteLine($"Path:  {path}");
            Console.WriteLine($"Bytes: {bytes}");
            Console.WriteLine($"Lines: {lineCount}");
            Console.WriteLine($"Words: {wordCount}");

            return 0; // success
        }

        static int CmdHead(string[] tail)
        {
            // --------------------------------------------------
            // head <path> <n>
            // Prints the first N lines of the file.
            // Similar to Unix "head".
            // --------------------------------------------------

            // Validate argument count
            if (tail.Length != 2)
            {
                Console.WriteLine("Usage: head <path> <n>");
                return 2;
            }

            // Normalize path
            string path = ResolvePath(tail[0]);

            // Ensure file exists
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return 2;
            }

            // Parse N and validate that it is >= 1
            if (!int.TryParse(tail[1], out int n) || n < 1)
            {
                Console.WriteLine("Error: <n> must be an integer >= 1");
                return 2;
            }

            // Open the file for streaming reads
            using var reader = new StreamReader(path);

            // Read at most N lines (or fewer if file ends early)
            for (int i = 0; i < n; i++)
            {
                string? line = reader.ReadLine();

                // If we've hit EOF, stop early
                if (line == null)
                    break;

                Console.WriteLine(line);
            }

            return 0; // success
        }


        // ==================================================
        // HELPERS
        // ==================================================

        static bool IsHelp(string token)
        {
            // Recognize common help flags across CLI tools
            return token.Equals("--help", StringComparison.OrdinalIgnoreCase)
                || token.Equals("-h", StringComparison.OrdinalIgnoreCase)
                || token.Equals("/?", StringComparison.OrdinalIgnoreCase);
        }

        static void PrintHelp()
        {
            // Short, clear help output (best practice for CLI UX)
            Console.WriteLine("FileReaderApp");
            Console.WriteLine("Usage:");
            Console.WriteLine("  filereader read <path>        Print entire file");
            Console.WriteLine("  filereader stats <path>       Print bytes, lines, words");
            Console.WriteLine("  filereader head <path> <n>    Print first n lines");
            Console.WriteLine("  filereader --help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine(@"  filereader read .\data.txt");
            Console.WriteLine(@"  filereader stats C:\temp\log.txt");
            Console.WriteLine(@"  filereader head .\data.txt 5");
        }

        static string ResolvePath(string inputPath)
        {
            // Users often pass relative paths. Convert to absolute so messages are clear.
            // If input is already absolute, GetFullPath returns it unchanged.
            return Path.GetFullPath(inputPath);
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
// If args empty OR args[0] is help flag
//   → PrintHelp()
//   → Exit code 0
//   ↓
// command = args[0].ToLowerInvariant()
// tail = args[1..end]
//   ↓
// try
//   switch(command)
//     ├─ "read"  → CmdRead(tail)
//     ├─ "stats" → CmdStats(tail)
//     ├─ "head"  → CmdHead(tail)
//     └─ default → print "Unknown command" → Exit code 2
// catch (Exception)
//   → print "Unexpected error" → Exit code 1
//   ↓
// Return exit code to OS
//
