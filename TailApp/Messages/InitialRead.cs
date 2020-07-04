namespace TailApp.Messages
{
    /// <summary>
    /// Signal to read the initial contents of the file at actor startup
    /// </summary>
    public class InitialRead
    {
        public string Text { get; private set; }

        public string FileName { get; private set; }
        
        public InitialRead(string fileName, string text)
        {
            FileName = fileName;
            Text = text;
        }
    }
}