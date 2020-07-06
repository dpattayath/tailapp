using System;
using System.IO;
using Akka.Actor;
using System.Text;
using TailApp.Common;
using TailApp.Messages;

namespace TailApp.Actors
{
    /// <summary>
    /// Actor handling file watching
    /// Demonstrating usage of UntypedActor
    /// </summary>
    public class TailActor : UntypedActor
    {
        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private StreamReader _fileStreamReader;
        private FileObserver _observer;

        public TailActor(string filePath, IActorRef reporterActor)
        {
            _filePath = filePath;
            _reporterActor = reporterActor;
        }

        /// <summary>
        /// initialization logic for actor that will tail changes to a file.
        /// </summary>
        protected override void PreStart()
        {
            base.PreStart();
            
            // start watching file for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();
            
            // open the file stream with shared read/write permissions, (so file can be written to while open)
            FileStream fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new FileChanged(_filePath, text));
        }

        /// <summary>
        /// clean up
        /// </summary>
        protected override void PostStop()
        {
            _observer.Dispose();
            _observer = null;
            _fileStreamReader.Close();
            _fileStreamReader.Dispose();
            base.PostStop();
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
                    _reporterActor.Tell(new FileChanged(_filePath, text));
                }
            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell(new InputError($"Tail error {fe.Reason}"));
            }
            else if (message is FileChanged)
            {
                var changed = message as FileChanged;
                _reporterActor.Tell(changed);
            }
            else if (message is StopTail)
            {
                _reporterActor.Tell(new InputSuccess($"user ended file watch for {_filePath}"));
                Context.Stop(Self);
            }
        }
    }
}