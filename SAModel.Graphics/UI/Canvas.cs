using SonicRetro.SAModel.Structs;
using System.Collections.Generic;
using System.Drawing;
using System;
using System.Collections.ObjectModel;

namespace SonicRetro.SAModel.Graphics.UI
{
	/// <summary>
	/// Responsible for drawing UI elements
	/// </summary>
	public abstract class Canvas
	{

		private ReadOnlyDictionary<Guid, UIElement> _lastQueueContents;

		private readonly Queue<UIElement> _renderQueue;

		/// <summary>
		/// Current Screen Width
		/// </summary>
		protected int width;

		/// <summary>
		/// Current Screen Height
		/// </summary>
		protected int height;

		public Canvas()
		{
			_renderQueue = new Queue<UIElement>();
			_lastQueueContents = new ReadOnlyDictionary<Guid, UIElement>(new Dictionary<Guid, UIElement>());
		}

		public void Draw(UIElement element)
		{
			_renderQueue.Enqueue((UIElement)element.Clone());
		}

		public virtual void Render(int width, int height)
		{
			Dictionary<Guid, UIElement> _newQueueContents = new Dictionary<Guid, UIElement>();

			this.width = width;
			this.height = height;
			while(_renderQueue.Count > 0)
			{
				UIElement element = _renderQueue.Dequeue();
				_lastQueueContents.TryGetValue(element.ID, out UIElement old);

				if(element.GetType() == typeof(UIImage))
					DrawImage((UIImage)element, (UIImage)old);
				else if(element.GetType() == typeof(UIText))
					DrawText((UIText)element, (UIText)old);
				else throw new InvalidOperationException($"UI element of type {element.GetType()} not supported");

				_newQueueContents.Add(element.ID, element);
			}

			_lastQueueContents = new ReadOnlyDictionary<Guid, UIElement>(_newQueueContents);
			_renderQueue.Clear();
		}

		protected abstract void DrawImage(UIImage image, UIImage old);
		protected abstract void DrawText(UIText text, UIText old);
	}
}

