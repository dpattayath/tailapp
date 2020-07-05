using Akka.Actor;

namespace TailApp.Messages
{
    /// <summary>
    /// Stop tailing the file
    /// </summary>
    public class StopTail
    {
        public string FilePath { get; private set;  }

        public StopTail(string filePath)
        {
            FilePath = filePath;
        }
    }
}