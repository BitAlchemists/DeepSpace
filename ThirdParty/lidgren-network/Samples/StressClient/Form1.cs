using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Lidgren.Network;

namespace StressClient
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
			NetChannel channel = NetChannel.Unreliable;
			switch (comboBox1.SelectedIndex)
			{
				case 0:
					channel = NetChannel.Unreliable;
					break;
				case 1:
					channel = NetChannel.UnreliableInOrder1;
					break;
				case 2:
					channel = NetChannel.ReliableUnordered;
					break;
				case 3:
					channel = NetChannel.ReliableInOrder1;
					break;
			}
			Program.Run(textBox2.Text, Int32.Parse(textBox3.Text), Int32.Parse(textBox1.Text), Int32.Parse(textBox4.Text), channel);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Program.Shutdown();
		}
	}
}