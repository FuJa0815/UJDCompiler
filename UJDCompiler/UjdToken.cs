using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UJDCompiler
{
    public class UjdToken : Token
    {
        private class UjdNode
        {
            private UjdNode _spaceNode;
            private UjdNode _tabNode;
            internal UjdNode SpaceNode => _spaceNode ??= new UjdNode();
            internal UjdNode TabNode => _tabNode ??= new UjdNode();
            internal string Value { get; set; }
        }

        private const string FILENAME = "DialektLookup";

        private static UjdNode root = new UjdNode();

        private static void WriteToTree(Stack<char> remain, string result, UjdNode currNode)
        {
            if (remain.TryPop(out var c))
                WriteToTree(remain, result, c == ' ' ? currNode.SpaceNode : currNode.TabNode);
            else
                currNode.Value = result;
        }

        public static void LoadTree()
        {
            File.ReadAllLines(FILENAME).AsParallel().Select(p => p.Split('0')).ForAll(p =>
                  WriteToTree(new Stack<char>(p[0].ToCharArray().Reverse()), p[1], root));
            //_ujdLookupTable = (from line in File.ReadAllLines(FILENAME).AsParallel() select line.Split('0')).ToDictionary(p=>p[0], p=>p[1]);
        }


        public override IEnumerable<JavaToken> JToken
        {
            get
            {
                UjdNode currentNode = root;
                foreach (var c in Code.ToCharArray())
                {
                    if (currentNode.Value != null)
                    {
                        yield return new JavaToken() { Code = currentNode.Value };
                        currentNode = root;
                    }
                    currentNode = c == ' ' ? currentNode.SpaceNode : currentNode.TabNode;
                }
                yield return new JavaToken() { Code = currentNode.Value };
            }
        }
    }
}
