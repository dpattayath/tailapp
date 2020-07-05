namespace TailApp.Messages
{
    /// <summary>
    /// Signal that there has been write to the file
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