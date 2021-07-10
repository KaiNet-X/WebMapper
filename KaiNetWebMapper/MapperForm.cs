using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Windows.Forms;

namespace KaiNetWebMapper
{
    public partial class MapperForm : Form
    {
        private static List<UrlTree> trees = new List<UrlTree>();
        private static UrlTree2 Tree = new UrlTree2();
        public static List<string> searched = new List<string>();
        private static List<Task> tasks = new List<Task>();
        private Url Base;
        private bool Canceled = false;
        int hits = 0;

        public MapperForm()
        {
            InitializeComponent();
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            string path = URLBox.Text;
            Base = new Url(path);

            UrlTree2.Tree = treeView1;
            UrlTree2.AddNodeToTree = Add;

            SearchButton.Enabled = false;
            CancelButton.Enabled = true;
            progressBar1.Style = ProgressBarStyle.Marquee;

            Task.Run(async () =>
            {
                return;
                int searchCount = searched.Count;
                do
                {
                    searchCount = searched.Count;
                    await Task.Delay(5000);
                }
                while (searched.Count > searchCount);
                Canceled = true;
                await Task.Delay(2500);
                MethodInvoker m = () =>
                {
                    SearchButton.Enabled = true;
                    CancelButton.Enabled = false;
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = 100;
                };
                Invoke(m);
            });

            await SearchUrl2(path);

        }

        private async Task SearchUrl(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = await web.LoadFromWebAsync(url);

            Url path = new Url(url);
            string s;
            Url u;
            hits++;

            MethodInvoker m = () => label2.Text = hits.ToString();
            Invoke(m);

            foreach (var node in doc.DocumentNode.Descendants("a"))
            {
                s = node.GetAttributeValue("href", "");
                s = s.TrimEnd('/');

                u = new Url(s);

                if (u.FullDomain == "")
                {
                    if (u.Path != "")
                    {
                        s = (path + s).TrimEnd('/');
                        u = new Url(s);
                    }
                    else continue;
                }

                Task t;
                lock (trees)
                    t = UrlTree.AddToTree(trees, u);

                await t;

                if (checkedListBox1.CheckedItems.Contains("Recursive"))
                {
                    lock (trees)
                    {
                        if (!searched.Contains(s) && u.Domain == Base.Domain)
                        {
                            if (tasks.Count >= 8)
                            {
                                Task.Run(() =>
                                {
                                    //while (tasks.Count >= 16)
                                    FinishTasks();
                                    lock (trees)
                                    {
                                        searched.Add(s);
                                        tasks.Add(SearchUrl(s));
                                    }
                                });
                            }
                            else
                            {
                                Task.Run(() =>
                                {
                                    //while (tasks.Count >= 16)
                                    lock (trees)
                                    {
                                        searched.Add(s);
                                        tasks.Add(SearchUrl(s));
                                    }
                                });
                            }
                        }
                    }
                }
            }
            m = () =>
            {
                treeView1.Nodes.Clear();
                treeView1.Nodes.AddRange(UrlTree.GetNodeTree(trees));
            };
            Invoke(m);
        }

        private async Task SearchUrl2(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = await web.LoadFromWebAsync(url);

            url = url.TrimEnd('/');
            Url path = new Url(url);
            string s;
            Url u;
            hits++;

            MethodInvoker m = () => label2.Text = hits.ToString();
            Invoke(m);

            foreach (var node in doc.DocumentNode.Descendants("a"))
            {
                if (Canceled) return;

                s = node.GetAttributeValue("href", "");
                if (s == "/") s = url;
                s = s.TrimEnd('/');

                u = new Url(s);

                if (u.FullDomain == "")
                {
                    if (u.Path != "")
                    {
                        s = (url + s).TrimEnd('/');
                        u = new Url(s);
                    }
                    else continue;
                }

                if (u.Domain != Base.Domain)
                {
                    if (!checkedListBox1.CheckedItems.Contains("Show links to other sites")) continue;
                }

                await Tree.AddNode(u);

                if (checkedListBox1.CheckedItems.Contains("Recursive"))
                {
                    lock (Tree)
                    {
                        bool b;
                        if (checkedListBox1.CheckedItems.Contains("Match full domain"))
                            b = u.FullDomain == Base.FullDomain;
                        else
                            b = u.Domain == Base.Domain;
                        if (!searched.Contains(s) && b)
                        {
                            Task.Run(() =>
                            {
                                check:
                                if (tasks.Count >= 8)
                                {
                                    FinishTasks();
                                }
                                lock (trees)
                                {
                                    if (tasks.Count >= 8) goto check;
                                    if (Canceled) return;
                                    searched.Add(s);
                                    tasks.Add(SearchUrl2(s));
                                }
                            });
                        }
                    }
                }
            }
        }

        private void FinishTasks()
        {
            lock (tasks)
            {
                while (tasks.Count > 0)
                {
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if (tasks[i] != null && tasks[i].IsCompleted)
                        {
                            tasks.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }

        private void Add(Url u, List<UrlTree2> tree)
        {
            MethodInvoker m = () =>
            {
                TreeNodeCollection nodes = treeView1.Nodes;
                for (int i = 0; i < tree.Count; i++)
                {
                    if (tree[i].Url == null) continue;
                    if (!nodes.ContainsKey(tree[i].Url.Base))
                        nodes.Add(tree[i].Url.Base, tree[i].Url.Base);

                    //nodes[tree[i].Url.Base].ExpandAll();

                    nodes = nodes[tree[i].Url.Base].Nodes;
                }
                //treeView1.ExpandAll();

            };
            Invoke(m);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Canceled = true;
            SearchButton.Enabled = true;
            CancelButton.Enabled = false;
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            FindForm f = new FindForm(Tree);
            f.ShowDialog();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                Clipboard.SetText(treeView1.SelectedNode.Text);
            }
        }
    }
}