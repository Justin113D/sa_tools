﻿using Reloaded.Memory.Streams.Writers;
using System;
using System.IO;
using static SonicRetro.SACommon.ByteConverter;

namespace SonicRetro.SAModel.Structs
{
	/// <summary>
	/// RGBA Color value
	/// </summary>
	public struct Color : IDataStructOut
	{
		public static readonly Color White = new Color(0xFF, 0xFF, 0xFF, 0xFF);

		/// <summary>
		/// The red value
		/// </summary>
		public byte R;

		/// <summary>
		/// The green channel
		/// </summary>
		public byte G;

		/// <summary>
		/// The blue value
		/// </summary>
		public byte B;

		/// <summary>
		/// The alpha value
		/// </summary>
		public byte A;


		/// <summary>
		/// Red float value. Ranges from 0 - 1
		/// </summary>
		public float RedF
		{
			get
			{
				return R / 255.0f;
			}
			set
			{
				R = (byte)Math.Round(value * 255);
			}
		}

		/// <summary>
		/// Green float value. Ranges from 0 - 1
		/// </summary>
		public float GreenF
		{
			get
			{
				return G / 255.0f;
			}
			set
			{
				G = (byte)(value * 255);
			}
		}

		/// <summary>
		/// Blue float value. Ranges from 0 - 1
		/// </summary>
		public float BlueF
		{
			get
			{
				return B / 255.0f;
			}
			set
			{
				B = (byte)(value * 255);
			}
		}

		/// <summary>
		/// Alpha float value. Ranges from 0 - 1
		/// </summary>
		public float AlphaF
		{
			get
			{
				return A / 255.0f;
			}
			set
			{
				A = (byte)(value * 255);
			}
		}


		/// <summary>
		/// Returns the color as an RGBA integer
		/// </summary>
		public uint RGBA
		{
			get => (uint)(A | (B << 8) | (G << 16) | (R << 24));
			set
			{
				A = (byte)(value & 0xFF);
				B = (byte)((value >> 8) & 0xFF);
				G = (byte)((value >> 16) & 0xFF);
				R = (byte)(value >> 24);
			}
		}

		/// <summary>
		/// Returns the color as an ARGB integer
		/// </summary>
		public uint ARGB
		{
			get => (uint)(B | (G << 8) | (R << 16) | (A << 24));
			set
			{
				B = (byte)(value & 0xFF);
				G = (byte)((value >> 8) & 0xFF);
				R = (byte)((value >> 16) & 0xFF);
				A = (byte)(value >> 24);
			}
		}

		/// <summary>
		/// Returns the color as an ARGB short
		/// </summary>
		public ushort ARGB4
		{
			get => (ushort)((B >> 4) | (G & 0xF) | ((R << 4) & 0xF) | ((A << 8) & 0xF));
			set
			{
				B = (byte)((value << 4) & 0xF0);
				B |= (byte)(B >> 4);

				G = (byte)(value & 0xF0);
				G |= (byte)(G >> 4);

				R = (byte)((value >> 4) & 0xF0);
				R |= (byte)(R >> 4);

				A = (byte)((value >> 8) & 0xF0);
				A |= (byte)(A >> 4);
			}
		}

		/// <summary>
		/// Returns the color as an RGB565 Short
		/// </summary>
		public ushort RGB565
		{
			get => (ushort)((B >> 3) | ((G << 3) & 0x3F) | ((R << 8) & 0x1F));
			set
			{
				B = (byte)(((value << 3) | (value >> 2)) & 0xFFu);
				G = (byte)(((value >> 3) | (value >> 9)) & 0xFFu);
				R = (byte)(((value >> 8) | (value >> 13)) & 0xFFu);
				A = 0xFF;
			}
		}

		/// <summary>
		/// Color as system color
		/// </summary>
		public System.Drawing.Color SystemCol
		{
			get => System.Drawing.Color.FromArgb(A, R, G, B);
			set
			{
				R = value.R;
				G = value.G;
				B = value.B;
				A = value.A;
			}
		}


		public Color(byte r, byte g, byte b)
		{
			R = r;
			G = g;
			B = b;
			A = 0xFF;
		}

