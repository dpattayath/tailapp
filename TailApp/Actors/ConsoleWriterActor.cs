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
            Receive<NoActivityWarning>(message => _lastMessageRecevied.AddMinutes(2) < DateTime.Now, message =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
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
            });
            ReceiveAny(message =>
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(message);
                Console.ResetColor();
            });
        }
    }
}