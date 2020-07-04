using Akka.Actor;
using TailApp.Actors;

namespace TailApp
{
    class Program
    {
        public static ActorSystem MyActorSystem;
        
        static void Main(string[] args)
        {
            // initialize ActorSystem
            MyActorSystem = ActorSystem.Create("TailAppActorSystem");
            
            Props writerActorProp = Props.Create(() => new ConsoleWriterActor());
            IActorRef writerActorRef = MyActorSystem.ActorOf(writerActorProp, "consoleWriterActor");

            Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            Props fileValidatorActorProps = Props.Create(() => new FileValidationActor(writerActorRef, tailCoordinatorActor));
            IActorRef fileValidationActor = MyActorSystem.ActorOf(fileValidatorActorProps, "fileValidationActor");

            Props readerActorProp = Props.Create<ConsoleReaderActor>(fileValidationActor);
            IActorRef readerActorRef = MyActorSystem.ActorOf(readerActorProp, "consoleReaderActor");
            
            // tell console reader to begin
            readerActorRef.Tell(ConsoleReaderActor.StartCommand);
            
            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
}