using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lidgren.Network;
using SamplesCommon;

namespace StressClient
{
	static class Program
	{
		private static Form1 s_mainForm;
		private static NetClient s_client;
		private static int s_messagesPerSecond = 10;
		private static double s_sentUntil;
		private static double s_oneMessageTime;
		private static double s_nextDisplay;
		private static int s_messagesSent = 0;
		private static NetBuffer s_readBuffer;
		private static NetChannel m_sendOnChannel;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();

			NetConfiguration config = new NetConfiguration("stress");
			config.ThrottleBytesPerSecond = 3500;

			s_client = new NetClient(config);

			// 100 ms simulated roundtrip latency
			s_client.SimulatedMinimumLatency = 0.1f;

			// ... + 0 to 50 ms
			s_client.SimulatedLatencyVariance = 0.05f;

			// 10% loss (!)
		//	s_client.SimulatedLoss = 0.1f;

			// 5% duplicated messages (!)
		//	s_client.SimulatedDuplicates = 0.05f;
			
			s_readBuffer = s_client.CreateBuffer();

			s_sentUntil = NetTime.Now;
			s_nextDisplay = NetTime.Now;
			
			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);

			s_client.Shutdown("Application exiting");
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

		private static byte[] s_randomData = new byte[128];
		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				double now = NetTime.Now;

				if (s_client.Status != NetConnectionStatus.Disconnected)
				{
					NetMessageType type;
					while(s_client.ReadMessage(s_readBuffer, out type))
						HandleMessage(type, s_readBuffer);

					// send stressing packets
					if (s_client.Status == NetConnectionStatus.Connected)
					{
						while (s_client.Status == NetConnectionStatus.Connected && s_sentUntil < now)
						{
							NetBuffer sendBuffer = s_client.CreateBuffer();
							sendBuffer.Reset();
							NetRandom.Instance.NextBytes(s_randomData);
							sendBuffer.Write(s_randomData);

							// calculate and append checksum
							ushort checksum = NetChecksum.Adler16(s_randomData, 0, s_randomData.Length);
							sendBuffer.Write(checksum);
					
							//m_buffer.Write("Nr: " + s_messagesSent);
							s_client.SendMessage(sendBuffer, m_sendOnChannel);
							s_messagesSent++;
							s_sentUntil += s_oneMessageTime;
						}
					}
				}

				if (now > s_nextDisplay)
				{
					UpdateStatisticsDisplay(s_client.ServerConnection);
					s_nextDisplay = now + 0.2; // five times per second
				}

				System.Threading.Thread.Sleep(1);
			}
		}

		private static void UpdateStatisticsDisplay(NetConnection connection)
		{
			string stats = s_client.GetStatisticsString(connection);
			s_mainForm.label3.Text = stats + 
				Environment.NewLine +
				"User messages sent: " + s_messagesSent + Environment.NewLine
			;
		}

		private static void HandleMessage(NetMessageType type, NetBuffer data)
		{
			switch (type)
			{
				case NetMessageType.DebugMessage:
					WriteToConsole(data.ReadString());
					break;
				case NetMessageType.StatusChanged:
					string statusMessage = data.ReadString();
					NetConnectionStatus newStatus = (NetConnectionStatus)data.ReadByte();
					WriteToConsole("New status: " + newStatus + " (" + statusMessage + ")");
					UpdateStatisticsDisplay(s_client.ServerConnection);
					break;
				case NetMessageType.Data:
					WriteToConsole("Message received: " + data);
					break;
				default:
					WriteToConsole("Unhandled: " + type);
					break;
			}
		}

		internal static void Run(string host, int port, int mps, int throttle, NetChannel channel)
		{
			s_client.Configuration.ThrottleBytesPerSecond = throttle;
			s_client.Connect(host, port, null);
			s_messagesPerSecond = mps;
			s_oneMessageTime = 1.0 / (double)mps;
			s_sentUntil = NetTime.Now;
			m_sendOnChannel = channel;
		}

		internal static void Shutdown()
		{
			s_client.Disconnect("Cya");
		}
	}
}