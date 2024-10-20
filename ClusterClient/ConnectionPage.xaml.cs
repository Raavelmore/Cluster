using ClusterClient.Network;
using System.Diagnostics;

namespace ClusterClient;

public partial class ConnectionPage : ContentPage
{
	static ConnectionPage Instance;
	public ConnectionPage()
	{
		Instance = this;
		InitializeComponent();
	}
	public void ConnectBtnPressed(object sender, EventArgs e)
	{
		if (Client.Instance.Connected) return;
		Client.Instance.Username = UsernameEntry.Text;
		Client.Instance.Password = PasswordEntry.Text;
		string[] adress;
		int port = 0;
		try
		{
			adress = IpEntry.Text.Split(':');
			port = int.Parse(adress[1]);
		}
		catch
		{
			return;
		}
		Client.Instance.Connect(adress[0], port);
	}
	public static void Close()
	{
		MainThread.InvokeOnMainThreadAsync(() => { Instance.Navigation.PopAsync(); });
	}
}