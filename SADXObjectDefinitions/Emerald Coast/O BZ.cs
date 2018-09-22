﻿using SharpDX;
using SonicRetro.SAModel;
using SonicRetro.SAModel.Direct3D;
using SonicRetro.SAModel.SAEditorCommon.DataTypes;
using SonicRetro.SAModel.SAEditorCommon.SETEditing;
using System.Collections.Generic;
using BoundingSphere = SonicRetro.SAModel.BoundingSphere;
using Mesh = SonicRetro.SAModel.Direct3D.Mesh;

namespace SADXObjectDefinitions.EmeraldCoast
{
	public class Plane : ObjectDefinition
	{
		protected NJS_OBJECT plane;
		protected Mesh[] planemsh;
		protected NJS_OBJECT sphere;
		protected Mesh[] spheremsh;

		public override void Init(ObjectData data, string name)
		{
			plane = ObjectHelper.LoadModel("Objects/Levels/Emerald Coast/O BZ.sa1mdl");
			planemsh = ObjectHelper.GetMeshes(plane);
			sphere = ObjectHelper.LoadModel("Objects/Collision/C SPHERE.sa1mdl");
			spheremsh = ObjectHelper.GetMeshes(sphere);
		}

		public override string Name { get { return "Tails' Plane"; } }

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			HitResult result = HitResult.NoHit;
			transform.Push();
			transform.NJTranslate((item.Position.X + 50f), item.Position.Y, item.Position.Z);
			transform.NJRotateObject(item.Rotation);
			transform.NJScale(1f, 1f, 1f);
			transform.Push();
			result = HitResult.Min(result, plane.CheckHit(Near, Far, Viewport, Projection, View, transform, planemsh));
			transform.Pop();
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.NJRotateObject(item.Rotation);
			transform.NJScale(item.Scale.X, item.Scale.X, item.Scale.X);
			transform.Push();
			result = HitResult.Min(result, sphere.CheckHit(Near, Far, Viewport, Projection, View, transform, spheremsh));
			transform.Pop();
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Renderer dev, EditorCamera camera, MatrixStack transform)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			transform.Push();
			transform.NJTranslate((item.Position.X + 50f), item.Position.Y, item.Position.Z);
			transform.NJRotateObject(item.Rotation);
			result.AddRange(plane.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("OBJ_REGULAR"), planemsh));
			if (item.Selected)
				result.AddRange(plane.DrawModelTreeInvert(transform, planemsh));
			transform.Pop();
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.NJRotateObject(item.Rotation);
			transform.NJScale(item.Scale.X, item.Scale.X, item.Scale.X);
			result.AddRange(sphere.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, null, spheremsh));
			if (item.Selected)
				result.AddRange(sphere.DrawModelTreeInvert(transform, spheremsh));
			transform.Pop();
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			List<ModelTransform> result = new List<ModelTransform>();
			transform.Push();
			transform.NJTranslate((item.Position.X + 50f), item.Position.Y, item.Position.Z);
			transform.NJRotateObject(item.Rotation);
			result.Add(new ModelTransform(plane, transform.Top));
			transform.Pop();
			return result;
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			MatrixStack transform1 = new MatrixStack();
			transform1.NJTranslate(item.Position);
			transform1.NJScale(item.Scale.X, item.Scale.X, item.Scale.X);
			return ObjectHelper.GetModelBounds(sphere, transform1);
		}

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);
			MatrixFunctions.RotateObject(ref matrix, item.Rotation);

			return matrix;
		}
	}
}