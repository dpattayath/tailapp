using Akka.Actor;
using TailApp.Actors;
using TailApp.Messages;

namespace TailApp
{
    class Program
    {
        public static ActorSystem MyActorSystem;
        
        static void Main(string[] args)
        {
            // initialize ActorSystem
            MyActorSystem = ActorSystem.Create("tailAppActorSystem");
            
            Props writerActorProp = Props.Create(() => new ConsoleWriterActor());
            IActorRef writerActorRef = MyActorSystem.ActorOf(writerActorProp, "consoleWriterActor");

            // even though this actor is not being used/referenced here, it need to initialised and add to the actor system context to make it discoverable
            Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            Props fileValidatorActorProps = Props.Create(() => new FileValidationActor(writerActorRef));
            IActorRef fileValidationActor = MyActorSystem.ActorOf(fileValidatorActorProps, "fileValidationActor");

            Props readerActorProp = Props.Create<ConsoleReaderActor>();
            IActorRef readerActorRef = MyActorSystem.ActorOf(readerActorProp, "consoleReaderActor");
            
            // tell console reader to begin
            readerActorRef.Tell(new StartProcessing());
            
            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
}