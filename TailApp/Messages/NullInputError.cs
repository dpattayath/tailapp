namespace TailApp.Messages
{
    /// <summary>
    /// User provided blank input
    /// </summary>
    public class NullInputError : InputError
    {
        public NullInputError(string reason) : base(reason)
        {
            
        }
    }
}