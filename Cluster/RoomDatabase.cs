using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cluster;

namespace Cluster
{
	public class RoomDatabase
	{
		static Dictionary<int, Room> Rooms { get; set; } = new();

		public static void AddRoom(Room room)
		{
			if (Rooms.ContainsKey(room.Id)) return;
			Rooms.Add(room.Id, room);
		}
		public static Room? GetRoom(int id)
		{
			if(Rooms.ContainsKey(id)) return Rooms[id];
			return null;
		}
	}
}
