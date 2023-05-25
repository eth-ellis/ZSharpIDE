using ZSharpIDE.Models;

namespace ZSharpIDE
{
    public sealed class Constants
    {
        #region Keywords

        public static readonly Keyword[] ReservedKeywords =
        { 
            new Keyword("abstract", ""),
            new Keyword("as", ""),
            new Keyword("base", ""),
            new Keyword("bool", ""),
            new Keyword("break", ""),
            new Keyword("byte", ""),
            new Keyword("case", ""),
            new Keyword("catch", ""),
            new Keyword("char", ""),
            new Keyword("checked", ""),
            new Keyword("class", "king"),
            new Keyword("class", "queen"),
            new Keyword("const", ""),
            new Keyword("continue", ""),
            new Keyword("decimal", ""),
            new Keyword("default", ""),
            new Keyword("delegate", ""),
            new Keyword("do", ""),
            new Keyword("double", ""),
            new Keyword("else", ""),
            new Keyword("enum", ""),
            new Keyword("event", ""),
            new Keyword("explicit", ""),
            new Keyword("extern", ""),
            new Keyword("false", "cap"),
            new Keyword("finally", ""),
            new Keyword("fixed", ""),
            new Keyword("float", ""),
            new Keyword("for", ""),
            new Keyword("foreach", ""),
            new Keyword("goto", ""),
            new Keyword("if", ""),
            new Keyword("implicit", ""),
            new Keyword("in", ""),
            new Keyword("in", ""),
            new Keyword("int", ""),
            new Keyword("interface", ""),
            new Keyword("internal", ""),
            new Keyword("is", ""),
            new Keyword("lock", ""),
            new Keyword("long", ""),
            new Keyword("namespace", ""),
            new Keyword("new", ""),
            new Keyword("null", ""),
            new Keyword("object", ""),
            new Keyword("operator", ""),
            new Keyword("out", ""),
            new Keyword("override", ""),
            new Keyword("params", ""),
            new Keyword("private", "lowkey"),
            new Keyword("protected", ""),
            new Keyword("public", "highkey"),
            new Keyword("readonly", ""),
            new Keyword("ref", ""),
            new Keyword("return", ""),
            new Keyword("sbyte", ""),
            new Keyword("sealed", ""),
            new Keyword("short", ""),
            new Keyword("sizeof", ""),
            new Keyword("stackalloc", ""),
            new Keyword("static", ""),
            new Keyword("string", ""),
            new Keyword("struct", ""),
            new Keyword("switch", ""),
            new Keyword("this", ""),
            new Keyword("throw", ""),
            new Keyword("true", "nocap"),
            new Keyword("try", ""),
            new Keyword("typeof", ""),
            new Keyword("uint", ""),
            new Keyword("ulong", ""),
            new Keyword("unchecked", ""),
            new Keyword("unsafe", ""),
            new Keyword("ushort", ""),
            new Keyword("using", ""),
            new Keyword("using static", ""),
            new Keyword("void", ""),
            new Keyword("volatile", ""),
            new Keyword("while", "")
        };

        public static readonly Keyword[] ContextualKeywords =
        { 
            new Keyword("add", ""),
            new Keyword("alias", ""),
            new Keyword("ascending", ""),
            new Keyword("async", ""),
            new Keyword("await", ""),
            new Keyword("descending", ""),
            new Keyword("dynamic", ""),
            new Keyword("from", ""),
            new Keyword("get", ""),
            new Keyword("global", ""),
            new Keyword("group", ""),
            new Keyword("into", ""),
            new Keyword("join", ""),
            new Keyword("let", ""),
            new Keyword("orderby", ""),
            new Keyword("partial", ""),
            new Keyword("remove", ""),
            new Keyword("select", ""),
            new Keyword("set", ""),
            new Keyword("value", ""),
            new Keyword("var", ""),
            new Keyword("when", ""),
            new Keyword("where", ""),
            new Keyword("yield", "")
        };

        public static readonly Keyword[] SpecialKeywords =
        {
            new Keyword("Console.WriteLine(\"Bustaclat\")", "bustaclat")
        };

        #endregion
    }
}
