using SonicRetro.SAModel.Structs;
using System;

namespace SonicRetro.SAModel.Graphics.UI
{
	/// <summary>
	/// Base UI Element class
	/// </summary>
	public abstract class UIElement : ICloneable
	{
		/// <summary>
		/// Used internally for re-drawing an element
		/// </summary>
		public readonly Guid ID;

		/// <summary>
		/// Position in the Window
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// Pivot Center for rotating
		/// </summary>
		public Vector2 LocalPivot { get; set; }

		/// <summary>
		/// Pivot center on the window (default is bottom left)
		/// </summary>
		public Vector2 GlobalPivot { get; set; }

		/// <summary>
		/// Rotation in radians around the pivot
		/// </summary>
		public float Rotation { get; set; }

		protected UIElement(Vector2 position, Vector2 localPivot, Vector2 globalPivot, float rotation)
		{
			Position = position;
			LocalPivot = localPivot;
			GlobalPivot = globalPivot;
			Rotation = rotation;
			ID = Guid.NewGuid();
		}

		public bool EqualTransform(UIElement other)
		{
			return other != null &&
				other.Position == Position
				&& other.LocalPivot == LocalPivot
				&& other.GlobalPivot == GlobalPivot
				&& other.Rotation == Rotation;
		}

		public virtual object Clone() => MemberwiseClone();
	}
}
