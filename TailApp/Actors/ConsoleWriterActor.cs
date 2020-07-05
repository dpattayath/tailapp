using System;
using Akka.Actor;
using TailApp.Messages;

namespace TailApp.Actors
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    public class ConsoleWriterActor : UntypedActor
    {
        private DateTime _lastMessageRecevied;

        protected override void OnReceive(object message)
        {
            if (message is InputError)
            {
                var msg = (InputError) message;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg.Reason);
            }
            else if (message is InputSuccess)
            {
                var msg = (InputSuccess) message;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(msg.Reason);
            }
            else if (message is NoActivity)
            {
                if (_lastMessageRecevied.AddMinutes(2) < DateTime.Now)
                {
                    var msg = (NoActivity) message;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(msg.Reason);
                }
            }
            else
            {
                _lastMessageRecevied = DateTime.Now;
                Console.WriteLine(message);
            }
            Console.ResetColor();
        }
    }
}