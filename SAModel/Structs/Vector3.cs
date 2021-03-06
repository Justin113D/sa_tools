﻿using Reloaded.Memory.Streams.Writers;
using System;
using System.IO;
using static SonicRetro.SACommon.ByteConverter;
using static SonicRetro.SACommon.MathHelper;
using static SonicRetro.SACommon.StringExtensions;

namespace SonicRetro.SAModel.Structs
{
	public struct Vector3 : IDataStructOut
	{
		/// <summary>
		/// Equal to (1, 0, 0)
		/// </summary>
		public static readonly Vector3 UnitX = new Vector3(1, 0, 0);

		/// <summary>
		/// Equal to (0, 1, 0)
		/// </summary>
		public static readonly Vector3 UnitY = new Vector3(0, 1, 0);

		/// <summary>
		/// Equal to (0, 0, 1)
		/// </summary>
		public static readonly Vector3 UnitZ = new Vector3(0, 0, 1);

		/// <summary>
		/// Equal to (1, 1, 1)
		/// </summary>
		public static readonly Vector3 One = new Vector3(1, 1, 1);

		/// <summary>
		/// Equal to (0, 0, 0)
		/// </summary>
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);

		public float X;
		public float Y;
		public float Z;

		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3(Vector2 vec2)
		{
			X = vec2.X;
			Y = vec2.X;
			Z = 0;
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
					case 2:
						return Z;
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
					case 2:
						Z = value;
						return;
					default:
						throw new IndexOutOfRangeException($"Index {index} out of range");
				}
			}
		}

		public float Length => (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));

		/// <summary>
		/// Returns the greatest of the 3 values in the vector
		/// </summary>
		public float GreatestValue
		{
			get
			{
				float r = X;
				if(Y > r)
					r = Y;
				if(Z > r)
					r = Z;
				return r;
			}
		}

		public Vector3 Normalized() => this / Length;

		public Vector3 Rounded(int floatingPoint) => new Vector3((float)Math.Round(X, floatingPoint), (float)Math.Round(Y, floatingPoint), (float)Math.Round(Z, floatingPoint));

		/// <summary>
		/// Calculates the distance between 2 points
		/// </summary>
		/// <param name="l"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		public static float Distance(Vector3 l, Vector3 r)
		{
			return (float)Math.Sqrt(Math.Pow(l.X - r.X, 2) + Math.Pow(l.Y - l.Y, 2) + Math.Pow(l.Z - l.Z, 2));
		}

		/// <summary>
		/// Calculates the average position of a collection of points
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Vector3 Average(Vector3[] points)
		{
			Vector3 center = new Vector3();

			if(points == null || points.Length == 0)
				return center;

			foreach(Vector3 p in points)
				center += p;

			return center / points.Length;
		}

		/// <summary>
		/// Calculates the center of the bounds of a list of points
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Vector3 Center(Vector3[] points)
		{
			if(points == null || points.Length == 0)
				return new Vector3();

			Vector3 Positive = points[0];
			Vector3 Negative = points[0];

			foreach(Vector3 p in points)
			{
				if(p.X > Positive.X)
					Positive.X = p.X;
				if(p.Y > Positive.Y)
					Positive.Y = p.Y;
				if(p.Z > Positive.Z)
					Positive.Z = p.Z;

				if(p.X < Negative.X)
					Negative.X = p.X;
				if(p.Y < Negative.Y)
					Negative.Y = p.Y;
				if(p.Z < Negative.Z)
					Negative.Z = p.Z;
			}

			return (Positive + Negative) / 2;
		}

		/// <summary>
		/// Linear interpolation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return b * t + a * (1 - t);
		}

		/// <summary>
		/// Reads a vector3 object from a byte array
		/// </summary>
		/// <param name="source">Byte source</param>
		/// <param name="address">Address at which the vector3 is located</param>
		/// <param name="type">How the bytes should be read</param>
		/// <returns></returns>
		public static Vector3 Read(byte[] source, ref uint address, IOType type)
		{
			Vector3 result;
			switch(type)
			{
				case IOType.Short:
					result = new Vector3()
					{
						X = source.ToInt16(address),
						Y = source.ToInt16(address + 2),
						Z = source.ToInt16(address + 4)
					};
					address += 6;
					break;
				case IOType.Float:
					result = new Vector3()
					{
						X = source.ToSingle(address),
						Y = source.ToSingle(address + 4),
						Z = source.ToSingle(address + 8)
					};
					address += 12;
					break;
				case IOType.BAMS16:
					result = new Vector3()
					{
						X = BAMSToDeg(source.ToInt16(address)),
						Y = BAMSToDeg(source.ToInt16(address + 2)),
						Z = BAMSToDeg(source.ToInt16(address + 4))
					};
					address += 6;
					break;
				case IOType.BAMS32:
					result = new Vector3()
					{
						X = BAMSToDeg(source.ToInt32(address)),
						Y = BAMSToDeg(source.ToInt32(address + 4)),
						Z = BAMSToDeg(source.ToInt32(address + 8))
					};
					address += 12;
					break;
				default:
					throw new ArgumentException($"Type {type} not available for struct Vector3");
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
					writer.WriteInt16((short)Z);
					break;
				case IOType.Float:
					writer.WriteSingle(X);
					writer.WriteSingle(Y);
					writer.WriteSingle(Z);
					break;
				case IOType.BAMS16:
					writer.WriteInt16((short)DegToBAMS(X));
					writer.WriteInt16((short)DegToBAMS(Y));
					writer.WriteInt16((short)DegToBAMS(Z));
					break;
				case IOType.BAMS32:
					writer.WriteInt32(DegToBAMS(X));
					writer.WriteInt32(DegToBAMS(Y));
					writer.WriteInt32(DegToBAMS(Z));
					break;
				default:
					throw new ArgumentException($"Type {type} not available for struct Vector3");
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
					writer.Write(", ");
					writer.Write((short)Z);
					break;
				case IOType.Float:
					writer.Write(X.ToC());
					writer.Write(", ");
					writer.Write(Y.ToC());
					writer.Write(", ");
					writer.Write(Z.ToC());
					break;
				case IOType.BAMS16:
					writer.Write(((short)DegToBAMS(X)).ToCHex());
					writer.Write(", ");
					writer.Write(((short)DegToBAMS(Y)).ToCHex());
					writer.Write(", ");
					writer.Write(((short)DegToBAMS(Z)).ToCHex());
					break;
				case IOType.BAMS32:
					writer.Write(DegToBAMS(X).ToCHex());
					writer.Write(", ");
					writer.Write(DegToBAMS(Y).ToCHex());
					writer.Write(", ");
					writer.Write(DegToBAMS(Z).ToCHex());
					break;
				default:
					throw new ArgumentException($"Type {type} not available for Vector2");
			}
			writer.Write(")");
		}

		public override string ToString() => $"({X}, {Y}, {Z})";

		// arithmetic operand
		public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);
		public static Vector3 operator +(Vector3 l, Vector3 r) => new Vector3(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
		public static Vector3 operator -(Vector3 l, Vector3 r) => l + (-r);
		public static float operator *(Vector3 l, Vector3 r) => l.X * r.X + l.Y * r.Y + l.Z * r.Z;
		public static Vector3 operator *(Vector3 l, float r) => new Vector3(l.X * r, l.Y * r, l.Z * r);
		public static Vector3 operator *(float l, Vector3 r) => r * l;
		public static Vector3 operator /(Vector3 l, float r) => l * (1 / r);

		// logical operators
		public override bool Equals(object obj)
		{
			return obj is Vector3 vector &&
				   X == vector.X &&
				   Y == vector.Y &&
				   Z == vector.Z;
		}

		public override int GetHashCode()
		{
			var hashCode = -307843816;
			hashCode = hashCode * -1521134295 + X.GetHashCode();
			hashCode = hashCode * -1521134295 + Y.GetHashCode();
			hashCode = hashCode * -1521134295 + Z.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Vector3 l, Vector3 r) => l.Equals(r);
		public static bool operator !=(Vector3 l, Vector3 r) => !l.Equals(r);

	}
}
