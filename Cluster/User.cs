using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public delegate void VoiceChatChangingHandler(User user, int from, int to);
	public class User
	{
		public string Name { get; set; }
		public string Password { get; set; }
		public int Id { get; set; }
		public List<int> Rooms { get; set; } = new List<int>();
		int voiceChat = 0;
		public int CurrentVoiceChat 
		{ get 
			{
				return voiceChat;
			} 
			set 
			{
				VoiceChat? oldChat = VoiceChatDatabase.GetChat(voiceChat);
				if (oldChat != null) oldChat.Leave(this);
				int oldValue = voiceChat;
				voiceChat = value;
				VoiceChat? newChat = VoiceChatDatabase.GetChat(voiceChat);
				if(newChat != null) newChat.Join(this);
				OnVoiceChatChanged?.Invoke(this, oldValue, voiceChat);
			}
		}
		public event VoiceChatChangingHandler? OnVoiceChatChanged;
		public User(string name, string password)
		{
			Name = name;
			Password = password;
			UserDatabase.AddUser(this);
		}
	}
}
