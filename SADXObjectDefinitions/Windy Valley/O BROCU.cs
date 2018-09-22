﻿using SharpDX;
using SonicRetro.SAModel;
using SonicRetro.SAModel.Direct3D;
using SonicRetro.SAModel.SAEditorCommon.DataTypes;
using SonicRetro.SAModel.SAEditorCommon.SETEditing;
using System.Collections.Generic;
using BoundingSphere = SonicRetro.SAModel.BoundingSphere;
using Mesh = SonicRetro.SAModel.Direct3D.Mesh;

namespace SADXObjectDefinitions.WindyValley
{
	public class Brocu : ObjectDefinition
	{
		private NJS_OBJECT modelA;
		private Mesh[] meshesA;
		private NJS_OBJECT modelB;
		private Mesh[] meshesB;
		private NJS_OBJECT modelC;
		private Mesh[] meshesC;
		private ulong v5;
		private int v6;

		public override void Init(ObjectData data, string name)
		{
			modelA = ObjectHelper.LoadModel("Objects/Levels/Windy Valley/O_CUBES.sa1mdl");
			meshesA = ObjectHelper.GetMeshes(modelA);
			modelB = ObjectHelper.LoadModel("Objects/Levels/Windy Valley/O_CUBEM.sa1mdl");
			meshesB = ObjectHelper.GetMeshes(modelB);
			modelC = ObjectHelper.LoadModel("Objects/Levels/Windy Valley/O_CUBEL.sa1mdl");
			meshesC = ObjectHelper.GetMeshes(modelC);
		}

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			HitResult result = HitResult.NoHit;
			v5 = (ulong)item.Scale.X;
			if (v5 != 0)
			{
				v6 = (int)v5 - 1;
				if (v6 == 0)
				{
					transform.Push();
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					result = HitResult.Min(result, modelB.CheckHit(Near, Far, Viewport, Projection, View, transform, meshesB));
					transform.Pop();
					return result;
				}
				if (v6 == 1)
				{
					transform.Push();
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					result = HitResult.Min(result, modelC.CheckHit(Near, Far, Viewport, Projection, View, transform, meshesC));
					transform.Pop();
					return result;
				}
			}
			else
			{
				transform.Push();
				transform.NJTranslate(item.Position);
				transform.NJRotateObject(item.Rotation);
				result = HitResult.Min(result, modelA.CheckHit(Near, Far, Viewport, Projection, View, transform, meshesA));
				transform.Pop();
				return result;
			}
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Renderer dev, EditorCamera camera, MatrixStack transform)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			v5 = (ulong)item.Scale.X;
			if (v5 != 0)
			{
				v6 = (int)v5 - 1;
				if (v6 == 0)
				{
					transform.Push();
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					result.AddRange(modelB.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("OBJ_WINDY"), meshesB));
					if (item.Selected)
						result.AddRange(modelB.DrawModelTreeInvert(transform, meshesB));
					transform.Pop();
					return result;
				}
				if (v6 == 1)
				{
					transform.Push();
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					result.AddRange(modelC.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("OBJ_WINDY"), meshesC));
					if (item.Selected)
						result.AddRange(modelC.DrawModelTreeInvert(transform, meshesC));
					transform.Pop();
					return result;
				}
			}
			else
			{
				transform.Push();
				transform.NJTranslate(item.Position);
				transform.NJRotateObject(item.Rotation);
				result.AddRange(modelA.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("OBJ_WINDY"), meshesA));
				if (item.Selected)
					result.AddRange(modelA.DrawModelTreeInvert(transform, meshesA));
				transform.Pop();
				return result;
			}
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			List<ModelTransform> result = new List<ModelTransform>();
			v5 = (ulong)item.Scale.X;
			if (v5 != 0)
			{
				v6 = (int)v5 - 1;
				if (v6 == 0)
				{
					transform.Push();
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					result.Add(new ModelTransform(modelB, transform.Top));
					transform.Pop();
					return result;
				}
				if (v6 == 1)
				{
					transform.Push();
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					result.Add(new ModelTransform(modelC, transform.Top));
					transform.Pop();
					return result;
				}
			}
			else
			{
				transform.Push();
				transform.NJTranslate(item.Position);
				transform.NJRotateObject(item.Rotation);
				result.Add(new ModelTransform(modelA, transform.Top));
				transform.Pop();
				return result;
			}
			return result;
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			MatrixStack transform = new MatrixStack();
			v5 = (ulong)item.Scale.X;
			if (v5 != 0)
			{
				v6 = (int)v5 - 1;
				if ( v6 == 0)
				{
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					return ObjectHelper.GetModelBounds(modelB, transform);
				}
				if (v6 == 1)
				{
					transform.NJTranslate(item.Position);
					transform.NJRotateObject(item.Rotation);
					return ObjectHelper.GetModelBounds(modelC, transform);
				}
			}
			else
			{
				transform.NJTranslate(item.Position);
				transform.NJRotateObject(item.Rotation);
				return ObjectHelper.GetModelBounds(modelA, transform);
			}
			return ObjectHelper.GetModelBounds(null, transform);
		}

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);
			MatrixFunctions.RotateObject(ref matrix, item.Rotation);

			return matrix;
		}

		public override string Name { get { return "Decoration Cubes (Small, Medium, Large)"; } }
	}
}