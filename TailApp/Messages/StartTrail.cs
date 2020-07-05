using Akka.Actor;

namespace TailApp.Messages
{
    /// <summary>
    /// Start tailing the file at user-specified path
    /// </summary>
    public class StartTail
    {
        public string FilePath { get; private set;  }
        public IActorRef ReporterActor { get; private set;  }

        public StartTail(string filePath, IActorRef reporterActor)
        {
            FilePath = filePath;
            ReporterActor = reporterActor;
        }
    }
}