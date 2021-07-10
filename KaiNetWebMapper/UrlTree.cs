using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaiNetWebMapper
{
    public class UrlTree
    {
        public readonly Url Url;
        public readonly List<UrlTree> Nodes = new List<UrlTree>();

        public UrlTree(Url url) => Url = url;

        public async Task AddNode(Url url)
        {
            if (url.Base == Url.Base || Nodes.FirstOrDefault(n => n.Url.Base == url.Base) != null) return;
            if (url.Segments.Length != Url.Segments.Length)
            {
                string[] segments = Url.Segments;
                UrlTree tree = Nodes
                    .Where(n => n.Url.Segments.Length > segments.Length)
                    .FirstOrDefault(n => n.Url.Segments[segments.Length] == url.Segments[segments.Length] );

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
                    tree = new UrlTree(u);

                    Nodes.Add(tree);
                }
                await tree.AddNode(url);
            }
        }
        
        public static async Task AddToTree(List<UrlTree> trees, Url url)
        {
            UrlTree tree = trees.FirstOrDefault(t => t.Url.FullDomain == url.FullDomain);
            if (tree == null)
            {
                string link = "";

                if (url.Segments.Length > 0) link = url.Base.Replace(Join(url.Segments), "");
                else link = url.Base;

                Url u = new Url(link);
                tree = new UrlTree(u);
                trees.Add(tree);
            }
            else
            {
                await tree.AddNode(url);
            }
        }

        public TreeNode ToNodeTree()
        {
            TreeNode[] nodes = new TreeNode[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                nodes[i] = Nodes[i].ToNodeTree();
            }

            return new TreeNode(Url.Base, nodes);
        }
        public static TreeNode[] GetNodeTree(List<UrlTree> trees)
        {
            TreeNode[] nodes = new TreeNode[trees.Count];
            for (int i = 0; i < trees.Count; i++)
            {
                nodes[i] = trees[i].ToNodeTree();
            }

            return nodes;
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
    }
}
