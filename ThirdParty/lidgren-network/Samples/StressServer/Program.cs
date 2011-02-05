using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lidgren.Network;
using SamplesCommon;

namespace StressServer
{
	static class Program
	{
		private static Form1 s_mainForm;
		private static NetServer s_server;
		private static NetBuffer s_readBuffer;

		private static double s_nextDisplay;
		private static int s_userMessagesReceived;
		private static int m_appFrameCount;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();

			NetConfiguration config = new NetConfiguration("stress");
			config.Port = 14242;
			config.MaxConnections = 32;
			s_server = new NetServer(config);

			s_server.SimulatedMinimumLatency = 0.1f;
			s_server.SimulatedLatencyVariance = 0.05f;
//			s_server.SimulatedLoss = 0.1f;
//			s_server.SimulatedDuplicates = 0.05f;

			s_readBuffer = s_server.CreateBuffer();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);

			s_server.Shutdown("Application exiting");
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				NetConnection source;
				NetMessageType type;
				while (s_server.ReadMessage(s_readBuffer, out type, out source))
					HandleMessage(type, source, s_readBuffer);

				double now = NetTime.Now;
				if (now > s_nextDisplay && s_server.Connections.Count > 0)
				{
					double span = now - (s_nextDisplay - 0.2);
					double appFps = (double)m_appFrameCount / span;
					s_mainForm.Text = "App fps: " + appFps;
					m_appFrameCount = 0;

					UpdateStatisticsDisplay(s_server.Connections[0]);
					s_nextDisplay = now + 0.2; // 5 fps max
				}

				m_appFrameCount++;
				System.Threading.Thread.Sleep(0);
			}
		}

		private static void UpdateStatisticsDisplay(NetConnection connection)
		{
			string stats = s_server.GetStatisticsString(connection);
			s_mainForm.label1.Text = stats + 
				Environment.NewLine +
				"User messages received by app: " + s_userMessagesReceived + Environment.NewLine
			;
		}

		private static double s_tmp;
		private static void HandleMessage(NetMessageType type, NetConnection source, NetBuffer buffer)
		{
			switch (type)
			{
				case NetMessageType.DebugMessage:
					WriteToConsole(buffer.ReadString());
					break;
				case NetMessageType.StatusChanged:
					string statusMessage = buffer.ReadString();
					NetConnectionStatus newStatus = (NetConnectionStatus)buffer.ReadByte();

					WriteToConsole("New status: " + newStatus + " (" + statusMessage + ")");
					UpdateStatisticsDisplay(source);
					break;
				case NetMessageType.Data:
					//System.IO.File.AppendAllText("C:\\receivedpackets.txt", s_userMessagesReceived.ToString() + ": " + msg.ReadString() + " (" + msg.m_sequenceNumber + ")" + Environment.NewLine);
					s_userMessagesReceived++;

					// simulate some processing of the message here
					for (int i = 0; i < buffer.LengthBytes - 2; i++)
						buffer.ReadByte();

					// check checksum
					ushort checksum = NetChecksum.Adler16(buffer.Data, 0, buffer.LengthBytes - 2);
					ushort given = buffer.ReadUInt16();
					if (checksum != given)
						WriteToConsole("Wrong checksum! Expected " + checksum + " found given " + given);

					double b = s_userMessagesReceived;
					for (int i = 0; i < 1000; i++)
						b += Math.Sqrt((double)i) / Math.Sin(s_tmp);
					s_tmp += b / 10000.0;
					break;
				default:
					break;
			}
		}

		public static void WriteToConsole(string text)
		{
			try
			{
				s_mainForm.richTextBox1.AppendText(text + Environment.NewLine);
				NativeMethods.ScrollRichTextBox(s_mainForm.richTextBox1);
			}
			catch
			{
				// gulp
			}
		}

		public static void Run()
		{
			s_server.Start();
		}

		public static void Shutdown()
		{
			s_server.Shutdown("Bye bye");
		}
	}
}