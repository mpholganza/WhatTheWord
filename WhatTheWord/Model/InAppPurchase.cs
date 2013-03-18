using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatTheWord.Model
{
	public class InAppPurchase
	{
		public string BundleId { get; set; }
		public double Price { get; set; }
		public int Coins { get; set; }
		public int Order { get; set; }
		public string Name { get; set; }
		public string Discount { get; set; }
	}
}
