using ClusterClient.Network;
using System.Diagnostics;

namespace ClusterClient;

public partial class AddServerPage : ContentPage
{
	public AddServerPage()
	{
		InitializeComponent();
	}
	public void CreateRoom(object sender, EventArgs e)
	{
		PacketHandler.SendRoomCreationRequest(RoomNameEntry.Text);
		Navigation.PopAsync();
	}
	public void EnterRoom(object sender, EventArgs e)
	{
		int id = 0;
		try
		{
			id = Convert.ToInt32(RoomIdEntry.Text);
		}
		catch (Exception ex) 
		{
			Trace.WriteLine(ex);
			return;
		}
		PacketHandler.SendRoomPreviewRequest(id);
		Navigation.PopAsync();
	}
	public void Back(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
}