﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster
{
	public class Message
	{
		public string Sender { get; set; }
		public DateTime Time {  get; set; }
		public string Text { get; set; }
	}
}
