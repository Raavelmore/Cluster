using Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public class VoiceChatDatabase
	{
		static Dictionary<int, VoiceChat> VoiceChats = new Dictionary<int, VoiceChat>();

		public static VoiceChat? GetChat(int id)
		{
			if(VoiceChats.ContainsKey(id)) return VoiceChats[id];
			return null;
		}
		public static void AddVoiceChat(VoiceChat chat)
		{
			if (VoiceChats.ContainsKey(chat.Id)) return;
			VoiceChats.Add(chat.Id, chat);
		}
	}
}
