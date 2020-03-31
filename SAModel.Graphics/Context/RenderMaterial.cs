using SonicRetro.SAModel.ModelData.Buffer;
using SonicRetro.SAModel.Structs;

namespace SonicRetro.SAModel.Graphics
{
	public class RenderMaterial
	{
		/// <summary>
		/// Active material
		/// </summary>
		public static BufferMaterial material { get; set; }

		/// <summary>
		/// Global rendering mode
		/// </summary>
		public static RenderMode RenderMode { get; set; }

		/// <summary>
		/// Viewing position
		/// </summary>
		public static Vector3 ViewPos { get; set; }

		/// <summary>
		/// Lighting direction
		/// </summary>
		public static Vector3 LightDir { get; set; }

		/// <summary>
		/// Camera viewing direction
		/// </summary>
		public static Vector3 ViewDir { get; set; }


	}
}
