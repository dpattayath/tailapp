using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using TailApp.Messages;

namespace TailApp.Actors
{
    /// <summary>
    /// Actor that validates user input and signals result to others
    /// </summary>
    public class ValidationActor: ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _consoleWriterActor;

        private ICancelable _cancellation;
        
        private const ushort WarningTimespanMinutes = 2;
        
        private List<string> _files = new List<string>();
        
        // added along with the IWithUnboundedStash interface
        public IStash Stash {get;set;}

        public ValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
            Handler();
        }

        private void Handler()
        {
            Receive<string>(msg => string.IsNullOrEmpty(msg), msg =>
            {
                _consoleWriterActor.Tell(new NullInputError("Input was blank. Please try again."));
                Sender.Tell(new ContinueProcessing());
            });
            Receive<FileNotWatching>(msg =>
            {
                _consoleWriterActor.Tell(new ValidationError($"{msg.FilePath} not in the watch list"));
                Sender.Tell(new ContinueProcessing());
            });
            ReceiveAny(msg =>
            {
                var message = msg as string;
                if (CheckFileInWatchList(message))
                {
                    _consoleWriterActor.Tell(new ValidationError($"{message} is already on watch list"));
                }
                else if (!IsFileUri(message))
                {
                    _consoleWriterActor.Tell(new ValidationError($"{message} is not an existing URI on disk"));
                }
                else
                {
                    _consoleWriterActor.Tell(new InputSuccess($"Starting processing for {message}"));
                    Context.ActorSelection("akka://tailAppActorSystem/user/tailCoordinatorActor")
                        .Tell(new StartTail(message, _consoleWriterActor));

                    // schedule to check no activity every 2 minutes
                    _cancellation = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                        TimeSpan.FromMinutes(WarningTimespanMinutes),
                        TimeSpan.FromMinutes(WarningTimespanMinutes),
                        _consoleWriterActor,
                        new NoActivityWarning($"No file activity for last {WarningTimespanMinutes} minutes, type 'exit' to finish watching or 'ignore' to dismiss the warning."),
                        ActorRefs.Nobody);
                    
                    _files.Add(message);
                    
                    // change validator mode to accept commands
                    BecomeStacked(CommandMode);
                }
                Sender.Tell(new ContinueProcessing());
            });
        }

        private void CommandMode()
        {
            Receive<string>(s => string.Equals(s, "ignore", StringComparison.OrdinalIgnoreCase), s =>
            {
                _cancellation.Cancel();
                _consoleWriterActor.Tell(new InputSuccess("disabled no activity warnings"));
                Sender.Tell(new ContinueProcessing());
            });
            Receive<string>(s => s.StartsWith("end-", StringComparison.OrdinalIgnoreCase), s =>
            {
                string[] parts = s.Split('-');
                if (CheckFileInWatchList(parts[1]))
                {
                    Context.ActorSelection("akka://tailAppActorSystem/user/tailCoordinatorActor")
                        .Tell(new StopTail(parts[1]), Self);

                    _files.Remove(parts[1]);
                    
                    // back to file mode / normal mode
                    UnbecomeStacked();
                    Stash.UnstashAll();
                }
                else
                {
                    _consoleWriterActor.Tell(new ValidationError($"file {parts[1]} not in the watch list"));
                }
                Sender.Tell(new ContinueProcessing());
            });
            ReceiveAny(s =>
            {
                Stash.Stash();
                Sender.Tell(new ContinueProcessing());
            });
        }

        /// <summary>
        /// Determines if the file exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileUri(string path)
        {
            return File.Exists(path);
        }

        private bool CheckFileInWatchList(string path)
        {
            return _files.Exists(element => element.Equals(path));
        }
    }
}