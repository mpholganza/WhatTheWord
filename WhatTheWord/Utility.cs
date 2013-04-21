using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatTheWord
{
	public class Utility
	{
		public static string IntArrayToString(int[] array, string delimiter)
		{
			if (array == null) return "";
			if (array.Length < 1) return "";
			if (array.Length == 1) return array[0].ToString();

			StringBuilder builder = new StringBuilder();

			builder.Append(array[0].ToString());
			for (int i = 1; i < array.Length; i++)
			{
				builder.Append(delimiter);
				builder.Append(array[i].ToString());
			}

			return builder.ToString();
		}
	}
}
