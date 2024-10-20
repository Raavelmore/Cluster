using ClusterClient.Network;
using ClusterClient.Sound;

namespace ClusterClient
{
	public partial class App : Application
	{
		public App()
		{
			Audio.Init();
			PacketHandler.Start();
			InitializeComponent();
			MainPage = new AppShell();
		}
	}
}
