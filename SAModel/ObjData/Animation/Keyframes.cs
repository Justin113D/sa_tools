﻿using Reloaded.Memory.Streams.Writers;
using SonicRetro.SAModel.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using static SonicRetro.SACommon.ByteConverter;
using static SonicRetro.SACommon.MathHelper;

namespace SonicRetro.SAModel.ObjData.Animation
{

	/// <summary>
	/// Keyframe storage for an animation
	/// </summary>
	public class Keyframes
	{

		/// <summary>
		/// Transform position keyframes
		/// </summary>
		public SortedDictionary<uint, Vector3> Position { get; private set; }

		/// <summary>
		/// Transform rotation keyframes
		/// </summary>
		public SortedDictionary<uint, Vector3> Rotation { get; private set; }

		/// <summary>
		/// Transform scale keyframes
		/// </summary>
		public SortedDictionary<uint, Vector3> Scale { get; private set; }

		/// <summary>
		/// General vector3 keyframes
		/// </summary>
		public SortedDictionary<uint, Vector3> Vector { get; private set; }

		/// <summary>
		/// Mesh vertex positions
		/// </summary>
		public SortedDictionary<uint, Vector3[]> Vertex { get; private set; }

		/// <summary>
		/// Mesh vertex normals
		/// </summary>
		public SortedDictionary<uint, Vector3[]> Normal { get; private set; }

		/// <summary>
		/// Camera lookat target
		/// </summary>
		public SortedDictionary<uint, Vector3> Target { get; private set; }

		/// <summary>
		/// Camera Roll
		/// </summary>
		public SortedDictionary<uint, float> Roll { get; private set; }

		/// <summary>
		/// Camera Fov
		/// </summary>
		public SortedDictionary<uint, float> Angle { get; private set; }

		/// <summary>
		/// Light Color
		/// </summary>
		public SortedDictionary<uint, Color> LightColor { get; private set; }

		/// <summary>
		/// Light intensity
		/// </summary>
		public SortedDictionary<uint, float> Intensity { get; private set; }

		/// <summary>
		/// Spotlight data
		/// </summary>
		public SortedDictionary<uint, Spotlight> Spot { get; private set; }

		/// <summary>
		/// Point light stuff
		/// </summary>
		public SortedDictionary<uint, Vector2> Point { get; private set; }

		private IEnumerable<uint>[] Frames
		{
			get
			{
				IEnumerable<uint>[] result = new IEnumerable<uint>[13];
				result[0] = Position.Keys;
				result[1] = Rotation.Keys;
				result[2] = Scale.Keys;
				result[3] = Vector.Keys;
				result[4] = Vertex.Keys;
				result[5] = Normal.Keys;
				result[6] = Target.Keys;
				result[7] = Roll.Keys;
				result[8] = Angle.Keys;
				result[9] = LightColor.Keys;
				result[10] = Intensity.Keys;
				result[11] = Spot.Keys;
				result[12] = Point.Keys;
				return result;
			}
		}

		/// <summary>
		/// Whether any keyframes exist in this keyframe set
		/// </summary>
		public bool HasKeyframes => Frames.Any(x => x.Count() > 0);

		/// <summary>
		/// Returns the last keyframe number across all sets / maximum keyframe size
		/// </summary>
		public uint KeyframeCount
		{
			get
			{
				uint count = 0;
				foreach(var v in Frames)
				{
					if(v.Count() == 0)
						continue;
					uint val = v.Last();
					if(val > count)
						count = val;
				}
				return count + 1;
			}
		}

		/// <summary>
		/// Channels that are used
		/// </summary>
		public AnimFlags Type
		{
			get
			{
				AnimFlags flags = 0;

				if(Position.Count > 0)
					flags |= AnimFlags.Position;
				if(Rotation.Count > 0)
					flags |= AnimFlags.Rotation;
				if(Scale.Count > 0)
					flags |= AnimFlags.Scale;
				if(Vector.Count > 0)
					flags |= AnimFlags.Vector;
				if(Vertex.Count > 0)
					flags |= AnimFlags.Vertex;
				if(Normal.Count > 0)
					flags |= AnimFlags.Normal;
				if(Target.Count > 0)
					flags |= AnimFlags.Target;
				if(Roll.Count > 0)
					flags |= AnimFlags.Roll;
				if(Angle.Count > 0)
					flags |= AnimFlags.Angle;
				if(LightColor.Count > 0)
					flags |= AnimFlags.LightColor;
				if(Intensity.Count > 0)
					flags |= AnimFlags.Intensity;
				if(Spot.Count > 0)
					flags |= AnimFlags.Spot;
				if(Point.Count > 0)
					flags |= AnimFlags.Point;

				return flags;
			}
		}

