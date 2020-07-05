namespace TailApp.Messages
{
    /// <summary>
    /// Signal that the mentioned file has been changed with the content
    /// </summary>
    public class FileChanged
    {
        public string FilePath { get; private set; }
        
        public string Content { get; private set; }
        
        public FileChanged(string filePath, string content)
        {
            FilePath = filePath;
            Content = content;
        }
    }
}