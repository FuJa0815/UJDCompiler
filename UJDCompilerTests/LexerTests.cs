using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UJDCompiler.Tests
{
    [TestClass]
    public class LexerTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Program.Attr = new Program();
        }

        [TestMethod]
        public void GetTokensTest()
        {
            var result = new List<Token>
            {
                new UjdToken {Code  = "   	 	"},
                new JavaToken {Code = "a"},
                new UjdToken {Code  = "   		 "}
            };
            CollectionAssert.AreEqual(result,
                                      new
                                          List<Token>(Lexer.GetTokens(new
                                                                          StreamReader(GenerateStreamFromString("   	 	a   		 ")))));
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}