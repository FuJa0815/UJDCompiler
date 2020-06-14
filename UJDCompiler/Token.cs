using System;
using System.Collections.Generic;

namespace UJDCompiler
{
    public abstract class Token
    {
        public string Code { get; set; } = "";

        public abstract IEnumerable<JavaToken> JToken { get; }

        public static Type TokenFactory(int c, char lT, char rT) =>
            c == lT || c == rT ? typeof(UjdToken) : typeof(JavaToken);

        protected bool Equals(Token other) => Code == other.Code;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Token) obj);
        }

        public override int GetHashCode() => (Code != null ? Code.GetHashCode() : 0);
    }
}