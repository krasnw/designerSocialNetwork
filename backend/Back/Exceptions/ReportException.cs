namespace Back.Exceptions
{
    public class ReportException : Exception
    {
        public ReportException(string message) : base(message) { }
        public ReportException(string message, Exception inner) : base(message, inner) { }
    }
}
