using SonicRetro.SAModel.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicRetro.SAModel.Graphics.APIAccess
{
	public interface IGAPIACanvas
	{
		void CanvasPreDraw(int width, int height);

		void CanvasPostDraw();

		void CanvasDrawUIElement(UIElement element, float width, float height, bool forceUpdateTransforms);
	}
}