		/// <summary>
		/// Creates an empty keyframe storage
		/// </summary>
		public Keyframes()
		{
			Position = new SortedDictionary<uint, Vector3>();
			Rotation = new SortedDictionary<uint, Vector3>();
			Scale = new SortedDictionary<uint, Vector3>();
			Vector = new SortedDictionary<uint, Vector3>();
			Vertex = new SortedDictionary<uint, Vector3[]>();
			Normal = new SortedDictionary<uint, Vector3[]>();
			Target = new SortedDictionary<uint, Vector3>();
			Roll = new SortedDictionary<uint, float>();
			Angle = new SortedDictionary<uint, float>();
			LightColor = new SortedDictionary<uint, Color>();
			Intensity = new SortedDictionary<uint, float>();
			Spot = new SortedDictionary<uint, Spotlight>();
			Point = new SortedDictionary<uint, Vector2>();
		}


		/// <summary>
		/// Searches through a keyframe dictionary and returns the interpolation between the values last and next. <br/>
		/// If the returned float is 0, then next will be default (as its not used)
		/// </summary>
		/// <typeparam name="T">Type of the Keyframe values</typeparam>
		/// <param name="keyframes">KEyframes to iterate through</param>
		/// <param name="frame">Current frame to get</param>
		/// <param name="before">Last Keyframe before given frame</param>
		/// <param name="next">Next Keyframe after given frame</param>
		/// <returns></returns>
		public static float GetNearestFrames<T>(SortedDictionary<uint, T> keyframes, float frame, out T before, out T next)
		{
			if(frame < 0)
				frame = 0;

			// if there is only one frame, we can take that one
			next = default;

			if(keyframes.Count == 1)
				foreach(T val in keyframes.Values) // faster than converting to an array and accessing the first index
				{
					before = val;
					return 0;
				}

			// if the given frame is spot on and exists, then we can use it
			uint baseFrame = (uint)Math.Floor(frame);
			if(frame == baseFrame && keyframes.ContainsKey(baseFrame))
			{
				before = keyframes[baseFrame];
				return 0;
			}

			// we gotta find the frames that the given frame is between
			// this is pretty easy thanks to the fact that the dictionary is always sorted

			uint nextSmallestFrame = 0;

			// getting the first frame index
			var keys = keyframes.Keys;
			foreach(uint key in keys) // faster than converting to an array and accessing the first index
			{
				nextSmallestFrame = key;
				break;
			}

			// if the smallest frame is greater than the frame we are at right now, then we can just return the frame
			if(nextSmallestFrame > baseFrame)
			{
				before = keyframes[nextSmallestFrame];
				return 0;
			}

			// getting the actual next smallest and biggest frames
			uint nextBiggestFrame = baseFrame;
			foreach(uint key in keyframes.Keys)
			{
				if(key > nextSmallestFrame && key <= baseFrame)
					nextSmallestFrame = key;
				else if(key > baseFrame)
				{
					// the first bigger value must be the next biggest frame
					nextBiggestFrame = key;
					break;
				}
			}

			// if the next biggest frame hasnt changed, then that means we are past the last frame
			before = keyframes[nextSmallestFrame];
			if(nextBiggestFrame == baseFrame)
				return 0;

			// the regular result
			next = keyframes[nextBiggestFrame];

			// getting the interpolation between the two frames
			float duration = nextBiggestFrame - nextSmallestFrame;
			return (frame - nextSmallestFrame) / duration;
		}

