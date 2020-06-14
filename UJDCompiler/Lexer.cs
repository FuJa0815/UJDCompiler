using System;
using System.Collections.Generic;
using System.IO;

namespace UJDCompiler
{
    public static class Lexer
    {
        public static IEnumerable<Token> GetTokens(StreamReader s)
        {
            Token currentToken = null;
            int   c;
            while ((c = s.Read()) >= 0)
            {
                var t = Token.TokenFactory(c, CommandLine.Attr.LeftChar, CommandLine.Attr.RightChar);
                if (currentToken == null || currentToken.GetType() != t)
                {
                    //token changed
                    if (currentToken != null) yield return currentToken;
                    currentToken = (Token) Activator.CreateInstance(t);
                }

                currentToken.Code += (char) c;
            }

            yield return currentToken;
        }
    }
}