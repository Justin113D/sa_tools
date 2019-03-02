using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SAModel.Direct3D
{
	public abstract class Renderer
	{
		public abstract void EnableLight(int index, bool enable);
		public abstract void SetLight(int index, Light light);
	}

	public struct Light
	{
		public LightType Type;
		public System.Drawing.Color Diffuse;
		public System.Drawing.Color Specular;
		public System.Drawing.Color Ambient;
		public Vector3 Position;
		public Vector3 Direction;
		public float Range;
		public float Falloff;
		public float Attenuation0;
		public float Attenuation1;
		public float Attenuation2;
		public float Theta;
		public float Phi;
	}

	public enum LightType
	{
		Point = 1,
		Spot = 2,
		Directional = 3
	}
}
