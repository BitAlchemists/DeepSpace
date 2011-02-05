using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using System.Threading;

namespace ChatClient
{
	class Program
	{
		private static bool s_keepGoing = true;

		static void Main(string[] args)
		{
			// create a client with a default configuration
			NetConfiguration config = new NetConfiguration("chatApp");
			NetClient client = new NetClient(config);
			client.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);
			client.SetMessageTypeEnabled(NetMessageType.DebugMessage, true);
			//client.SetMessageTypeEnabled(NetMessageType.VerboseDebugMessage, true);
			client.Start();

			// Wait half a second to allow server to start up if run via Visual Studio
			Thread.Sleep(500);

			// Emit discovery signal
			client.DiscoverLocalServers(14242);

			// create a buffer to read data into
			NetBuffer buffer = client.CreateBuffer();

			// current input string
			string input = "";

			// keep running until the user presses a key
			Console.WriteLine("Type 'quit' to exit client");
			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
			s_keepGoing = true;
			while (s_keepGoing)
			{
				NetMessageType type;

				// check if any messages has been received
				while (client.ReadMessage(buffer, out type))
				{
					switch (type)
					{
						case NetMessageType.ServerDiscovered:
							// just connect to any server found!

							// make hail
							NetBuffer buf = client.CreateBuffer();
							buf.Write("Hail from " + Environment.MachineName);
							client.Connect(buffer.ReadIPEndPoint(), buf.ToArray());
							break;
						case NetMessageType.ConnectionRejected:
							Console.WriteLine("Rejected: " + buffer.ReadString());
							break;
						case NetMessageType.DebugMessage:
						case NetMessageType.VerboseDebugMessage:
							Console.WriteLine(buffer.ReadString());
							break;
						case NetMessageType.StatusChanged:
							string statusMessage = buffer.ReadString();
							NetConnectionStatus newStatus = (NetConnectionStatus)buffer.ReadByte();
							Console.WriteLine("New status: " + newStatus + " (" + statusMessage + ")");
							break;
						case NetMessageType.Data:
							// The server sent this data!
							string msg = buffer.ReadString();
							Console.WriteLine(msg);
							break;
					}
				}

				while (Console.KeyAvailable)
				{
					ConsoleKeyInfo ki = Console.ReadKey();
					if (ki.Key == ConsoleKey.Enter)
					{
						if (!string.IsNullOrEmpty(input))
						{
							if (input == "quit")
							{
								// exit application
								s_keepGoing = false;
							}
							else
							{
								// Send chat message
								NetBuffer sendBuffer = new NetBuffer();
								sendBuffer.Write(input);
								client.SendMessage(sendBuffer, NetChannel.ReliableInOrder1);
								input = "";
							}
						}
					}
					else
					{
						input += ki.KeyChar;
					}
				}

				Thread.Sleep(1);
			}

			client.Shutdown("Application exiting");
		}

		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			s_keepGoing = false;
		}
	}
}
