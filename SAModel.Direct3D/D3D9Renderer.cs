using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SonicRetro.SAModel.Direct3D
{
	public class D3D9Renderer : Renderer
	{
		private Device device;

		public D3D9Renderer(Control control) : this(control.Handle) { }

		public D3D9Renderer(IntPtr windowHandle)
		{
			device = new Device(new SharpDX.Direct3D9.Direct3D(), 0, DeviceType.Hardware, windowHandle, CreateFlags.HardwareVertexProcessing,
				new PresentParameters
				{
					Windowed = true,
					SwapEffect = SwapEffect.Discard,
					EnableAutoDepthStencil = true,
					AutoDepthStencilFormat = Format.D24X8
				});
		}

		public override void EnableLight(int index, bool enable)
		{
			device.EnableLight(index, enable);
		}

		public override void SetLight(int index, Light light)
		{
			SharpDX.Direct3D9.Light l0 = new SharpDX.Direct3D9.Light()
			{
				Type = (SharpDX.Direct3D9.LightType)light.Type,
				Diffuse = light.Diffuse.ToRawColor4(),
				Specular = light.Specular.ToRawColor4(),
				Ambient = light.Ambient.ToRawColor4(),
				Position = light.Position,
				Direction = light.Direction,
				Range = light.Range,
				Falloff = light.Falloff,
				Attenuation0 = light.Attenuation0,
				Attenuation1 = light.Attenuation1,
				Attenuation2 = light.Attenuation2,
				Theta = light.Theta,
				Phi = light.Phi
			};
			device.SetLight(index, ref l0);
		}
	}
}
