using System.Collections.Generic;

namespace Operations.Classification.Collections
{
    public class Trie
    {
        private readonly Node _root;

        public Trie()
        {
            _root = new Node('^', 0, null);
        }

        public void Delete(string s)
        {
            if (Search(s))
            {
                var node = Prefix(s).FindChildNode('$');

                while (node.IsLeaf())
                {
                    var parent = node.Parent;
                    parent.DeleteChildNode(node.Value);
                    node = parent;
                }
            }
        }

        public void Insert(string s)
        {
            var commonPrefix = Prefix(s);
            var current = commonPrefix;

            for (var i = current.Depth; i < s.Length; i++)
            {
                var newNode = new Node(s[i], current.Depth + 1, current);
                current.Children.Add(newNode);
                current = newNode;
            }

            current.Children.Add(new Node('$', current.Depth + 1, current));
        }

        public void InsertRange(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                Insert(item);
            }
        }

        public Node Prefix(string s)
        {
            var currentNode = _root;
            var result = currentNode;

            foreach (var c in s)
            {
                currentNode = currentNode.FindChildNode(c);
                if (currentNode == null)
                {
                    break;
                }

                result = currentNode;
            }

            return result;
        }

        public bool Search(string s)
        {
            var prefix = Prefix(s);
            return prefix.Depth == s.Length && prefix.FindChildNode('$') != null;
        }

        public class Node
        {
            public Node(char value, int depth, Node parent)
            {
                Value = value;
                Children = new List<Node>();
                Depth = depth;
                Parent = parent;
            }

            public char Value { get; set; }

            public List<Node> Children { get; set; }

            public Node Parent { get; set; }

            public int Depth { get; set; }

            public void DeleteChildNode(char c)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    if (Children[i].Value == c)
                    {
                        Children.RemoveAt(i);
                    }
                }
            }

            public Node FindChildNode(char c)
            {
                foreach (var child in Children)
                {
                    if (child.Value == c)
                    {
                        return child;
                    }
                }

                return null;
            }

            public bool IsLeaf()
            {
                return Children.Count == 0;
            }
        }
    }
}