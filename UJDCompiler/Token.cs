using System;
using System.Collections.Generic;

namespace UJDCompiler
{
    public abstract class Token
    {
        public string Code { get; set; } = "";

        public abstract IEnumerable<JavaToken> JToken { get; }

        public static Type GetToken(int c, char lT, char rT) =>
            c == lT || c == rT ? typeof(UjdToken) : typeof(JavaToken);

        public override bool Equals(object obj) => (obj as Token)?.Code == Code;
    }
}