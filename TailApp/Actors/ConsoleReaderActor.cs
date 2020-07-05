using System;
using Akka.Actor;
using TailApp.Messages;

namespace TailApp.Actors
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Terminate"/>.
    /// </summary>
    public class ConsoleReaderActor : ReceiveActor
    {
        private const string ExitCommand = "exit";

        public ConsoleReaderActor()
        {
            Receive<StartProcessing>(msg =>
            {
                DoPrintInstructions();
                Self.Tell(new ContinueProcessing());
            });
            Receive<EndProcessing>(msg =>
            {
                Context.System.Terminate();
            });
            Receive<ContinueProcessing>(msg => 
            {
                Handler();
            });
        }

        private void Handler()
        {
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && string.Equals(input, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                Self.Tell(new EndProcessing());
                return;
            }
            Context.ActorSelection("akka://tailAppActorSystem/user/fileValidationActor").Tell(input);
        }

        private void DoPrintInstructions()
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        }
    }
}