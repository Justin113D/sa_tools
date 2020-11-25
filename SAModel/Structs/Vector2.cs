using Reloaded.Memory.Streams.Writers;
using System;
using System.IO;
using static SonicRetro.SACommon.ByteConverter;
using static SonicRetro.SACommon.StringExtensions;

namespace SonicRetro.SAModel.Structs
{
	public struct Vector2 : IDataStructOut
	{
		/// <summary>
		/// Equal to (1, 0)
		/// </summary>
		public static readonly Vector2 UnitX = new Vector2(1, 0);

		/// <summary>
		/// Equal to (0, 1)
		/// </summary>
		public static readonly Vector2 UnitY = new Vector2(0, 1);

		/// <summary>
		/// Equal to (0, 0)
		/// </summary>
		public static readonly Vector2 Zero = new Vector2(0, 0);

		public float X;
		public float Y;

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public Vector2(Vector3 vec3)
		{
			X = vec3.X;
			Y = vec3.Y;
		}

		public float this[int index]
		{
			get
			{
				switch(index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					default:
						throw new IndexOutOfRangeException($"Index {index} out of range");
				}
			}
			set
			{
				switch(index)
				{
					case 0:
						X = value;
						return;
					case 1:
						Y = value;
						return;
					default:
						throw new IndexOutOfRangeException($"Index {index} out of range");
				}
			}
		}

		/// <summary>
		/// Calculates the distance between two points
		/// </summary>
		/// <param name="l"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		public static float Distance(Vector2 l, Vector2 r)
		{
			return (float)Math.Sqrt(Math.Pow(l.X - r.X, 2) + Math.Pow(l.Y - l.Y, 2));
		}

		/// <summary>
		/// Linear interpolation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
		{
			return b * t + a * (1 - t);
		}

		/// <summary>
		/// Reads a vector2 from a byte source
		/// </summary>
		/// <param name="source">Byte source</param>
		/// <param name="address">Address at which the vector2 object is located</param>
		/// <param name="type">How the bytes should be read</param>
		/// <returns></returns>
		public static Vector2 Read(byte[] source, ref uint address, IOType type)
		{
			Vector2 result;
			switch(type)
			{
				case IOType.Short:
					result = new Vector2()
					{
						X = source.ToInt16(address),
						Y = source.ToInt16(address + 2)
					};
					address += 4;
					break;
				case IOType.Float:
					result = new Vector2()
					{
						X = source.ToSingle(address),
						Y = source.ToSingle(address + 4)
					};
					address += 8;
					break;
				default:
					throw new ArgumentException($"{type} is not available for Vector2");
			}
			return result;
		}

		public void Write(EndianMemoryStream writer, IOType type)
		{
			switch(type)
			{
				case IOType.Short:
					writer.WriteInt16((short)X);
					writer.WriteInt16((short)Y);
					break;
				case IOType.Float:
					writer.WriteSingle(X);
					writer.WriteSingle(Y);
					break;
				default:
					throw new ArgumentException($"Type {type} not available for Vector2");
			}
		}

		public void WriteNJA(TextWriter writer, IOType type)
		{
			writer.Write("( ");
			switch(type)
			{
				case IOType.Short:
					writer.Write((short)X);
					writer.Write(", ");
					writer.Write((short)Y);
					break;
				case IOType.Float:
					writer.Write(X.ToC());
					writer.Write(", ");
					writer.Write(Y.ToC());
					break;
				default:
					throw new ArgumentException($"Type {type} not available for Vector2");
			}
			writer.Write(")");
		}

		public override string ToString() => $"({X}, {Y})";

		// arithmetic operators
		public static Vector2 operator -(Vector2 l) => new Vector2(-l.X, -l.Y);
		public static Vector2 operator +(Vector2 l, Vector2 r) => new Vector2(l.X + r.X, l.Y + r.Y);
		public static Vector2 operator -(Vector2 l, Vector2 r) => l + (-r);
		public static float operator *(Vector2 l, Vector2 r) => l.X * r.X + l.Y * r.Y;
		public static Vector2 operator *(Vector2 l, float r) => new Vector2(l.X * r, l.Y * r);
		public static Vector2 operator *(float l, Vector2 r) => r * l;
		public static Vector2 operator /(Vector2 l, float r) => l * (1 / r);

		// logical operators
		public override bool Equals(object obj)
		{
			return obj is Vector2 vector &&
				   X == vector.X &&
				   Y == vector.Y;
		}

		public override int GetHashCode()
		{
			var hashCode = 1861411795;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Vector2 l, Vector2 r) => l.Equals(r);
		public static bool operator !=(Vector2 l, Vector2 r) => !l.Equals(r);
	}
}
