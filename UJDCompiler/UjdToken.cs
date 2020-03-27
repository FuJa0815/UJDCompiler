using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace UJDCompiler
{
    public class UjdToken : Token
    {
        private class UjdNode
        {
            internal UjdNode SpaceNode { get; set; }
            internal UjdNode TabNode { get; set; }
            internal string Value { get; set; }
        }

        private const string FILENAME = "ujdTable.csv";

        private static UjdNode root = new UjdNode();

        private static void WriteToTree(Stack<char> remain, string result, UjdNode currNode)
        {
            if (remain.TryPop(out var c))
            {
                if (c == ' ')
                {
                    if (currNode.SpaceNode == null) currNode.SpaceNode = new UjdNode();
                    WriteToTree(remain, result, currNode.SpaceNode);
                } else
                {
                    if (currNode.TabNode == null) currNode.TabNode = new UjdNode();
                    WriteToTree(remain, result, currNode.TabNode);
                }
            } else
            {
                currNode.Value = result;
            }
        }

        public static void LoadTree()
        {
            File.ReadAllLines(FILENAME).AsParallel().Select(p=>p.Split('0')).ForAll(p =>
                WriteToTree(new Stack<char>(p[0].ToCharArray()), p[1], root));
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
