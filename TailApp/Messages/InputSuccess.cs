namespace TailApp.Messages
{
    /// <summary>
    /// Base class for signalling that user input was valid
    /// </summary>
    public class InputSuccess
    {
        public string Reason { get; private set; }
        
        public InputSuccess(string reason)
        {
            Reason = reason;
        }
    }
}