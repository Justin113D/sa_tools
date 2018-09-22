﻿using SharpDX;
using SonicRetro.SAModel;
using SonicRetro.SAModel.Direct3D;
using SonicRetro.SAModel.SAEditorCommon.DataTypes;
using SonicRetro.SAModel.SAEditorCommon.SETEditing;
using System.Collections.Generic;
using BoundingSphere = SonicRetro.SAModel.BoundingSphere;
using Color = System.Drawing.Color;
using Mesh = SonicRetro.SAModel.Direct3D.Mesh;

namespace SADXObjectDefinitions.EmeraldCoast
{
	class OBEWind : ObjectDefinition
	{
		private NJS_MATERIAL material;
		private Texture texture;
		private Mesh mesh;

		public override void Init(ObjectData data, string name)
		{
			mesh = Mesh.Box(0.5f, 0.5f, 0.5f);
			material = new NJS_MATERIAL
			{
				DiffuseColor = Color.FromArgb(127, 178, 178, 178),
				UseAlpha = false
			};
		}

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			transform.Push();
			float scaleX = item.Scale.X;
			float scaleY = item.Scale.Y * 0.5f;
			float scaleZ = item.Scale.Z;
			if (scaleX >= 10.0f)
			{
				if (scaleX > 200.0f)
				{
					scaleX = 200f;
				}
			}
			else
			{
				scaleX = 10f;
			}
			if (scaleY >= 10.0f)
			{
				if (scaleY > 200.0f)
				{
					scaleY = 200f;
				}
			}
			else
			{
				scaleY = 10f;
			}
			if (scaleZ >= 10.0f)
			{
				if (scaleZ > 200.0f)
				{
					scaleZ = 200f;
				}
			}
			else
			{
				scaleZ = 10f;
			}
			transform.NJTranslate(item.Position);
			transform.NJRotateY(item.Rotation.Y);
			transform.NJScale(scaleX, scaleY, scaleZ);
			HitResult result = mesh.CheckHit(Near, Far, Viewport, Projection, View, transform);

			transform.Pop();
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Renderer dev, EditorCamera camera, MatrixStack transform)
		{
			// is this even necessary?
			if (texture == null)
				texture = new Texture(dev, 2, 2, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			List<RenderInfo> result = new List<RenderInfo>();
			transform.Push();
			float scaleX = item.Scale.X;
			float scaleY = item.Scale.Y * 0.5f;
			float scaleZ = item.Scale.Z;
			if (scaleX >= 10.0f)
			{
				if (scaleX > 200.0f)
				{
					scaleX = 200f;
				}
			}
			else
			{
				scaleX = 10f;
			}
			if (scaleY >= 10.0f)
			{
				if (scaleY > 200.0f)
				{
					scaleY = 200f;
				}
			}
			else
			{
				scaleY = 10f;
			}
			if (scaleZ >= 10.0f)
			{
				if (scaleZ > 200.0f)
				{
					scaleZ = 200f;
				}
			}
			else
			{
				scaleZ = 10f;
			}
			transform.NJTranslate(item.Position);
			transform.NJRotateY(item.Rotation.Y);
			transform.NJScale(scaleX, scaleY, scaleZ);


			float largestScale = item.Scale.X;
			if (item.Scale.Z > largestScale) largestScale = item.Scale.Z;

			BoundingSphere boxSphere = new BoundingSphere() { Center = new Vertex(item.Position.X, item.Position.Y, item.Position.Z), Radius = largestScale};

			RenderInfo outputInfo = new RenderInfo(mesh, 0, transform.Top, material, texture, FillMode.Wireframe, boxSphere);
			result.Add(outputInfo);

			if (item.Selected)
			{
				RenderInfo highlightInfo = new RenderInfo(mesh, 0, transform.Top, material, texture, FillMode.Wireframe, boxSphere);
				result.Add(highlightInfo);
			}

			transform.Pop();
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			return new List<ModelTransform>();
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			float scaleX = item.Scale.X;
			float scaleY = item.Scale.Y * 0.5f;
			float scaleZ = item.Scale.Z;
			if (scaleX >= 10.0f)
			{
				if (scaleX > 200.0f)
				{
					scaleX = 200f;
				}
			}
			else
			{
				scaleX = 10f;
			}
			if (scaleY >= 10.0f)
			{
				if (scaleY > 200.0f)
				{
					scaleY = 200f;
				}
			}
			else
			{
				scaleY = 10f;
			}
			if (scaleZ >= 10.0f)
			{
				if (scaleZ > 200.0f)
				{
					scaleZ = 200f;
				}
			}
			else
			{
				scaleZ = 10f;
			}
			float largestScale = scaleX;
			if (scaleY > largestScale) largestScale = scaleY;
			if (scaleZ > largestScale) largestScale = scaleZ;

			BoundingSphere boxSphere = new BoundingSphere() { Center = new Vertex(item.Position.X, item.Position.Y, item.Position.Z), Radius = largestScale };

			return boxSphere;
		}

		private readonly PropertySpec[] customProperties = new PropertySpec[] {
			new PropertySpec("Power", typeof(float), "Extended", null, null, (o) => o.Rotation.X, (o, v) => o.Rotation.X = (int)v)
		};

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);
			MatrixFunctions.RotateY(ref matrix, item.Rotation.Y);

			return matrix;
		}

		public override string Name { get { return "Updraft"; } }
	}
}
