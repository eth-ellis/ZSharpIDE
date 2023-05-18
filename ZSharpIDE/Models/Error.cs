using ZSharpIDE.Enums;

namespace ZSharpIDE.Models
{
    public sealed class Error
    {
        public Severity Severity { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string FileName { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public Error(Severity severity, string errorCode, string errorMessage, string fileName, int line, int column)
        {
            this.Severity = severity;
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
            this.FileName = fileName;
            this.Line = line;
            this.Column = column;
        }
    }
}
