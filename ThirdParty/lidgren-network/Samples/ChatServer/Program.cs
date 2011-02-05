using System;
using System.Collections.Generic;

using Lidgren.Network;
using System.Threading;

namespace ChatServer
{
	class Program
	{
		static void Main(string[] args)
		{
			// create a configuration for the server
			NetConfiguration config = new NetConfiguration("chatApp");
			config.MaxConnections = 128;
			config.Port = 14242;

			// create server and start listening for connections
			NetServer server = new NetServer(config);
			server.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
			server.Start();

			// create a buffer to read data into
			NetBuffer buffer = server.CreateBuffer();

			// keep running until the user presses a key
			Console.WriteLine("Press ESC to quit server");
			bool keepRunning = true;
			while (keepRunning)
			{
				NetMessageType type;
				NetConnection sender;

				// check if any messages has been received
				while(server.ReadMessage(buffer, out type, out sender))
				{
					switch (type)
					{
						case NetMessageType.DebugMessage:
							Console.WriteLine(buffer.ReadString());
							break;
						case NetMessageType.ConnectionApproval:
							Console.WriteLine("Approval; hail is " + buffer.ReadString());
							sender.Approve();
							break;
						case NetMessageType.StatusChanged:
							string statusMessage = buffer.ReadString();
							NetConnectionStatus newStatus = (NetConnectionStatus)buffer.ReadByte();
							Console.WriteLine("New status for " + sender + ": " + newStatus + " (" + statusMessage + ")");
							break;
						case NetMessageType.Data:
							// A client sent this data!
							string msg = buffer.ReadString();

							// send to everyone, including sender
							NetBuffer sendBuffer = server.CreateBuffer();
							sendBuffer.Write(sender.RemoteEndpoint.ToString() + " wrote: " + msg);

							// send using ReliableInOrder
							server.SendToAll(sendBuffer, NetChannel.ReliableInOrder1);
							break;
					}
				}

				// User pressed ESC?
				while (Console.KeyAvailable)
				{
					ConsoleKeyInfo info = Console.ReadKey();
					if (info.Key == ConsoleKey.Escape)
						keepRunning = false;
				}

				Thread.Sleep(1);
			}

			server.Shutdown("Application exiting");
		}
	}
}
