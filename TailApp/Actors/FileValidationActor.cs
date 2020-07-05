using System;
using System.IO;
using Akka.Actor;
using TailApp.Messages;

namespace TailApp.Actors
{
    /// <summary>
    /// Actor that validates user input and signals result to others
    /// </summary>
    public class FileValidationActor: UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        private ICancelable _cancellation;
        
        private const ushort WarningTimespanMinutes = 2;

        public FileValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }
        
        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new NullInputError("Input was blank. Please try again."));
            }
            else if (string.Equals(msg, "ignore", StringComparison.OrdinalIgnoreCase))
            {
                _cancellation.Cancel();
                _consoleWriterActor.Tell(new InputSuccess("disabled no activity warnings"));
            }
            else
            {
                bool valid = IsFileUri(msg);
                if (valid)
                {
                    // send success to console writer
                    _consoleWriterActor.Tell(new InputSuccess(string.Format("Starting processing for {0}", msg)));

                    // start coordinator
                    Context.ActorSelection("akka://tailAppActorSystem/user/tailCoordinatorActor")
                        .Tell(new StartTail(msg, _consoleWriterActor));

                    // schedule to check no activity every 2 minutes
                    _cancellation = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                        TimeSpan.FromMinutes(2),
                        TimeSpan.FromMinutes(2),
                        _consoleWriterActor,
                        new NoActivity($"No file activity for last {WarningTimespanMinutes} minutes, type 'exit' to finish watching or 'ignore' to dismiss the warning."),
                        ActorRefs.Nobody);
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new ValidationError($"{msg} is not an existing URI on disk"));
                }
            }
            // tell sender to continue doing its thing
            // (whatever that may be, this actor doesn't care)
            Sender.Tell(new ContinueProcessing());
        }
        
        /// <summary>
        /// Determines if the message received is valid
        /// Checks if number of chars in message received is even
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}