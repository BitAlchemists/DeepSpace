using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Lidgren.Network;
using System.Net;

namespace MultiImageClient
{
	public partial class ImageClient : Form
	{
		private NetClient m_client;
		private int m_imageWidth;
		private int m_imageHeight;
		private int m_lineDisplayed;
		private NetBuffer m_readBuffer;

		public ImageClient()
		{
			InitializeComponent();

			NetConfiguration config = new NetConfiguration("imageservice");
			m_client = new NetClient(config);
			m_client.SimulatedMinimumLatency = 0.05f;

			m_readBuffer = m_client.CreateBuffer();

			m_client.DiscoverLocalServers(14242);

			this.FormClosed += new FormClosedEventHandler(OnClosed);
			this.Show();
		}

		void OnClosed(object sender, FormClosedEventArgs e)
		{
			m_client.Shutdown("Form exiting");
		}

		/// <summary>
		/// Heartbeat; is called regularely by the main application
		/// </summary>
		public void Heartbeat()
		{
			NetMessageType type;
			while (m_client.ReadMessage(m_readBuffer, out type))
				HandleMessage(type, m_readBuffer);
		}

		/// <summary>
		/// Handle incoming message
		/// </summary>
		private void HandleMessage(NetMessageType type, NetBuffer buffer)
		{
			switch (type)
			{
				case NetMessageType.DebugMessage:
					//
					// it's a library debug message; just display it in the console if debugger is attached
					//
					Console.WriteLine(buffer.ReadString());
					break;
				case NetMessageType.StatusChanged:
					//
					// it's a status change message; set the reason as window title and refresh picture
					//
					this.Text = buffer.ReadString();
					pictureBox1.Refresh();
					break;
				case NetMessageType.ServerDiscovered:
					//
					// it's a server discovered message; connect to the discovered server
					//
					m_imageWidth = 0;
					m_imageHeight = 0;
					m_lineDisplayed = 0;

					m_client.Connect(buffer.ReadIPEndPoint());
					break;
				case NetMessageType.Data:
					//
					// it's a data message (data sent from the server)
					//
					if (m_imageWidth == 0)
					{
						// first message is size
						m_imageWidth = (int)buffer.ReadVariableUInt32();
						m_imageHeight = (int)buffer.ReadVariableUInt32();
						this.Size = new System.Drawing.Size(m_imageWidth + 40, m_imageHeight + 60);
						pictureBox1.Image = new Bitmap(m_imageWidth, m_imageHeight);
						pictureBox1.SetBounds(12, 12, m_imageWidth, m_imageHeight);
						return;
					}

					uint pixelPos = buffer.ReadUInt32();

					// it's color data
					int y = (int)(pixelPos / m_imageWidth);
					int x = (int)(pixelPos - (y * m_imageWidth));

					Bitmap bm = pictureBox1.Image as Bitmap;
					pictureBox1.SuspendLayout();
					int pixels = (buffer.LengthBytes - 4) / 3;
					for (int i = 0; i < pixels; i++)
					{
						// set pixel
						byte r = buffer.ReadByte();
						byte g = buffer.ReadByte();
						byte b = buffer.ReadByte();
						Color col = Color.FromArgb(r, g, b);
						if (y > m_imageHeight)
							continue;
						bm.SetPixel(x, y, col);
						x++;
						if (x >= m_imageWidth)
						{
							x = 0;
							y++;
						}
					}
					pictureBox1.ResumeLayout();

					// refresh image every horizontal line
					if (pixelPos / m_imageWidth > m_lineDisplayed)
					{
						m_lineDisplayed = (int)(pixelPos / m_imageWidth);
						pictureBox1.Refresh();
					}
					break;
				default:
					// unhandled
					break;
			}
		}
	}
}