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
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new ValidationError(string.Format("{0} is not an existing URI on disk", msg)));
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