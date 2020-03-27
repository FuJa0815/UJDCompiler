using System;
using System.Collections.Generic;

namespace UJDCompiler
{
    public abstract class Token
    {
        public string Code { get; set; } = "";

        public abstract IEnumerable<JavaToken> JToken { get; }

        public static Type GetToken(int c)
        {
            return c == ' ' || c == '\t' ? typeof(UjdToken) : typeof(JavaToken);
        }
    }
}
