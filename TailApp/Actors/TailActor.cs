using System;
using System.IO;
using Akka.Actor;
using System.Text;
using TailApp.Common;
using TailApp.Messages;

namespace TailApp.Actors
{
    public class TailActor : UntypedActor
    {
        private readonly IActorRef _reporterActor;
        private readonly StreamReader _fileStreamReader;

        public TailActor(string filePath, IActorRef reporterActor)
        {
            _reporterActor = reporterActor;

            // start watching file for changes
            FileObserver observer = new FileObserver(Self, Path.GetFullPath(filePath));
            observer.Start();
            
            // open the file stream with shared read/write permissions
            // (so file can be written to while open)
            FileStream fileStream = new FileStream(Path.GetFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(filePath, text));
        }
        
        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                // move file cursor forward
                // pull results from cursor to end of file and write to output
                // (this is assuming a log file type format that is append-only)
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell(string.Format("Tail error {0}", fe.Reason));
            }
            else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }
    }
}