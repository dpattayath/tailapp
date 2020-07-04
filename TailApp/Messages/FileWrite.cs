namespace TailApp.Messages
{
    /// <summary>
    /// Signal that the file has changed, and we need to read the next line of the file.
    /// </summary>
    public class FileWrite
    {
        public string FileName { get; private set; }

        public FileWrite(string fileName)
        {
            FileName = fileName;
        }
    }
}