		private static Vector3 ValueAtFrame(SortedDictionary<uint, Vector3> keyframes, float frame)
		{
			float interpolation = GetNearestFrames(keyframes, frame, out Vector3 before, out Vector3 next);
			if(interpolation == 0)
				return before;
			else
				return Vector3.Lerp(before, next, interpolation);
		}

		private static Vector3[] ValueAtFrame(SortedDictionary<uint, Vector3[]> keyframes, float frame)
		{
			float interpolation = GetNearestFrames(keyframes, frame, out Vector3[] before, out Vector3[] next);
			if(interpolation == 0)
				return (Vector3[])before.Clone();

			Vector3[] result = new Vector3[before.Length];
			for(int i = 0; i < result.Length; i++)
			{
				result[i] = Vector3.Lerp(before[i], next[i], interpolation);
			}
			return result;
		}

		private static Vector2 ValueAtFrame(SortedDictionary<uint, Vector2> keyframes, float frame)
		{
			float interpolation = GetNearestFrames(keyframes, frame, out Vector2 before, out Vector2 next);
			if(interpolation == 0)
				return before;
			else
				return Vector2.Lerp(before, next, interpolation);
		}

		private static Color ValueAtFrame(SortedDictionary<uint, Color> keyframes, float frame)
		{
			float interpolation = GetNearestFrames(keyframes, frame, out Color before, out Color next);
			if(interpolation == 0)
				return before;
			else
				return Color.Lerp(before, next, interpolation);
		}

		private static float ValueAtFrame(SortedDictionary<uint, float> keyframes, float frame)
		{
			float interpolation = GetNearestFrames(keyframes, frame, out float before, out float next);
			if(interpolation == 0)
				return before;
			else
				return next * interpolation + before * (1 - interpolation);
		}

		private static Spotlight ValueAtFrame(SortedDictionary<uint, Spotlight> keyframes, float frame)
		{
			float interpolation = GetNearestFrames(keyframes, frame, out Spotlight before, out Spotlight next);
			if(interpolation == 0)
				return before;
			else
				return Spotlight.Lerp(before, next, interpolation);
		}

		/// <summary>
		/// Returns a all values at a specific frame
		/// </summary>
		/// <param name="frame">Frame to get the values of</param>
		/// <returns></returns>
		public Frame GetFrameAt(float frame)
		{
			Frame result = new Frame()
			{
				frame = frame,
			};

			if(Position.Count > 0)
				result.position = ValueAtFrame(Position, frame);

			if(Rotation.Count > 0)
				result.rotation = ValueAtFrame(Rotation, frame);

			if(Scale.Count > 0)
				result.scale = ValueAtFrame(Scale, frame);

			if(Vector.Count > 0)
				result.vector = ValueAtFrame(Vector, frame);

			if(Vertex.Count > 0)
				result.vertex = ValueAtFrame(Vertex, frame);

			if(Normal.Count > 0)
				result.normal = ValueAtFrame(Normal, frame);

			if(Target.Count > 0)
				result.target = ValueAtFrame(Target, frame);

			if(Roll.Count > 0)
				result.roll = ValueAtFrame(Roll, frame);

			if(Angle.Count > 0)
				result.angle = ValueAtFrame(Angle, frame);

			if(LightColor.Count > 0)
				result.color = ValueAtFrame(LightColor, frame);

			if(Intensity.Count > 0)
				result.Intensity = ValueAtFrame(Intensity, frame);

			if(Spot.Count > 0)
				result.spot = ValueAtFrame(Spot, frame);

			if(Point.Count > 0)
				result.point = ValueAtFrame(Point, frame);

			return result;
		}


