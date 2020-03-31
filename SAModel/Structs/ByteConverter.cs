using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SonicRetro.SAModel
{
	/// <summary>
	/// Converts between numbers and bytes
	/// </summary>
	[DebuggerNonUserCode]
	public static class ByteConverter
	{
		public static bool BigEndian { get; set; }

		public static byte[] GetBytes(ushort value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(short value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(uint value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[3], y[2], y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(int value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[3], y[2], y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(ulong value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[7], y[6], y[5], y[4], y[3], y[2], y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(long value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[7], y[6], y[5], y[4], y[3], y[2], y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(float value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[3], y[2], y[1], y[0] };
			return y;
		}

		public static byte[] GetBytes(double value)
		{
			byte[] y = BitConverter.GetBytes(value);
			if (BigEndian) y = new byte[] { y[7], y[6], y[5], y[4], y[3], y[2], y[1], y[0] };
			return y;
		}

		public static ushort ToUInt16(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex + 1], value[startIndex] }
				: new byte[] { value[startIndex], value[++startIndex] };
			return BitConverter.ToUInt16(y, 0);
		}

		public static short ToInt16(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex + 1], value[startIndex] }
				: new byte[] { value[startIndex], value[++startIndex] };
			return BitConverter.ToInt16(y, 0);
		}

		public static uint ToUInt32(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex] }
				: new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
			return BitConverter.ToUInt32(y, 0);
		}

		public static int ToInt32(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex] }
				: new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
			return BitConverter.ToInt32(y, 0);
		}

		public static ulong ToUInt64(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex] }
				: new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
			return BitConverter.ToUInt64(y, 0);
		}

		public static long ToInt64(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex] }
				: new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
			return BitConverter.ToInt64(y, 0);
		}

		public static float ToSingle(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex] }
				: new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
			return BitConverter.ToSingle(y, 0);
		}

		public static double ToDouble(byte[] value, uint startIndex)
		{
			byte[] y = BigEndian
				? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex] }
				: new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
			return BitConverter.ToDouble(y, 0);
		}
	}

	/// <summary>
	/// A writer class to support big and small endian <br/>
	/// Uses the <see cref="ByteConverter"/> and thus the <see cref="ByteConverter.BigEndian"/> variable
	/// </summary>
	public class ByteWriter : BinaryWriter
	{
		public ByteWriter()
			: base() { }

		public ByteWriter(Stream output)
			: base(output) { }

		public ByteWriter(Stream output, Encoding encoding)
			: base(output, encoding) { }

		public uint Position => (uint)BaseStream.Position;

		public override void Write(decimal value)	=> throw new NotSupportedException();

		public override void Write(short value)		=> Write(ByteConverter.GetBytes(value));

		public override void Write(ushort value)	=> Write(ByteConverter.GetBytes(value));

		public override void Write(int value)		=> Write(ByteConverter.GetBytes(value));

		public override void Write(uint value)		=> Write(ByteConverter.GetBytes(value));

		public override void Write(float value)		=> Write(ByteConverter.GetBytes(value));

		public override void Write(long value)		=> Write(ByteConverter.GetBytes(value));

		public override void Write(ulong value)		=> Write(ByteConverter.GetBytes(value));

		public override void Write(double value)	=> Write(ByteConverter.GetBytes(value));
	}
}
