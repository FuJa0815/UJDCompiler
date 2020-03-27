using System;

namespace UJDCompiler
{
    public abstract class Token
    {
        public string Code { get; set; } = "";

        public static Type GetToken(int c)
        {
            return c == ' ' || c == '\t' ? typeof(UjdToken) : typeof(JavaToken);
        }
    }
}
