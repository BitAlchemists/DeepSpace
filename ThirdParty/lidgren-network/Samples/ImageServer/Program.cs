using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SamplesCommon;
using Lidgren.Network;
using System.IO;
using System.Drawing;

namespace ImageServer
{
	public enum ImageClientStatus
	{
		JustConnected,
		WaitingForSizeReceipt,
		Running
	}

	static class Program
	{
		private static Form1 s_mainForm;
		private static NetServer s_server;
		private static byte[] s_imageData;
		private static uint s_imageWidth, s_imageHeight;
		private static Dictionary<NetConnection, uint> s_nextPixelToSend;
		private static double s_nextSend;
		private static NetBuffer s_readBuffer;

		private const float m_loss = 0.01f;

		private static double s_fpsStart;
		private static int s_ticks;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();

			s_nextPixelToSend = new Dictionary<NetConnection, uint>();

			NetConfiguration config = new NetConfiguration("imageservice");
			config.Port = 14242;
			config.MaxConnections = 64;
			config.ThrottleBytesPerSecond = 25000;
			s_server = new NetServer(config);
			s_server.SimulatedMinimumLatency = 0.05f;
			s_server.SimulatedLoss = m_loss;
			s_fpsStart = NetTime.Now;

			s_readBuffer = s_server.CreateBuffer();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);

			s_server.Shutdown("Application exit");
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			NetChannel useChannel = NetChannel.Unreliable;
			switch (s_mainForm.comboBox1.SelectedIndex)
			{
				case 0: useChannel = NetChannel.Unreliable; break;
				case 1: useChannel = NetChannel.ReliableUnordered; break;
				case 2: useChannel = NetChannel.ReliableInOrder1; break;
			}

			while (NativeMethods.AppStillIdle)
			{
				NetConnection source;
				NetMessageType type;
				while(s_server.ReadMessage(s_readBuffer, out type, out source))
					HandleMessage(type, source, s_readBuffer);

				// send segment to all connections
				double now = NetTime.Now;
				if (now > s_nextSend)
				{
					foreach (NetConnection conn in s_server.Connections)
					{
						if (conn.Status == NetConnectionStatus.Connected)
						{
							NetBuffer outBuf = null;
							if (conn.Tag == null)
								continue;
							switch ((ImageClientStatus)conn.Tag)
							{
								case ImageClientStatus.JustConnected:
									if (outBuf == null)
										outBuf = s_server.CreateBuffer();
									// send pixel size
									outBuf.WriteVariableUInt32(s_imageWidth);
									outBuf.WriteVariableUInt32(s_imageHeight);

									// send size using reliable and receipt, so we know when it's received and we can start pushing pixels
									conn.SendMessage(outBuf, NetChannel.ReliableUnordered, s_server.CreateBuffer());

									conn.Tag = ImageClientStatus.WaitingForSizeReceipt;
									break;
								case ImageClientStatus.WaitingForSizeReceipt:
									// keep waiting
									break;
								case ImageClientStatus.Running:
									if (outBuf == null)
										outBuf = s_server.CreateBuffer();

									uint nextPixel = s_nextPixelToSend[conn];
									uint chunkSize = 128;

									uint pixelsToSend = (uint)(s_imageData.Length / 3) - nextPixel;
									if (pixelsToSend > 0)
									{
										if (pixelsToSend > chunkSize)
											pixelsToSend = chunkSize;
										outBuf.Reset();
										outBuf.Write(nextPixel);
										outBuf.Write(s_imageData, (int)(nextPixel * 3), (int)(pixelsToSend * 3));
										conn.SendMessage(outBuf, useChannel);

										s_nextPixelToSend[conn] = nextPixel + pixelsToSend;
									}
									else
									{
										// disconnect?
										if (conn.UnsentMessagesCount < 1)
											conn.Disconnect("Fin", 5.0f);
									}
									break;
							}
						}
					}

					s_nextSend = now + (1.0 / 150.0); // 150 segments per second max
				}

				s_ticks++;

				double fpsnow = NetTime.Now;
				double span = fpsnow - s_fpsStart;
				if (span >= 1.0)
				{
					double fps = (double)s_ticks / span;
					s_ticks = 0;
					s_fpsStart = fpsnow;
					s_mainForm.Text	= "Server " + (int)fps + " fps; " + s_server.Connections.Count + " connections";

					s_mainForm.label2.Text = s_server.GetStatisticsString(s_server.Connections.Count > 0 ? s_server.Connections[0] : null);
				}

				System.Threading.Thread.Sleep(1);
			}
		}

		internal static void Run(string filename)
		{
			s_server.Start();

			// get image size
			Bitmap bm = Bitmap.FromFile(filename) as Bitmap;
			s_imageWidth = (uint)bm.Width;
			s_imageHeight = (uint)bm.Height;

			// extract color bytes
			// very slow method, but small code size
			s_imageData = new byte[3 * s_imageWidth * s_imageHeight];
			int ptr = 0;
			for (int y = 0; y < s_imageHeight; y++)
			{
				for (int x = 0; x < s_imageWidth; x++)
				{
					Color color = bm.GetPixel(x, y);
					s_imageData[ptr++] = color.R;
					s_imageData[ptr++] = color.G;
					s_imageData[ptr++] = color.B;
				}
			}

			bm.Dispose();
		}

		private static void HandleMessage(NetMessageType type, NetConnection source, NetBuffer buffer)
		{
			switch (type)
			{
				case NetMessageType.DebugMessage:
					Console.WriteLine(buffer.ReadString());
					break;
				case NetMessageType.StatusChanged:
					NetConnectionStatus status = source.Status;
					if (status == NetConnectionStatus.Connected)
					{
						source.Tag = ImageClientStatus.JustConnected;
						s_nextPixelToSend[source] = 0;
					}
					else if (status == NetConnectionStatus.Disconnected)
					{
						if (s_nextPixelToSend.ContainsKey(source))
							s_nextPixelToSend.Remove(source);
					}
					break;
				case NetMessageType.Receipt:
					source.Tag = ImageClientStatus.Running;
					break;
				default:
					// unhandled
					break;
			}
		}
	}
}