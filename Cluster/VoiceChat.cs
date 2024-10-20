using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public class VoiceChat
	{
		public string Name { get; set; }
		public List<string> Users { get; set; } = new();
		public int Id { get; private set; }
		public int RoomId { get; set; }
		public VoiceChat(string name)
		{
			Name = name;
			Id = GetHashCode();
			VoiceChatDatabase.AddVoiceChat(this);
		}
		public VoiceChat(string name, int id)
		{
			Name = name;
			Id = id;
		}
		public void Join(User user)
		{
			if (Users.Contains(user.Name)) return;
			Users.Add(user.Name);
		}
		public void Leave(User user)
		{
			if (!Users.Contains(user.Name)) return;
			Users.Remove(user.Name);
		}
	}
}
