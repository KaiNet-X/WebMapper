using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaiNetWebMapper
{
    public class UrlTree2 : IEnumerable<Url>
    {
        public readonly Url Url;
        public readonly List<UrlTree2> Nodes = new List<UrlTree2>();
        public readonly UrlTree2 Parent;

        public delegate void Add(Url url, List<UrlTree2> fullTree);
        public static Add AddNodeToTree;

        public static TreeView Tree;

        public UrlTree2() { }
        public UrlTree2(Url url) => Url = url;
        public UrlTree2(Url url, UrlTree2 parent)
        {
            Url = url;
            Parent = parent;
        }

        public async Task AddNode(Url url)
        {
            if (Url == null)
            {
                string[] segments = url.Segments;

                UrlTree2 tree = Nodes.FirstOrDefault(n => n.Url.FullDomain == url.FullDomain);

                if (tree == null)
                {
                    string link = "";

                    if (segments.Length > 0) link = url.Base.Replace(Join(segments), "");
                    else link = url.Base;

                    Url u = new Url(link);
                    tree = new UrlTree2(u, this);

                    Nodes.Add(tree);
                }
                await tree.AddNode(url);
            }
            else if (url.Segments.Length != Url.Segments.Length)
            {
                string[] segments = Url.Segments;
                UrlTree2 tree = Nodes
                    .Where(n => n.Url.Segments.Length > segments.Length)
                    .FirstOrDefault(n => n.Url.Segments[segments.Length] == url.Segments[segments.Length]);

                if (tree == null)
                {
                    string link = "";
                    if (segments.Length > 0) link = Url.Base.Replace(Join(segments), "");
                    else link = Url.Base;

                    string[] seg = new string[segments.Length + 1];

                    for (int i = 0; i < segments.Length; i++)
                    {
                        seg[i] = segments[i];
                    }

                    seg[segments.Length] = url.Segments[segments.Length];
                    link += Join(seg);

                    Url u = new Url(link);
                    tree = new UrlTree2(u, this);

                    Nodes.Add(tree);
                }
                await tree.AddNode(url);
            }
            else
            {
                AddToTree(url);
            }
        }

        public static string Join(string[] arr)
        {
            string s = "";
            for (int i = 0; i < arr.Length; i++)
            {
                s += '/' + arr[i];
            }
            return s;
        }

        public void AddToTree(Url url)
        {
            List<UrlTree2> fullTree = GetHeritance();
            //fullTree.Reverse();
            TreeNodeCollection nodes = Tree.Nodes;
            AddNodeToTree(url, fullTree);
                //for(int i = 0; i < fullTree.Count; i++)
                //{
                //    if (fullTree[i].Url == null) continue;
                //    if (!nodes.ContainsKey(fullTree[i].Url.Base)) 
                //        nodes.Add(fullTree[i].Url.Base, fullTree[i].Url.Base);
                //    nodes = nodes[fullTree[i].Url.Base].Nodes;
                //}
            //if (!nodes.ContainsKey(url.Base)) nodes.Add(url.Base, url.Base);
            //nodes = nodes[url.Base].Nodes;
        }

        public List<UrlTree2> GetHeritance()
        {
            List<UrlTree2> trees = new List<UrlTree2>();
            if (Parent != null) trees.AddRange(Parent.GetHeritance());
            trees.Add(this);
            return trees;
        }

        public IEnumerator<Url> GetEnumerator()
        {
            if (Url != null)
                yield return Url;
            foreach (UrlTree2 tree in Nodes)
            {
                foreach (Url u in tree)
                {
                    yield return u;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
