using System.Collections.Generic;

namespace UJDCompiler
{
    public class JavaToken : Token
    {
        public override IEnumerable<JavaToken> JToken => new[] { this };
    }
}
