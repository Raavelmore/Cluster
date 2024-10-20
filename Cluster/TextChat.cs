using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public class TextChat
	{
		public string Name { get; set; }
		public List<Message> Messages { get; set; } = new();
		public int Id { get; private set; }
		public int RoomId { get; set; }

		public TextChat(string name)
		{
			Name = name;
			Id = GetHashCode();
			TextChatDatabase.AddVoiceChat(this);
		}
		public TextChat(string name, int id)
		{
			Name = name;
			Id = id;
		}
	}
}
