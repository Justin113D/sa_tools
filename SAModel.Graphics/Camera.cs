using SonicRetro.SAModel.ObjData;
using SonicRetro.SAModel.Structs;
using System;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Camera class
	/// </summary>
	public abstract class Camera
	{
		protected Vector3 _position;
		protected Vector3 _rotation;

		protected bool _orbiting;
		protected float _distance;

		protected float _fov;
		protected float _aspect;
		protected float _viewDist;

		protected bool _orthographic;

		protected Vector3 _forward;
		protected Vector3 _right;
		protected Vector3 _up;

		public float DragSpeed { get; set; }
		public float MovementSpeed { get; set; }
		public float MovementModif { get; set; }
		public float MouseSensitivity { get; set; }


		/// <summary>
		/// Position of the camera in world space <br/>
		/// Position of focus in orbit mode
		/// </summary>
		public Vector3 Position
		{
			get => _position;
			set
			{
				_position = value;
				UpdateViewMatrix();
			}
		}

		/// <summary>
		/// Position of camera in world space (regardless of orbit mode
		/// </summary>
		public Vector3 Realposition
		{
			get
			{
				return _position - _forward * _distance;
			}
		}

		/// <summary>
		/// The rotation of the camera in world space
		/// </summary>
		public Vector3 Rotation
		{
			get => _rotation;
			set
			{
				_rotation = value;
				UpdateDirections();
				UpdateViewMatrix();
			}
		}

		/// <summary>
		/// The bams rotation of the camera in world space
		/// </summary>
		public Rotation BAMSRotation
		{
			get
			{
				int x = SAModel.Rotation.DegToBAMS(_rotation.X);
				int y = SAModel.Rotation.DegToBAMS(_rotation.Y);
				int z = SAModel.Rotation.DegToBAMS(_rotation.Z);
				return new Rotation(x, y, z);
			}
			set
			{
				float x = SAModel.Rotation.BAMSToDeg(value.X);
				float y = SAModel.Rotation.BAMSToDeg(value.Y);
				float z = SAModel.Rotation.BAMSToDeg(value.Z);
				_rotation = new Vector3(x, y, z);
				UpdateDirections();
				UpdateViewMatrix();
			}
		}

		/// <summary>
		/// Whether the control scheme is set to orbiting
		/// </summary>
		public bool Orbiting
		{
			get
			{
				return _orbiting;
			}
			set
			{
				if (_orbiting == value) return;
				_orbiting = value;

				var t = _forward * _distance;
				Position += _orbiting ? t : -t;

				UpdateViewMatrix();
				UpdateProjectionMatrix();
			}
		}

		/// <summary>
		/// The orbiting distance
		/// </summary>
		public float Distance
		{
			get
			{
				return _distance;
			}
			set
			{
				_distance = Math.Min(_viewDist, Math.Max(0.0001f, value));
				UpdateViewMatrix();
				UpdateProjectionMatrix();
			}
		}

		/// <summary>
		/// The field of view
		/// </summary>
		public float FieldOfView
		{
			get =>  Helper.RadToDeg(_fov);
			set
			{
				_fov = Helper.DegToRad(value);
				UpdateProjectionMatrix();
			}
		}

		/// <summary>
		/// The screen aspect
		/// </summary>
		public float Aspect
		{
			get => _aspect;
			set
			{
				_aspect = value;
				UpdateProjectionMatrix();
			}
		}

		/// <summary>
		/// Whether the camera is set to orthographic
		/// </summary>
		public bool Orthographic
		{
			get => _orthographic;
			set
			{
				if (_orthographic == value) return;
				_orthographic = value;
				UpdateViewMatrix();
				UpdateProjectionMatrix();
			}
		}

		/// <summary>
		/// The view/render distance
		/// </summary>
		public float ViewDistance
		{
			get => _viewDist;
			set
			{
				_viewDist = value;
				UpdateViewMatrix();
			}
		}

		/// <summary>
		/// The direction in which the camera is viewing
		/// </summary>
		public Vector3 ViewDir
		{
			get => _forward;
		}

		/// <summary>
		/// Creates a new camera from the resolution ratio
		/// </summary>
		/// <param name="aspect"></param>
		public Camera(float aspect)
		{
			_orbiting = true;
			_distance = 50;
			_fov = Helper.DegToRad(50);
			_aspect = aspect;
			_orthographic = false;
			_viewDist = 3000;
			MovementSpeed = 30f;
			MovementModif = 2f;
			DragSpeed = 0.001f;
			MouseSensitivity = 0.1f;

			UpdateDirections();
			UpdateViewMatrix();
			UpdateProjectionMatrix();
		}

		protected abstract void UpdateDirections();
		protected abstract void UpdateViewMatrix();
		protected abstract void UpdateProjectionMatrix();

		/// <summary>
		/// Moves the camera using global settings and the input class
		/// </summary>
		public void Move(float delta)
		{
			DebugSettings s = DebugSettings.Global;
			if (!_orbiting)
			{
				// rotation
				Rotation = new Vector3(Math.Max(-90, Math.Min(90, Rotation.X + Input.CursorDif.Y * MouseSensitivity)), (Rotation.Y + Input.CursorDif.X * MouseSensitivity) % 360f, 0);

				// modifying movement speed 
				float dir = Input.ScrollDif > 0 ? -0.05f : 0.05f;
				for(int i = Math.Abs(Input.ScrollDif); i > 0; i--)
				{
					MovementSpeed += MovementSpeed * dir;
					MovementSpeed = Math.Max(0.0001f, Math.Min(1000, MovementSpeed));
				}

				// movement
				Vector3 dif = default;

				if (Input.IsKeyDown(s.fpForward))
					dif += _forward;

				if (Input.IsKeyDown(s.fpBackward))
					dif -= _forward;

				if (Input.IsKeyDown(s.fpLeft))
					dif += _right;

				if (Input.IsKeyDown(s.fpRight))
					dif -= _right;

				if (Input.IsKeyDown(s.fpUp))
					dif += _up;

				if (Input.IsKeyDown(s.fpDown))
					dif -= _up;

				if (dif.Length == 0) return;
				Position += dif.Normalized() * MovementSpeed * (Input.IsKeyDown(s.fpSpeedup) ? MovementModif : 1) * delta;
			}
			else
			{
				// mouse orientation
				if (Input.IsKeyDown(s.OrbitKey))
				{
					if (Input.IsKeyDown(s.zoomModifier)) // zooming
					{
						Distance += Distance * Input.CursorDif.Y * 0.01f;
					}
					else if (Input.IsKeyDown(s.dragModifier)) // moving
					{
						Vector3 dif = default;
						float speed = DragSpeed * Distance;
						dif += _right * Input.CursorDif.X * speed;
						dif += _up * Input.CursorDif.Y * speed;
						Position += dif;
					}
					else // rotation
					{
						Rotation = new Vector3(Math.Max(-90, Math.Min(90, Rotation.X + Input.CursorDif.Y * MouseSensitivity * 2)), (Rotation.Y + Input.CursorDif.X * MouseSensitivity * 2) % 360f, 0);
					}
				}
				else
				{
					if (Input.KeyPressed(s.perspective))
						Orthographic = !Orthographic;

					bool invertAxis = Input.IsKeyDown(s.alignInvert);
					if (Input.KeyPressed(s.alignForward))
						Rotation = new Vector3(0, invertAxis ? 180 : 0, 0);
					else if (Input.KeyPressed(s.alignSide))
						Rotation = new Vector3(0, invertAxis ? -90 : 90, 0);
					else if (Input.KeyPressed(s.alignUp))
						Rotation = new Vector3(invertAxis ? -90 : 90, 0, 0);

					float dir = Input.ScrollDif > 0 ? 0.07f : -0.07f;
					for (int i = Math.Abs(Input.ScrollDif); i > 0; i--)
					{
						Distance += Distance * dir;
					}
				}
			}
		}

		public abstract bool Renderable(LandEntry geometry);
	}
}
