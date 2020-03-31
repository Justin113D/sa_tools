using SonicRetro.SAModel.ObjData;
using SonicRetro.SAModel.ObjData.Animation;

namespace SonicRetro.SAModel.Graphics
{
	public abstract class GameTask
	{
		public NjsObject obj;
		// texture list
		public virtual void Start()
		{

		}

		public virtual void Update(double time)
		{

		}

		public virtual void Display()
		{

		}

		public virtual void End()
		{

		}

	}

	/// <summary>
	/// An empty task, which is just used to display a model
	/// </summary>
	public class DisplayTask : GameTask
	{
		public Motion motion;
		public float animSpeed;

		public DisplayTask(NjsObject obj)
		{
			this.obj = obj;
		}

		public void UpdateAnim(double time)
		{
			if (motion == null) return;

			float f = (float)(time % (motion.Frames - 1));

			NjsObject[] models = obj.GetObjects();
			for(int i = 0; i < models.Length; i++)
			{
				if(motion.Keyframes.ContainsKey(i))
				{
					NjsObject mdl = models[i];
					if (!mdl.Animate) continue;
					Frame frame = motion.Keyframes[i].GetFrameAt(f);
					if(frame.position.HasValue)
						mdl.Position = frame.position.Value;
					if (frame.rotation.HasValue)
						mdl.Rotation = frame.rotation.Value;
					if (frame.scale.HasValue)
						mdl.Scale = frame.scale.Value;
				}
			}
		}

		public override void Update(double time)
		{
			base.Update(time);
			UpdateAnim(time * animSpeed);
		}
	}
}
