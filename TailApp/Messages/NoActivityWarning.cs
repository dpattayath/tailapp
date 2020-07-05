namespace TailApp.Messages
{
    /// <summary>
    /// Signal no activity warning
    /// </summary>
    public class NoActivityWarning
    {
        public string Reason { get; private set; }
        
        public NoActivityWarning(string reason)
        {
            Reason = reason;
        }
    }
}