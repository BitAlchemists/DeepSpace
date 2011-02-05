using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SamplesCommon;

namespace MultiImageClient
{
	static class Program
	{
		private static Form1 s_mainForm;
		private static List<ImageClient> s_clients;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();
			s_clients = new List<ImageClient>();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				foreach (ImageClient client in s_clients)
					client.Heartbeat();
				System.Threading.Thread.Sleep(1);
			}
		}

		internal static void LaunchClient()
		{
			// create a new ImageClient window and add it to the list
			ImageClient client = new ImageClient();
			s_clients.Add(client);
		}

		internal static void DestroyClient(ImageClient client)
		{
			client.Close();
			s_clients.Remove(client);
		}
	}
}