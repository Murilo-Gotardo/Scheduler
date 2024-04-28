using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Util
{
    public class ConsoleLogger : IConsoleLogger
    {
        private string? CallingClass { get; set; } 

        public void LogInfo(string message)
        {
            CallingClass = new StackFrame(1).GetMethod().DeclaringType.Name;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteMessage(message, CallingClass);
        }

        public void LogMetrics(string message)
        {
            CallingClass = new StackFrame(1).GetMethod().DeclaringType.Name;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            WriteMessage(message, CallingClass);
        }

        public void LogStatistics(string message) 
        {
            CallingClass = new StackFrame(1).GetMethod().DeclaringType.Name;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            WriteMessage(message, CallingClass);
        }

        private static void WriteMessage(string message, string callingClass)
        {
            Console.WriteLine(callingClass + " || " + message);
        }
    }
}