		private static void ReadVector3Set(byte[] source, uint address, uint count, SortedDictionary<uint, Vector3> dictionary, IOType type)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = source.ToUInt32(address);
				address += 4;
				dictionary.Add(frame, Vector3.Read(source, ref address, type));
			}
		}

		private static void ReadVector3ArraySet(byte[] source, uint address, uint imageBase, uint count, SortedDictionary<uint, Vector3[]> dictionary)
		{
			uint origAddr = address;

			// <address, frame>
			SortedDictionary<uint, uint> ptrs = new SortedDictionary<uint, uint>();
			for(int i = 0; i < count; i++)
			{
				uint frame = source.ToUInt32(address);
				uint ptr = source.ToUInt32(address + 4) - imageBase;
				address += 8;

				ptrs.Add(ptr, frame);
			}

			if(ptrs.Count == 0)
				return;

			var values = ptrs.ToArray();
			uint size = (origAddr - values[values.Length - 1].Key) / 12;
			for(int i = values.Length - 1; i >= 0; i--)
			{
				uint ptr = values[i].Key;
				Vector3[] vectors = new Vector3[size];
				for(int j = 0; j < size; j++)
					vectors[j] = Vector3.Read(source, ref ptr, IOType.Float);
				dictionary.Add(values[i].Value, vectors);
			}
		}

		private static void ReadVector2Set(byte[] source, uint address, uint count, SortedDictionary<uint, Vector2> dictionary, IOType type)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = source.ToUInt32(address);
				address += 4;
				dictionary.Add(frame, Vector2.Read(source, ref address, type));
			}
		}

		private static void ReadColorSet(byte[] source, uint address, uint count, SortedDictionary<uint, Color> dictionary, IOType type)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = source.ToUInt32(address);
				address += 4;
				dictionary.Add(frame, Color.Read(source, ref address, type));
			}
		}

		private static void ReadFloatSet(byte[] source, uint address, uint count, SortedDictionary<uint, float> dictionary, bool BAMS)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = source.ToUInt32(address);
				float value = BAMS ? BAMSToDeg(source.ToInt32(address + 4)) : source.ToSingle(address + 4);
				address += 8;
				dictionary.Add(frame, value);
			}
		}

		private static void ReadSpotSet(byte[] source, uint address, uint count, SortedDictionary<uint, Spotlight> dictionary)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = source.ToUInt32(address);
				Spotlight value = Spotlight.Read(source, address + 4);
				address += 8 + Spotlight.Size;
				dictionary.Add(frame, value);
			}
		}

		/// <summary>
		/// Reads a set of keyframes from a byte array
		/// </summary>
		/// <param name="source">Byte source</param>
		/// <param name="address">Address at which the keyframes are located</param>
		/// <param name="imageBase">Image base for all addresses</param>
		/// <param name="channels">Channel count (derived from type)</param>
		/// <param name="type">Which channels should be read</param>
		/// <param name="shortRot">Whether to read Rotations using 16Bit BAMS</param>
		/// <returns></returns>
		public static Keyframes Read(byte[] source, ref uint address, uint imageBase, int channels, AnimFlags type, bool shortRot = false)
		{
			uint[] addresses = new uint[channels];
			uint[] frameCounts = new uint[channels];
			for(int i = 0; i < channels; i++)
			{
				uint val = source.ToUInt32(address);
				if(val > 0)
					addresses[i] = val - imageBase;
				address += 4;
			}
			for(int i = 0; i < channels; i++)
			{
				frameCounts[i] = source.ToUInt32(address);
				address += 4;
			}

			int channelIndex = 0;
			Keyframes result = new Keyframes();

			if(type.HasFlag(AnimFlags.Position))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3Set(source, addresses[channelIndex], frameCounts[channelIndex], result.Position, IOType.Float);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Rotation))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3Set(source, addresses[channelIndex], frameCounts[channelIndex], result.Rotation, shortRot ? IOType.BAMS16 : IOType.BAMS32);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Scale))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3Set(source, addresses[channelIndex], frameCounts[channelIndex], result.Scale, IOType.Float);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Vector))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3Set(source, addresses[channelIndex], frameCounts[channelIndex], result.Vector, IOType.Float);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Vertex))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3ArraySet(source, addresses[channelIndex], imageBase, frameCounts[channelIndex], result.Vertex);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Normal))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3ArraySet(source, addresses[channelIndex], imageBase, frameCounts[channelIndex], result.Normal);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Target))
			{
				if(addresses[channelIndex] != 0)
					ReadVector3Set(source, addresses[channelIndex], frameCounts[channelIndex], result.Target, IOType.Float);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Roll))
			{
				if(addresses[channelIndex] != 0)
					ReadFloatSet(source, addresses[channelIndex], frameCounts[channelIndex], result.Roll, true);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Angle))
			{
				if(addresses[channelIndex] != 0)
					ReadFloatSet(source, addresses[channelIndex], frameCounts[channelIndex], result.Angle, true);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.LightColor))
			{
				if(addresses[channelIndex] != 0)
					ReadColorSet(source, addresses[channelIndex], frameCounts[channelIndex], result.LightColor, IOType.ARGB8_32);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Intensity))
			{
				if(addresses[channelIndex] != 0)
					ReadFloatSet(source, addresses[channelIndex], frameCounts[channelIndex], result.Intensity, false);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Spot))
			{
				if(addresses[channelIndex] != 0)
					ReadSpotSet(source, addresses[channelIndex], frameCounts[channelIndex], result.Spot);
				channelIndex++;
			}

			if(type.HasFlag(AnimFlags.Point))
			{
				if(addresses[channelIndex] != 0)
					ReadVector2Set(source, addresses[channelIndex], frameCounts[channelIndex], result.Point, IOType.Float);
			}

			return result;
		}

		/// <summary>
		/// Writes the keyframe set to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		/// <param name="imageBase">Image base for all addresses</param>
		/// <param name="channels">Channel count (derived from type)</param>
		/// <param name="type">Which channels should be written</param>
		/// <param name="shortRot">Whether to write rotation in </param>
		public (uint address, uint count)[] Write(EndianMemoryStream writer, uint imageBase, int channels, AnimFlags type, bool shortRot = false)
		{
			(uint address, uint count)[] keyframeLocs = new (uint address, uint count)[channels];
			int channelIndex = 0;

			bool ContinueWrite(int count, AnimFlags flag)
			{
				if(type.HasFlag(flag))
				{
					if(count == 0)
					{
						channelIndex++;
						return false;
					}
					keyframeLocs[channelIndex] = ((uint)writer.Stream.Position + imageBase, (uint)count);
					channelIndex++;
					return true;
				}
				return false;
			}

			void WriteDataStruct(Dictionary<uint, IDataStructOut> dict, IOType ioType, AnimFlags flag)
			{
				if(!ContinueWrite(dict.Count, flag))
					return;

				foreach(var pair in dict)
				{
					writer.WriteUInt32(pair.Key);
					pair.Value.Write(writer, ioType);
				}
			}

			void WriteVector3Array(SortedDictionary<uint, Vector3[]> dict, AnimFlags flag)
			{
				if(type.HasFlag(flag))
				{
					if(dict.Count == 0)
					{
						channelIndex++;
						return;
					}
					// <frame, ptr>
					Dictionary<uint, uint> ptrs = new Dictionary<uint, uint>();
					foreach(var pair in dict)
					{
						ptrs.Add(pair.Key, (uint)writer.Stream.Position + imageBase);
						foreach(Vector3 v in pair.Value)
							v.Write(writer, IOType.Float);
					}

					keyframeLocs[channelIndex] = ((uint)writer.Stream.Position + imageBase, (uint)dict.Count);

					foreach(var pair in dict)
					{
						writer.WriteUInt32(pair.Key);
						writer.WriteUInt32(ptrs[pair.Key]);
					}

					channelIndex++;
				}
			}

			void WriteFloat(SortedDictionary<uint, float> dict, bool BAMS, AnimFlags flag)
			{
				if(!ContinueWrite(dict.Count, flag))
					return;

				foreach(var pair in dict)
				{
					writer.WriteUInt32(pair.Key);
					if(BAMS)
						writer.WriteInt32(DegToBAMS(pair.Value));
					else
						writer.WriteSingle(pair.Value);
				}
			}

			void WriteSpotlight(SortedDictionary<uint, Spotlight> dict, AnimFlags flag)
			{
				if(!ContinueWrite(dict.Count, flag))
					return;

				foreach(var pair in dict)
				{
					writer.WriteUInt32(pair.Key);
					pair.Value.Write(writer);
				}
			}

			WriteDataStruct(Position.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), IOType.Float, AnimFlags.Position);
			WriteDataStruct(Rotation.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), shortRot ? IOType.BAMS16 : IOType.BAMS32, AnimFlags.Rotation);
			WriteDataStruct(Scale.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), IOType.Float, AnimFlags.Scale);
			WriteDataStruct(Vector.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), IOType.Float, AnimFlags.Vector);
			WriteVector3Array(Vertex, AnimFlags.Vertex);
			WriteVector3Array(Normal, AnimFlags.Normal);
			WriteDataStruct(Target.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), IOType.Float, AnimFlags.Target);
			WriteFloat(Roll, true, AnimFlags.Roll);
			WriteFloat(Angle, true, AnimFlags.Angle);
			WriteDataStruct(LightColor.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), IOType.ARGB8_32, AnimFlags.LightColor);
			WriteFloat(Intensity, false, AnimFlags.Intensity);
			WriteSpotlight(Spot, AnimFlags.Spot);
			WriteDataStruct(Point.ToDictionary(x => x.Key, x => x.Value as IDataStructOut), IOType.Float, AnimFlags.Point);

			return keyframeLocs;
		}
	}

	/// <summary>
	/// Frame on timeline with interpoalted values from a keyframe storage
	/// </summary>
	public struct Frame
	{
		/// <summary>
		/// Position on the timeline
		/// </summary>
		public float frame;

		/// <summary>
		/// Position at the frame
		/// </summary>
		public Vector3? position;

		/// <summary>
		/// Rotation at the frame
		/// </summary>
		public Vector3? rotation;

		/// <summary>
		/// Scale at the frame
		/// </summary>
		public Vector3? scale;

		/// <summary>
		/// Vector at the frame
		/// </summary>
		public Vector3? vector;

		/// <summary>
		/// Vertex positions at the frame
		/// </summary>
		public Vector3[] vertex;

		/// <summary>
		/// Vertex normals at the frame
		/// </summary>
		public Vector3[] normal;

		/// <summary>
		/// Camera target position at the frame
		/// </summary>
		public Vector3? target;

		/// <summary>
		/// Camera roll at the frame
		/// </summary>
		public float? roll;

		/// <summary>
		/// Camera FOV at the frame
		/// </summary>
		public float? angle;

		/// <summary>
		/// Light color at the frame
		/// </summary>
		public Color? color;

		/// <summary>
		/// Light intensity at the frame
		/// </summary>
		public float? Intensity;

		/// <summary>
		/// Spotlight at the frame
		/// </summary>
		public Spotlight? spot;

		/// <summary>
		/// Point light stuff at the frame
		/// </summary>
		public Vector2? point;
	}

	/// <summary>
	/// Spotlight for cutscenes
	/// </summary>
	public struct Spotlight
	{
		public static uint Size => 16;

		/// <summary>
		/// Closest light distance
		/// </summary>
		///
		public float near;

		/// <summary>
		/// Furthest light distance
		/// </summary>
		public float far;

		/// <summary>
		///
		/// </summary>
		public float insideAngle;

		/// <summary>
		///
		/// </summary>
		public float outsideAngle;

		/// <summary>
		/// Linear interpolation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Spotlight Lerp(Spotlight a, Spotlight b, float t)
		{
			float inverse = 1 - t;
			return new Spotlight()
			{
				near = b.near * t + a.near * inverse,
				far = b.far * t + a.far * inverse,
				insideAngle = b.insideAngle * t + a.insideAngle * inverse,
				outsideAngle = b.outsideAngle * t + a.outsideAngle * inverse,
			};
		}

		/// <summary>
		/// Reads a spotlight from a byte array
		/// </summary>
		/// <param name="source"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public static Spotlight Read(byte[] source, uint address)
		{
			return new Spotlight()
			{
				near = source.ToSingle(address),
				far = source.ToSingle(address + 4),
				insideAngle = BAMSToDeg(source.ToInt32(address + 8)),
				outsideAngle = BAMSToDeg(source.ToInt32(address + 12))
			};
		}

		/// <summary>
		/// Writes the spotlight to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		public void Write(EndianMemoryStream writer)
		{
			writer.WriteSingle(near);
			writer.WriteSingle(far);
			writer.WriteInt32(DegToBAMS(insideAngle));
			writer.WriteInt32(DegToBAMS(outsideAngle));
		}
	}
}
