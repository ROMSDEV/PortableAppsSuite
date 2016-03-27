using System;
using System.Windows.Forms;

namespace GuidGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) =>
            Icon = Properties.Resources.Hengerek_Teal;

        private void button1_Click(object sender, EventArgs e) =>
            textBox1.Text = Guid.NewGuid().ToString();
    }
}
