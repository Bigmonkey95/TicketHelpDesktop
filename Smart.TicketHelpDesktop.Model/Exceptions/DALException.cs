
namespace Smart.TicketHelpDesktop.Model.Exceptions
{
    /// <summary>
    /// Database Exception
    /// </summary>
    public class DALException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public DALException() : base("Error communicating with database") { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">exception message</param>
        public DALException(string message) : base(message) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="inner">inner exception</param>
        public DALException(string message, Exception inner) : base(message, inner) { }
    }
}
