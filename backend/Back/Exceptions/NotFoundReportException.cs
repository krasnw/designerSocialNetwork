namespace Back.Exceptions
{
    public class NotFoundReportException : ReportException
    {
        public NotFoundReportException(string message) : base(message) { }
    }
}
