using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UJDCompiler
{
    public class UjdToken : Token
    {
        private static readonly UjdNode root = new UjdNode();


        public override IEnumerable<JavaToken> JToken
        {
            get
            {
                var currentNode = root;
                foreach (var c in Code.ToCharArray())
                {
                    if (currentNode.Value != null)
                    {
                        yield return new JavaToken {Code = currentNode.Value};
                        currentNode = root;
                    }

                    currentNode = c == CommandLine.Attr.LeftChar ? currentNode.LeftNode : currentNode.RightNode;
                }

                yield return new JavaToken {Code = currentNode.Value};
            }
        }

        private static void WriteToTree(Stack<char> remain, string result, UjdNode currNode)
        {
            if (remain.TryPop(out var c))
                WriteToTree(remain, result, c == CommandLine.Attr.LeftChar ? currNode.LeftNode : currNode.RightNode);
            else
                currNode.Value = result;
        }

        public static void LoadTree(string dialectLookupFile, char dialectLookupSeparator)
        {
            File.ReadAllLines(dialectLookupFile).AsParallel()
                .Select(p => p.Split(dialectLookupSeparator)).ForAll(p =>
                                                                         WriteToTree(new Stack<char>(p[0].ToCharArray().Reverse()),
                                                                                     p[1], root));
            //_ujdLookupTable = (from line in File.ReadAllLines(FILENAME).AsParallel() select line.Split('0')).ToDictionary(p=>p[0], p=>p[1]);
        }

        private class UjdNode
        {
            private  UjdNode _leftNode;
            private  UjdNode _rightNode;
            internal UjdNode LeftNode  => _leftNode ??= new UjdNode();
            internal UjdNode RightNode => _rightNode ??= new UjdNode();
            internal string  Value     { get; set; }
        }
    }
}