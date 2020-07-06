using System;
using Akka.Actor;
using TailApp.Messages;

namespace TailApp.Actors
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// </summary>
    public class ConsoleWriterActor : ReceiveActor
    {
        private DateTime _lastMessageRecevied;
        
        private const ushort WarningTimespanMinutes = 1;

        public ConsoleWriterActor()
        {
            Receive<InputError>(message =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message.Reason);
                Console.ResetColor();
            });
            Receive<InputSuccess>(message =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message.Reason);
                Console.ResetColor();
            });
            Receive<FileChanged>(message =>
            {
                _lastMessageRecevied = DateTime.Now;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(message.FilePath);
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(message.Content);
                Console.ResetColor();
                
                // after initial file change, set a timeout to enable a warning
                Context.SetReceiveTimeout(TimeSpan.FromMinutes(WarningTimespanMinutes));
            });
            // handle timeout and issue a warning
            Receive<ReceiveTimeout>(timeout =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No file activity for last {WarningTimespanMinutes} minutes, type 'exit' to finish watching or 'ignore' to dismiss the warning.");
                Console.ResetColor();
            });
            Receive<IgnoreWarning>(msg =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"disabled no activity warnings");
                Console.ResetColor();
                Context.SetReceiveTimeout(null);
            });
            
            // catch all
            ReceiveAny(message =>
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(message);
                Console.ResetColor();
            });
        }
    }
}