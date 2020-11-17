using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SonicRetro.SAModel.Graphics.OpenGL.Rendering;
using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.ObjData;
using Color = SonicRetro.SAModel.Structs.Color;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	struct BufferMeshHandle
	{
		public readonly int vao;
		public readonly int vbo;
		public readonly int eao;
		public readonly int vertexCount;

		public BufferMeshHandle(int vao, int vbo, int eao, int vertexCount)
		{
			this.vao = vao;
			this.vbo = vbo;
			this.eao = eao;
			this.vertexCount = vertexCount;
		}
	}

	struct CachedVertex
	{
		public Vector4 position;
		public Vector3 normal;

		public CachedVertex(Vector4 position, Vector3 normal)
		{
			this.position = position;
			this.normal = normal;
		}
	}

	public static class RenderMethods
	{
		public static void PrepareModel(List<GLRenderMesh> renderMeshes, GLCamera camera, NJObject activeObj, NJObject obj, Matrix4? parentWorld, bool weighted)
		{
			Matrix4 world = obj.LocalMatrix();
			if(parentWorld.HasValue)
				world *= parentWorld.Value;

			if(obj.Attach != null)
			{
				// if a model is weighted, then the buffered vertex positions/normals will have to be set to world space, which means that world and normal matrix should be identities
				if(weighted)
				{
					renderMeshes.Add(new GLRenderMesh(obj.Attach, world, Matrix4.Identity, Matrix4.Identity, camera.ViewMatrix * camera.Projectionmatrix, obj == activeObj));
				}
				else
				{
					Matrix4 normalMtx = world.Inverted();
					normalMtx.Transpose();
					renderMeshes.Add(new GLRenderMesh(obj.Attach, null, world, normalMtx, world * camera.ViewMatrix * camera.Projectionmatrix, obj == activeObj));
				}
			}

			for(int i = 0; i < obj.ChildCount; i++)
				PrepareModel(renderMeshes, camera, activeObj, obj[i], world, weighted);
		}

		public static void PrepareLandentry(List<GLRenderMesh> renderMeshes, List<LandEntry> entries, GLCamera camera, LandEntry activeLE, LandEntry le)
		{
			if(!camera.Renderable(le) || entries.Contains(le))
				return;
			entries.Add(le);
			Matrix4 world = le.LocalMatrix();
			Matrix4 normalMtx = world.Inverted();
			normalMtx.Transpose();
			renderMeshes.Add(new GLRenderMesh(le.Attach, null, world, normalMtx, world * camera.ViewMatrix * camera.Projectionmatrix, le == activeLE));
		}

		public static void RenderModels(List<GLRenderMesh> renderMeshes, bool transparent)
		{
			for(int i = 0; i < renderMeshes.Count; i++)
			{
				GLRenderMesh m = renderMeshes[i];
				GL.UniformMatrix4(10, false, ref m.worldMtx);
				GL.UniformMatrix4(11, false, ref m.normalMtx);
				GL.UniformMatrix4(12, false, ref m.MVP);
				m.attach.Render(m.realWorldMtx, transparent, m.active);
			}
		}

		public static void RenderModelsWireframe(List<GLRenderMesh> renderMeshes, bool transparent)
		{
			GL.Uniform1(13, 0.001f); // setting normal offset for wireframe
			RenderMode old = GLMaterial.RenderMode;
			GLMaterial.RenderMode = RenderMode.FullDark; // drawing the lines black
			GLMaterial.Reset();
			GLMaterial.ReBuffer();
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

			for(int i = 0; i < renderMeshes.Count; i++)
			{
				GLRenderMesh m = renderMeshes[i];
				GL.UniformMatrix4(10, false, ref m.worldMtx);
				GL.UniformMatrix4(11, false, ref m.normalMtx);
				GL.UniformMatrix4(12, false, ref m.MVP);
				m.attach.RenderWireframe(transparent);
			}

			// reset
			GL.Uniform1(13, 0f);
			GLMaterial.RenderMode = old;
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}

	}

	public static class Extensions
	{
		private static readonly CachedVertex[] vertices = new CachedVertex[0xFFFF];
		private static readonly float[] weights = new float[0xFFFF];
		private static readonly Dictionary<BufferMesh, BufferMeshHandle> meshHandles = new Dictionary<BufferMesh, BufferMeshHandle>();

		public static void ClearWeights()
		{
			Array.Clear(weights, 0, weights.Length);
		}

		static CachedVertex ToCache(this BufferVertex vtx)
		{
			return new CachedVertex(vtx.position.ToGL4(), vtx.normal.ToGL());
		}

		public static Vector3 ToGL(this Structs.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}

		public static Vector4 ToGL4(this Structs.Vector3 vec3)
		{
			return new Vector4(vec3.X, vec3.Y, vec3.Z, 1);
		}

		public static Structs.Vector3 ToSA(this Vector3 vec3)
		{
			return new Structs.Vector3(vec3.X, vec3.Y, vec3.Z);
		}

		public static Vector2 ToGL(this Structs.Vector2 vec2)
		{
			return new Vector2(vec2.X, vec2.Y);
		}

		public static Structs.Vector2 ToSA(this Vector2 vec2)
		{
			return new Structs.Vector2(vec2.X, vec2.Y);
		}

		public static Matrix4 LocalMatrix(this NJObject obj)
		{
			Matrix4 rotMtx;
			if(obj.RotateZYX)
			{
				rotMtx = Matrix4.CreateRotationZ(Structs.Helper.DegToRad(obj.Rotation.Z)) *
						Matrix4.CreateRotationY(Structs.Helper.DegToRad(obj.Rotation.Y)) *
						Matrix4.CreateRotationX(Structs.Helper.DegToRad(obj.Rotation.X));
			}
			else
			{
				rotMtx = Matrix4.CreateRotationX(Structs.Helper.DegToRad(obj.Rotation.X)) *
						Matrix4.CreateRotationY(Structs.Helper.DegToRad(obj.Rotation.Y)) *
						Matrix4.CreateRotationZ(Structs.Helper.DegToRad(obj.Rotation.Z));
			}


			return Matrix4.CreateScale(obj.Scale.ToGL()) * rotMtx * Matrix4.CreateTranslation(obj.Position.ToGL());
		}

		public static Matrix4 LocalMatrix(this LandEntry obj)
		{
			Matrix4 rotMtx;
			if (obj.RotateZYX)
			{
				rotMtx = Matrix4.CreateRotationZ(Structs.Helper.DegToRad(obj.Rotation.Z)) *
						Matrix4.CreateRotationY(Structs.Helper.DegToRad(obj.Rotation.Y)) *
						Matrix4.CreateRotationX(Structs.Helper.DegToRad(obj.Rotation.X));
			}
			else
			{
				rotMtx = Matrix4.CreateRotationX(Structs.Helper.DegToRad(obj.Rotation.X)) *
						Matrix4.CreateRotationY(Structs.Helper.DegToRad(obj.Rotation.Y)) *
						Matrix4.CreateRotationZ(Structs.Helper.DegToRad(obj.Rotation.Z));
			}


			return Matrix4.CreateScale(obj.Scale.ToGL()) * rotMtx * Matrix4.CreateTranslation(obj.Position.ToGL());
		}

		unsafe public static void Buffer(this ModelData.Attach atc, Matrix4? worldMtx, bool active)
		{
			if (atc.MeshData == null) throw new InvalidOperationException("Attach \"" + atc.Name + "\" has not been buffered");
			if (atc.MeshData.Length == 0) return;
			
			Matrix3 normalMtx = default;
			if(worldMtx.HasValue)
			{
				Matrix4 t = worldMtx.Value.Inverted();
				t.Transpose();
				normalMtx = new Matrix3(t);
			}

			foreach (BufferMesh mesh in atc.MeshData)
			{
				if (mesh.Vertices != null)
				{
					if (worldMtx == null)
					{
						foreach (BufferVertex vtx in mesh.Vertices)
							vertices[vtx.index] = vtx.ToCache();
					}
					else
					{
						foreach (BufferVertex vtx in mesh.Vertices)
						{
							Vector4 pos = (vtx.position.ToGL4() * worldMtx.Value) * vtx.weight;
							Vector3 nrm = (vtx.normal.ToGL() * normalMtx) * vtx.weight;
							if (active)
								weights[vtx.index] = vtx.weight;
							else if (weights[vtx.index] > 0 && !mesh.ContinueWeight) weights[vtx.index] = 0;

							if (mesh.ContinueWeight)
							{
								vertices[vtx.index].position += pos;
								vertices[vtx.index].normal += nrm;
							}
							else
							{
								vertices[vtx.index].position = pos;
								vertices[vtx.index].normal = nrm;
							}
						}
					}
				}
				
				if(mesh.Corners != null)
				{
					int structSize = 36;
					byte[] vertexData;
					using (MemoryStream stream = new MemoryStream(mesh.Corners.Length * structSize))
					{
						BinaryWriter writer = new BinaryWriter(stream);

						foreach (BufferCorner c in mesh.Corners)
						{
							CachedVertex vtx = vertices[c.vertexIndex];

							writer.Write(vtx.position.X);
							writer.Write(vtx.position.Y);
							writer.Write(vtx.position.Z);

							writer.Write(vtx.normal.X);
							writer.Write(vtx.normal.Y);
							writer.Write(vtx.normal.Z);

							Color col = worldMtx.HasValue ? GraphicsHelper.GetWeightColor(weights[c.vertexIndex]) : c.color;
							writer.Write(new byte[] { col.R, col.G, col.B, col.A });

							writer.Write(c.uv.X);
							writer.Write(c.uv.Y);
						}

						vertexData = stream.ToArray();
					}

					if (meshHandles.ContainsKey(mesh))
					{
						if (!worldMtx.HasValue) throw new InvalidOperationException("Rebuffering weighted(?) mesh without matrix");
						BufferMeshHandle meshHandle = meshHandles[mesh];

						GL.BindBuffer(BufferTarget.ArrayBuffer, meshHandle.vbo);

						fixed (byte* ptr = vertexData)
							GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length, (IntPtr)ptr, BufferUsageHint.StreamDraw);
					}
					else
					{
						// generating the buffers
						int vao = GL.GenVertexArray();
						int vbo = GL.GenBuffer();
						int eao = 0;
						int vtxCount = mesh.TriangleList == null ? mesh.Corners.Length : mesh.TriangleList.Length;

						// Binding the buffers
						GL.BindVertexArray(vao);
						GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

						fixed (byte* ptr = vertexData)
							GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length, (IntPtr)ptr, worldMtx.HasValue ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
						

						if (mesh.TriangleList != null)
						{
							eao = GL.GenBuffer();
							GL.BindBuffer(BufferTarget.ElementArrayBuffer, eao);

							fixed (uint* ptr = mesh.TriangleList)
								GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.TriangleList.Length * sizeof(uint), (IntPtr)ptr, BufferUsageHint.StaticDraw);
						}
						else GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);


						// assigning attribute data
						// position
						GL.EnableVertexAttribArray(0);
						GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, structSize, 0);

						// normal
						GL.EnableVertexAttribArray(1);
						GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, structSize, 12);

						// color
						GL.EnableVertexAttribArray(2);
						GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, structSize, 24);

						// uv
						GL.EnableVertexAttribArray(3);
						GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, structSize, 28);

						meshHandles.Add(mesh, new BufferMeshHandle(vao, vbo, eao, vtxCount));
					}
				}
			}
		}

		public static void DeBuffer(this ModelData.Attach atc)
		{
			foreach(BufferMesh mesh in atc.MeshData)
			{
				if(meshHandles.TryGetValue(mesh, out var handle))
				{
					GL.DeleteVertexArray(handle.vao);
					GL.DeleteBuffer(handle.vbo);
					if(handle.eao != 0) GL.DeleteBuffer(handle.eao);
					meshHandles.Remove(mesh);
				}
			}
		}

		public static void Render(this ModelData.Attach atc, Matrix4? weightMtx, bool transparent, bool active)
		{
			if (atc.MeshData == null) throw new InvalidOperationException($"Attach {atc.Name} has no buffer meshes");

			// rebuffer weighted models
			if (weightMtx.HasValue && !transparent)
			{
				atc.Buffer(weightMtx, active);
			}

			foreach (BufferMesh m in atc.MeshData)
			{
				if (m.Material == null || m.Material.UseAlpha != transparent) continue;

				if(!meshHandles.TryGetValue(m, out var handle))
				{
					atc.Buffer(null, active);
					handle = meshHandles[m];
				}
				GLMaterial.Buffer(m.Material);
				GL.BindVertexArray(handle.vao);
				GL.DrawElements(BeginMode.Triangles, handle.vertexCount, DrawElementsType.UnsignedInt, 0);
			}
		}

		public static void RenderWireframe(this ModelData.Attach atc, bool transparent)
		{
			if (atc.MeshData == null) throw new InvalidOperationException($"Attach {atc.Name} has no buffer meshes");

			foreach (BufferMesh m in atc.MeshData)
			{
				if (m.Material == null || m.Material.UseAlpha != transparent) continue;

				if (meshHandles.TryGetValue(m, out var handle))
				{

					if (m.Material.Culling)
						GL.Enable(EnableCap.CullFace);
					else GL.Disable(EnableCap.CullFace);

					GL.BindVertexArray(handle.vao);
					if (handle.eao == 0)
						GL.DrawArrays(PrimitiveType.Triangles, 0, handle.vertexCount);
					else GL.DrawElements(BeginMode.Triangles, handle.vertexCount, DrawElementsType.UnsignedInt, 0);
				}
				else throw new InvalidOperationException($"Mesh in {atc.Name} not buffered");
			}
		}
	}
}
