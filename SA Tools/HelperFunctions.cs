using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SASplit
{
	public static class HelperFunctions
	{
		static readonly System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();

		public static string FileHash(string path) { return FileHash(File.ReadAllBytes(path)); }

		public static string FileHash(byte[] file)
		{
			file = md5.ComputeHash(file);
			string result = string.Empty;
			foreach(byte item in file)
				result += item.ToString("x2");
			return result;
		}

		private static readonly Encoding jpenc = Encoding.GetEncoding(932);
		private static readonly Encoding euenc = Encoding.GetEncoding(1252);

		public static Encoding GetEncoding() { return jpenc; }

		public static Encoding GetEncoding(Languages language) => GetEncoding(Game.SA1, language);

		public static Encoding GetEncoding(Game game, Languages language)
		{
			switch(language)
			{
				case Languages.Japanese:
					return jpenc;
				case Languages.English:
					switch(game)
					{
						case Game.SA1:
						case Game.SADX:
							return jpenc;
						case Game.SA2:
						case Game.SA2B:
							return euenc;
					}
					throw new ArgumentOutOfRangeException("game");
				default:
					return euenc;
			}
		}

		public static string UnescapeNewlines(this string line)
		{
			StringBuilder sb = new StringBuilder(line.Length);
			for(int c = 0; c < line.Length; c++)
				switch(line[c])
				{
					case '\\': // escape character
						if(c + 1 == line.Length)
							goto default;
						c++;
						switch(line[c])
						{
							case 'n': // line feed
								sb.Append('\n');
								break;
							case 'r': // carriage return
								sb.Append('\r');
								break;
							default: // literal character
								sb.Append(line[c]);
								break;
						}
						break;
					default:
						sb.Append(line[c]);
						break;
				}
			return sb.ToString();
		}

		public static string EscapeNewlines(this string line)
		{
			return line.Replace(@"\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r");
		}

		public static string ToCHex(this ushort i)
		{
			if(i < 10)
				return i.ToString(NumberFormatInfo.InvariantInfo);
			else
				return "0x" + i.ToString("X");
		}

		public static string ToC(this string str) => str.ToC(Languages.Japanese);

		public static string ToC(this string str, Languages language) => ToC(str, Game.SA1, language);

		public static string ToC(this string str, Game game, Languages language)
		{
			if(str == null)
				return "NULL";
			Encoding enc = GetEncoding(game, language);
			StringBuilder result = new StringBuilder("\"");
			foreach(char item in str)
			{
				if(item == '\0')
					result.Append(@"\0");
				else if(item == '\a')
					result.Append(@"\a");
				else if(item == '\b')
					result.Append(@"\b");
				else if(item == '\f')
					result.Append(@"\f");
				else if(item == '\n')
					result.Append(@"\n");
				else if(item == '\r')
					result.Append(@"\r");
				else if(item == '\t')
					result.Append(@"\t");
				else if(item == '\v')
					result.Append(@"\v");
				else if(item == '"')
					result.Append(@"\""");
				else if(item == '\\')
					result.Append(@"\\");
				else if(item < ' ')
					result.AppendFormat(@"\{0}", Convert.ToString((short)item, 8).PadLeft(3, '0'));
				else if(item > '\x7F')
					foreach(byte b in enc.GetBytes(item.ToString()))
						result.AppendFormat(@"\{0}", Convert.ToString(b, 8).PadLeft(3, '0'));
				else
					result.Append(item);
			}
			result.Append("\"");
			return result.ToString();
		}

		public static string ToComment(this string str)
		{
			return "/* " + str.ToCNoEncoding().Replace("*/", @"*\/") + " */";
		}

		public static string ToCNoEncoding(this string str)
		{
			if(str == null)
				return "NULL";
			StringBuilder result = new StringBuilder("\"");
			foreach(char item in str)
			{
				if(item == '\0')
					result.Append(@"\0");
				else if(item == '\a')
					result.Append(@"\a");
				else if(item == '\b')
					result.Append(@"\b");
				else if(item == '\f')
					result.Append(@"\f");
				else if(item == '\n')
					result.Append(@"\n");
				else if(item == '\r')
					result.Append(@"\r");
				else if(item == '\t')
					result.Append(@"\t");
				else if(item == '\v')
					result.Append(@"\v");
				else if(item == '"')
					result.Append(@"\""");
				else if(item == '\\')
					result.Append(@"\\");
				else if(item < ' ')
					result.AppendFormat(@"\{0}", Convert.ToString((short)item, 8).PadLeft(3, '0'));
				else
					result.Append(item);
			}
			result.Append("\"");
			return result.ToString();
		}

		public static string ToC<T>(this T item)
			where T : Enum
		{
			return item.ToC(typeof(T).Name);
		}

		public static string ToC<T>(this T item, string enumname)
			where T : Enum
		{
			Type type = typeof(T);
			if(type.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0)
				if(Enum.IsDefined(typeof(T), item))
					return enumname + "_" + item.ToString();
				else
					return item.ToString();
			else
			{
				ulong num = Convert.ToUInt64(item);
				ulong[] values = Array.ConvertAll((T[])Enum.GetValues(type), (a) => Convert.ToUInt64(a));
				int num2 = values.Length - 1;
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = true;
				ulong num3 = num;
				while(num2 >= 0 && (num2 != 0 || values[num2] != 0uL))
				{
					if((num & values[num2]) == values[num2])
					{
						num -= values[num2];
						if(!flag)
							stringBuilder.Insert(0, " | ");
						stringBuilder.Insert(0, enumname + "_" + Enum.GetName(type, values[num2]));
						flag = false;
					}
					num2--;
				}
				if(num != 0uL)
				{
					if(flag)
						return item.ToString();
					else
						return stringBuilder.ToString() + " | " + item.ToString();
				}
				if(num3 != 0uL)
					return stringBuilder.ToString();
				if(values.Length > 0 && values[0] == 0uL)
					return enumname + "_" + Enum.GetName(type, 0);
				return "0";
			}
		}
	}
}