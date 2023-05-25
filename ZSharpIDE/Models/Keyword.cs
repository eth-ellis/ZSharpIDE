namespace ZSharpIDE.Models
{
    public sealed class Keyword
    {
        public string CSharpKeyword { get; set; }

        public string ZSharpKeyword { get; set; }

        public Keyword(string cSharpKeyword, string zSharpKeyword)
        {
            this.CSharpKeyword = cSharpKeyword;
            this.ZSharpKeyword = zSharpKeyword;
        }

        /// <summary>
        /// Returns the <see cref="ZSharpKeyword"/> if available, otherwise returns the <see cref="CSharpKeyword"/>
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.ZSharpKeyword) ? this.CSharpKeyword : this.ZSharpKeyword;
        }
    }
}
