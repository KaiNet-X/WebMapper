using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace KaiNetWebMapper
{
    public partial class FindForm : Form
    {
        private readonly UrlTree2 Urls;

        public FindForm(UrlTree2 urls)
        {
            Urls = urls;
            InitializeComponent();
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(Urls.Select(u => u.Base).Where(u => u.Contains(textBox1.Text)).ToArray());
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                foreach (object item in listBox1.SelectedItems)
                    copy_buffer.AppendLine(item.ToString());
                if (copy_buffer.Length > 0)
                    Clipboard.SetText(copy_buffer.ToString());
            }
        }
    }
}
