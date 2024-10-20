using Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public class TextChatDatabase
	{
		static Dictionary<int, TextChat> TextChats = new Dictionary<int, TextChat>();

		public static TextChat? GetChat(int id)
		{
			if (TextChats.ContainsKey(id)) return TextChats[id];
			return null;
		}
		public static void AddVoiceChat(TextChat chat)
		{
			if (TextChats.ContainsKey(chat.Id)) return;
			TextChats.Add(chat.Id, chat);
		}
	}
}
