// ===============================
// Import required system libraries
// ===============================

// Provides basic system features (Console, String, Double, etc.)
using System;

// Provides culture-specific number parsing (e.g., 3.14 vs 3,14)
using System.Globalization;

// (Not used yet, but supports async programming if needed later)
using System.Threading.Tasks;


// ===============================
// Application Namespace
// ===============================
// Acts like a folder that groups related classes
namespace CalculatorApp
{
    // Main program class
    internal class Program
    {
        // ==================================================
        // ENTRY POINT: Program starts execution here
        // ==================================================
        static void Main(string[] args)
        {
            // Set the title of the console window
            Console.Title = "CalculatorApp";

            // Display application header
            Console.WriteLine("=== CalculatorApp ===");
            Console.WriteLine("Operations: +  -  *  /");
            Console.WriteLine("Type 'q' at any prompt to quit.\n");

            // ==================================================
            // Main application loop
            // Runs until user chooses to quit
            // ==================================================
            while (true)
            {
                // -------------------------------
                // Read first number
                // -------------------------------
                double a;

                // Try to read a valid double from user
                // If user quits, break out of loop
                if (!TryReadDouble("Enter first number: ", out a))
                    break;


                // -------------------------------
                // Read operator
                // -------------------------------
                char op;

                // Try to read valid operator (+ - * /)
                if (!TryReadOperator("Enter operator (+, -, *, /): ", out op))
                    break;


                // -------------------------------
                // Read second number
                // -------------------------------
                double b;

                // Try to read second number
                if (!TryReadDouble("Enter second number: ", out b))
                    break;


                // -------------------------------
                // Perform calculation
                // -------------------------------

                // Try to calculate result safely
                // result  -> output value
                // error   -> error message if failed
                if (!TryCalculate(a, b, op, out double result, out string error))
                {
                    // Display error message if calculation fails
                    Console.WriteLine($"Error: {error}\n");

                    // Restart loop
                    continue;
                }


                // -------------------------------
                // Display result
                // -------------------------------

                Console.WriteLine($"Result: {a} {op} {b} = {result}\n");
            }

            // Display goodbye message when loop ends
            Console.WriteLine("\nGoodbye!");
        }


        // ==================================================
        // Reads a valid double value from user
        // Keeps asking until valid input or quit
        // ==================================================
        static bool TryReadDouble(string prompt, out double value)
        {
            // Initialize output variable
            value = 0;

            // Keep asking until valid input
            while (true)
            {
                // Display prompt
                Console.Write(prompt);

                // Read input from user
                string? input = Console.ReadLine();

                // If input stream closed, exit
                if (input is null)
                    return false;

                // Remove leading/trailing spaces
                input = input.Trim();

                // If user wants to quit
                if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
                    return false;


                // ---------------------------------------
                // Try parsing number using different cultures
                // ---------------------------------------

                // Try using user's local culture
                // OR using international standard format
                if (double.TryParse(
                        input,
                        NumberStyles.Float,
                        CultureInfo.CurrentCulture,
                        out value)

                    ||

                    double.TryParse(
                        input,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out value))
                {
                    // Parsing successful
                    return true;
                }


                // If parsing failed, show error and retry
                Console.WriteLine("Invalid number. Try again (or type 'q' to quit).");
            }
        }


        // ==================================================
        // Reads a valid operator from user
        // Accepts only: + - * /
        // ==================================================
        static bool TryReadOperator(string prompt, out char op)
        {
            // Initialize operator with null character
            op = '\0';

            // Keep asking until valid input
            while (true)
            {
                // Display prompt
                Console.Write(prompt);

                // Read input
                string? input = Console.ReadLine();

                // If input stream closed, exit
                if (input is null)
                    return false;

                // Remove spaces
                input = input.Trim();

                // Quit if user types 'q'
                if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
                    return false;


                // ---------------------------------------
                // Validate operator
                // ---------------------------------------

                // Must be exactly one character
                // AND must be one of + - * /
                if (input.Length == 1 && "+-*/".Contains(input[0]))
                {
                    // Store valid operator
                    op = input[0];

                    return true;
                }


                // Invalid operator message
                Console.WriteLine("Invalid operator. Use +, -, *, / (or type 'q' to quit).");
            }
        }


        // ==================================================
        // Performs calculation safely
        // Returns true if successful, false if error
        // ==================================================
        static bool TryCalculate(
            double a,          // First number
            double b,          // Second number
            char op,           // Operator
            out double result, // Computed result
            out string error   // Error message (if any)
        )
        {
            // Initialize outputs
            result = 0;
            error = "";


            // ---------------------------------------
            // Select operation based on operator
            // ---------------------------------------
            switch (op)
            {
                // Addition
                case '+':
                    result = a + b;
                    return true;

                // Subtraction
                case '-':
                    result = a - b;
                    return true;

                // Multiplication
                case '*':
                    result = a * b;
                    return true;

                // Division
                case '/':

                    // Prevent division by zero
                    if (b == 0)
                    {
                        error = "Division by zero is not allowed.";
                        return false;
                    }

                    result = a / b;
                    return true;

                // Unknown operator (safety fallback)
                default:
                    error = "Unknown operator.";
                    return false;
            }
        }
    }
}


// ==================================================
// PROGRAM FLOW (High-Level View)
// ==================================================
//
// Start
//   ↓
// Display menu
//   ↓
// Loop:
//    → Read number A
//    → Read operator
//    → Read number B
//    → Calculate
//    → Display result
// Repeat
//   ↓
// User enters 'q'
//   ↓
// Exit program
//
