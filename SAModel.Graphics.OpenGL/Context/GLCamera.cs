using OpenTK;
using SonicRetro.SAModel.ObjData;

namespace SonicRetro.SAModel.Graphics.OpenGL
{
	public class GLCamera : Camera
	{
		public Matrix4 ViewMatrix { get; private set; }
		public Matrix4 Projectionmatrix { get; private set; }

		private Matrix4 RotMtx
		{
			get
			{
				return Matrix4.CreateRotationZ(OpenTK.MathHelper.DegreesToRadians(_rotation.Z)) *
						Matrix4.CreateRotationY(OpenTK.MathHelper.DegreesToRadians(_rotation.Y)) *
						Matrix4.CreateRotationX(OpenTK.MathHelper.DegreesToRadians(_rotation.X));
			}
		}


		public GLCamera(float aspect) : base(aspect)
		{
		}

		protected override void UpdateDirections()
		{
			Matrix4 mtx = RotMtx;
			_forward = new Vector3(mtx * -Vector4.UnitZ).ToSA().Normalized();
			_up = new Vector3(mtx * Vector4.UnitY).ToSA().Normalized();
			_right = new Vector3(mtx * -Vector4.UnitX).ToSA().Normalized();
		}

		protected override void UpdateViewMatrix()
		{
			ViewMatrix = Matrix4.CreateTranslation(-_position.ToGL()) * RotMtx;
			if (_orbiting)
			{
				ViewMatrix = Matrix4.CreateTranslation(_forward.ToGL() * (_orthographic ? _viewDist / 2 : _distance)) * ViewMatrix;
			}
		}

		protected override void UpdateProjectionMatrix()
		{
			if (_orthographic && _orbiting)
			{
				float scale = _distance;
				Projectionmatrix = Matrix4.CreateOrthographic(scale * _aspect, scale, 0.1f, _viewDist);
			}
			else Projectionmatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, _aspect, 0.1f, _viewDist);
		}

		public override bool Renderable(LandEntry geometry)
		{
			Vector4 pos = geometry.ModelBounds.Position.ToGL4() * ViewMatrix;
			float dist = pos.Length;
			return (dist - geometry.ModelBounds.Radius) <= _viewDist && pos.Z <= geometry.ModelBounds.Radius;
		}
	}
}
