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
            int c;
            while((c = s.Read()) >= 0)
            {
                var t = Token.GetToken(c);
                if (currentToken == null)
                    currentToken = (Token)Activator.CreateInstance(t);
                else if (currentToken.GetType() != t)
                {
                    //token changed
                    yield return currentToken;
                    currentToken = (Token) Activator.CreateInstance(t);
                }

                currentToken.Code += (char) c;
            }

            yield return currentToken;
        }
    }
}
