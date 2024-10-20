using Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public class UserDatabase
	{
		public static VoiceChatChangingHandler? VoiceChatChanging;
		static Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();

		public static User? GetUser(string username)
		{
			if(Users.ContainsKey(username)) return Users[username];
			return null;
		}
		public static void AddUser(User user)
		{
			if (Users.ContainsKey(user.Name)) return;
			user.OnVoiceChatChanged += VoiceChatChanging;
			Users.Add(user.Name, user);
		}
		public static bool CheckPassword(string username, string password)
		{
			return Users[username].Password == password;
		}
	}
}
