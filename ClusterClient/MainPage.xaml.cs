using Cluster;
using ClusterClient.Network;
using ClusterClient.Sound;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace ClusterClient
{
	public partial class MainPage : ContentPage
	{
		public static int VoiceChatId = 0;
		public static int SelectedRoom = 0;

		static VerticalStackLayout Rooms;
		static VerticalStackLayout Chats;
		static VerticalStackLayout Users;
		static Button DisconnectButton;
		static Label usernameLabel;
		static bool micEnabled = true;
		static Dictionary<int, VerticalStackLayout> VoiceChatViews = new();

		Thread VoiceThread = new(SendVoice);
		public MainPage()
		{
			InitializeComponent();
			if (!Client.Instance.Connected)
			{
				Navigation.PushAsync(new ConnectionPage());
			}
			Rooms = RoomList;
			Chats = ChatList;
			Users = UserList;
			DisconnectButton = DisconnectBtn;
			usernameLabel = UsernameLabel;
			VoiceThread.Start();
		}
		public void AddRoom(object sender, EventArgs e)
		{
			Navigation.PushAsync(new AddServerPage());
		}
		public void DisconnectButtonClicked(object sender, EventArgs e) 
		{
			VoiceChatId = 0;
			PacketHandler.DisconnectFromVoiceChat();
			DisconnectBtn.IsEnabled = false;
		}
		public void MicSwitchToggled(object sender, EventArgs e)
		{
			micEnabled = MicSwitch.IsToggled;
		}
		public void NoiseGateSliderValueChanged(object sender, EventArgs e)
		{
			SoundProcessing.NoiseGateValue = (float)NoiseGateSlider.Value;
		}
		public static void AddRoomPreview(Room room)
		{
			MainThread.InvokeOnMainThreadAsync(() => 
			{
				Button roomButton = new();
				roomButton.Text = room.Name;
				roomButton.Clicked += (s, e) => 
				{ 
					if (SelectedRoom == room.Id) return; 
					SelectedRoom = room.Id; 
					PacketHandler.SendRoomRequest(room.Id);
				};
				Rooms.Children.Add(roomButton);
			});
		}
		public static void ClearRoom()
		{
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				Chats.Children.Clear();
				Users.Children.Clear();
				VoiceChatViews.Clear();
			});
		}
		public static void AddRoomId(int roomId)
		{
			MainThread.InvokeOnMainThreadAsync(() => { Chats.Children.Add(new Label() { Text = $"ID комнаты: {roomId}" }); });
		}
		public static void AddTextChat(TextChat textChat)
		{
			MainThread.InvokeOnMainThreadAsync(() => { Chats.Children.Add(new Label() { Text = $"{textChat.Name} TEXT CHATS ARE NOT SUPPORTED YET!" }); });
		}
		public static void AddVoiceChat(VoiceChat voiceChat)
		{
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				VerticalStackLayout views = new VerticalStackLayout();
				views.Margin = 25;
				Button chatButton = new();
				chatButton.Text = voiceChat.Name;
				chatButton.Clicked += (s, e) =>
				{
					if(VoiceChatId == voiceChat.Id) return;
					PacketHandler.SendVoiceChatConnectionRequest(voiceChat.Id);
				};
				views.Children.Add(chatButton);
				foreach (var user in voiceChat.Users) 
				{
					views.Children.Add(new Label() { Text = user });
				}
				Chats.Children.Add(views);
				VoiceChatViews.Add(voiceChat.Id, views);
			});
		}
		public static void AddUser(string username) 
		{
			MainThread.InvokeOnMainThreadAsync(() => { Users.Children.Add(new Label() { Text = username }); });
		}
		public static void UserJoinedVoiceChat(string username, int chat)
		{
			MainThread.InvokeOnMainThreadAsync(() => 
			{
				VoiceChatViews[chat].Children.Add(new Label() { Text = username });
			});
		}
		public static void UserLeftVoiceChat(string username, int chat)
		{
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				Label? userLabel = null;
				foreach(IView view in VoiceChatViews[chat].Children)
				{
					if (view.GetType() != typeof(Label)) continue;
					Label label = (Label)view;
					if (label == null) continue;
					if(label.Text == username)
					{
						userLabel = label;
						break;
					}
				}
				if (userLabel == null) return;
				VoiceChatViews[chat].Children.Remove(userLabel);
			});
		}
		public static void EnableDisconnectButton()
		{
			MainThread.InvokeOnMainThreadAsync(() => { DisconnectButton.IsEnabled = true; });
		}
		public static void UpdateUsernameLabel()
		{
			MainThread.InvokeOnMainThreadAsync(() => { usernameLabel.Text = Client.Instance.Username; });
		}
		static void SendVoice()
		{
			while (true)
			{
				Thread.Sleep(100);
				if (!micEnabled || VoiceChatId == 0) 
				{
					Audio.GetRecorded();
					continue;
				}
				AudioSample audio = SoundProcessing.NoiseGate(Audio.GetRecorded());
				if (audio.Type == SampleType.Garbage) continue;
				PacketHandler.SendVoice(audio.Data);
			}
		}
	}

}
