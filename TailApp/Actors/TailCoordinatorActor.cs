using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using TailApp.Messages;

namespace TailApp.Actors
{
    public class TailCoordinatorActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = (StartTail) message;
                
                // the TailActor instance created here is a child of this instance of TailCoordinatorActor
                Context.ActorOf(Props.Create(() => new TailActor( msg.FilePath, msg.ReporterActor)), $"tailActor-{msg.FilePath.Replace('/','_')}");
            }
            else if (message is StopTail)
            {
                var msg = (StopTail) message;
                var child = Context.ActorSelection($"tailActor-{msg.FilePath.Replace('/','_')}");
                child.Tell(message);
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // withinTimeRange
                x =>
                {
                    // Not relevant but for demonstration - ArithmeticException to not be application critical
                    if (x is ArithmeticException) return Directive.Resume;

                    //Error that we cannot recover from, stop the failing actor
                    else if (x is NotSupportedException) return Directive.Stop;

                    //In all other cases, just restart the failing actor
                    else return Directive.Restart;
                });
        }
    }
}