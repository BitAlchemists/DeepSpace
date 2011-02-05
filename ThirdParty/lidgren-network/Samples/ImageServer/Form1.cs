using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageServer
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			comboBox1.SelectedIndex = 0;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.CheckFileExists = true;
			dlg.RestoreDirectory = true;
			dlg.Filter = "Images|*.jpg;*.png;*.bmp";

			DialogResult res = dlg.ShowDialog();
			if (res != DialogResult.OK)
				return;

			Program.Run(dlg.FileName);
		}
	}
}