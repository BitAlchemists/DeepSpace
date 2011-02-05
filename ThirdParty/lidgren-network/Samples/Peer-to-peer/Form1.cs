using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PeerToPeer
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			textBox1.KeyDown += new KeyEventHandler(OnKeyDown);
		}

		void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
				button1_Click(sender, e);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(textBox1.Text))
				Program.Input(textBox1.Text);
			textBox1.Text = "";
		}
	}
}