		public Color(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public Color(float r, float g, float b) : this()
		{
			RedF = r;
			GreenF = g;
			BlueF = b;
			A = 0xFF;
		}

		public Color(float r, float g, float b, float a) : this()
		{
			RedF = r;
			GreenF = g;
			BlueF = b;
			AlphaF = a;
		}

		/// <summary>
		/// Linear interpolation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Color Lerp(Color a, Color b, float t)
		{
			float inverse = 1 - t;
			return new Color(
				b.RedF * t + a.RedF * inverse,
				b.GreenF * t + a.GreenF * inverse,
				b.BlueF * t + a.BlueF * inverse,
				b.AlphaF * t + a.AlphaF * inverse
				);
		}

		/// <summary>
		/// Reads a color object from a byte array
		/// </summary>
		/// <param name="source">Byte source</param>
		/// <param name="address">Address at which the vector2 object is located</param>
		/// <param name="type">How the bytes should be read</param>
		/// <returns></returns>
		public static Color Read(byte[] source, ref uint address, IOType type)
		{
			Color col = default;
			switch(type)
			{
				case IOType.RGBA8:
					col.RGBA = source.ToUInt32(address);
					address += 4;
					break;
				case IOType.ARGB8_32:
					col.ARGB = source.ToUInt32(address);
					address += 4;
					break;
				case IOType.ARGB8_16:
					ushort GB = source.ToUInt16(address);
					ushort AR = source.ToUInt16(address + 2);
					col.ARGB = (uint)(GB | (AR << 16));
					address += 4;
					break;
				case IOType.ARGB4:
					col.ARGB4 = source.ToUInt16(address);
					address += 2;
					break;
				case IOType.RGB565:
					col.RGB565 = source.ToUInt16(address);
					address += 2;
					break;
				default:
					throw new ArgumentException($"{type} is not a valid type for Color");
			}
			return col;
		}

		public void WriteNJA(TextWriter writer, IOType type)
		{
			switch(type)
			{
				case IOType.ARGB8_32:
					writer.Write("( ");
					writer.Write(A);
					writer.Write(", ");
					writer.Write(R);
					writer.Write(", ");
					writer.Write(G);
					writer.Write(", ");
					writer.Write(B);
					writer.Write(")");
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void Write(EndianMemoryStream writer, IOType type)
		{
			switch(type)
			{
				case IOType.RGBA8:
					writer.WriteUInt32(RGBA);
					break;
				case IOType.ARGB8_32:
					writer.WriteUInt32(ARGB);
					break;
				case IOType.ARGB8_16:
					uint val = ARGB;
					writer.WriteUInt16((ushort)val);
					writer.WriteUInt16((ushort)(val >> 16));
					break;
				case IOType.ARGB4:
					writer.WriteUInt16(ARGB4);
					break;
				case IOType.RGB565:
					writer.WriteUInt16(RGB565);
					break;
				default:
					throw new ArgumentException($"{type} is not a valid type for Color");
			}
		}

		//public override string ToString() => $"({Math.Round(RedF, 3)}, {Math.Round(GreenF, 3)}, {Math.Round(BlueF, 3)}, {Math.Round(AlphaF, 3)})";

		public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";

		// arithmetic operators
		public static Color operator +(Color l, Color r)
		{
			return new Color()
			{
				R = (byte)Math.Min(l.R + r.R, byte.MaxValue),
				G = (byte)Math.Min(l.G + r.G, byte.MaxValue),
				B = (byte)Math.Min(l.B + r.B, byte.MaxValue),
				A = (byte)Math.Min(l.A + r.A, byte.MaxValue)
			};
		}
		public static Color operator -(Color l, Color r)
		{
			return new Color()
			{
				R = (byte)Math.Max(l.R - r.R, 0),
				G = (byte)Math.Max(l.G - r.G, 0),
				B = (byte)Math.Max(l.B - r.B, 0),
				A = (byte)Math.Max(l.A - r.A, 0)
			};
		}
		public static Color operator *(Color l, float r)
		{
			return new Color()
			{
				R = (byte)Math.Min(Math.Min(l.R * r, byte.MaxValue), 0),
				G = (byte)Math.Min(Math.Min(l.G * r, byte.MaxValue), 0),
				B = (byte)Math.Min(Math.Min(l.B * r, byte.MaxValue), 0),
				A = (byte)Math.Min(Math.Min(l.A * r, byte.MaxValue), 0)
			};
		}
		public static Color operator *(float l, Color r) => r * l;
		public static Color operator /(Color l, float r) => l * (1 / r);

		// logical operators
		public override bool Equals(object obj)
		{
			return obj is Color color &&
				   R == color.R &&
				   G == color.G &&
				   B == color.B &&
				   A == color.A;
		}

		public override int GetHashCode()
		{
			var hashCode = 1960784236;
			hashCode = hashCode * -1521134295 + R.GetHashCode();
			hashCode = hashCode * -1521134295 + G.GetHashCode();
			hashCode = hashCode * -1521134295 + B.GetHashCode();
			hashCode = hashCode * -1521134295 + A.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Color l, Color r) => l.Equals(r);
		public static bool operator !=(Color l, Color r) => !l.Equals(r);
	}
}
