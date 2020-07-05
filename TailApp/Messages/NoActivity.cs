namespace TailApp.Messages
{
    public class NoActivity
    {
        public string Reason { get; private set; }
        
        public NoActivity(string reason)
        {
            Reason = reason;
        }
    }
}