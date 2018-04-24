using QYWeiXinDemo.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QYWeiXinDemo
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            QYWeiXinHelper.DoAlert(textBox3.Text, textBox1.Text, textBox4.Text, textBox2.Text, textBox5.Text);
        }
    }
}
