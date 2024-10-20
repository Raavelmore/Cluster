using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cluster
{
	public class Room
	{
		public string Name { get; set; }
		public int Id { get; private set; }
		public List<string> Users { get; set; } = new();
		public List<int> VoiceChats { get; set; } = new();
		public List<int> TextChats { get; set; } = new();

		public Room(string name)
		{
			Id = GetHashCode();
			VoiceChat voiceChat = new VoiceChat("Voice chat");
			voiceChat.RoomId = Id;
			VoiceChats.Add(voiceChat.Id);
			TextChat textChat = new TextChat("Text chat");
			textChat.RoomId = Id;
			TextChats.Add(textChat.Id);
			Name = name;
			RoomDatabase.AddRoom(this);
		}
		public Room(string name, int id)
		{
			Name = name;
			Id = id;
		}
	}
}
