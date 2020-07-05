namespace TailApp.Messages
{
    public class FileNotWatching
    {
        public string FilePath { get; private set;  }

        public FileNotWatching(string filePath)
        {
            FilePath = filePath;
        }
    }
}