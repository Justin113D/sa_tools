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
	public class BigWJump : ObjectDefinition
	{
		protected NJS_OBJECT model;
		protected Mesh[] mesh;

		public override void Init(ObjectData data, string name)
		{
			model = ObjectHelper.LoadModel("Objects/Collision/C CUBE.sa1mdl");
			mesh = ObjectHelper.GetMeshes(model);
		}

		public override string Name { get { return "Big Water Jump"; } }

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			HitResult result = HitResult.NoHit;
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.NJScale((item.Scale.X + 10f), (item.Scale.Y + 10f), (item.Scale.Z + 10f));
			transform.Push();
			result = HitResult.Min(result, model.CheckHit(Near, Far, Viewport, Projection, View, transform, mesh));
			transform.Pop();
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Renderer dev, EditorCamera camera, MatrixStack transform)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			transform.Push();
			transform.NJTranslate(item.Position);
			transform.NJScale((item.Scale.X + 10f), (item.Scale.Y + 10f), (item.Scale.Z + 10f));
			result.AddRange(model.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("OBJ_BEACH"), mesh));
			if (item.Selected)
				result.AddRange(model.DrawModelTreeInvert(transform, mesh));
			transform.Pop();
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			return new List<ModelTransform>();
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			MatrixStack transform = new MatrixStack();
			transform.NJTranslate(item.Position);
			transform.NJScale((item.Scale.X + 10f), (item.Scale.Y + 10f), (item.Scale.Z + 10f));
			return ObjectHelper.GetModelBounds(model, transform);
		}

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);

			return matrix;
		}
	}
}