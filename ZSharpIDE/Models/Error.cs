using ZSharpIDE.Enums;

namespace ZSharpIDE.Models
{
    public sealed class Error
    {
        public Severity Severity { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public int Line { get; set; }

        public Error(Severity severity, string code, string description, string filePath, int line)
        {
            this.Severity = severity;
            this.Code = code;
            this.Description = description;
            this.FilePath = filePath;
            this.Line = line;
        }
    }
}
