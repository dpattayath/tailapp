namespace TailApp.Messages
{
    /// <summary>
    /// Base class for signalling that user input was invalid
    /// </summary>
    public class InputError
    {
        public string Reason { get; private set; }
        
        public InputError(string reason)
        {
            Reason = reason;
        }
    }
}