using SonicRetro.SAModel.ObjData;
using SonicRetro.SAModel.ObjData.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonicRetro.SAModel.Graphics
{
	public class Scene
	{
		public readonly Camera cam;
		public double time = 0;
		public float timelineSpeed = 1;

		public readonly List<GameTask> objects = new List<GameTask>();
		public readonly List<LandEntry> geometry = new List<LandEntry>();
		public readonly List<LandEntry> collision = new List<LandEntry>();
		public readonly List<ModelData.Attach> attaches = new List<ModelData.Attach>();
		public readonly List<ModelData.Attach> weightedAttaches = new List<ModelData.Attach>();

		public Scene(Camera cam)
		{
			this.cam = cam;
		}

		public void LoadModelFile(ObjData.ModelFile file)
		{
			objects.Add(new DisplayTask(file.Model));
			NjsObject[] objs = file.Model.GetObjects();
			if(file.Model.HasWeight)
			{
				foreach (NjsObject obj in objs)
				{
					if (obj.Attach == null) continue;
					if (!weightedAttaches.Contains(obj.Attach))
						weightedAttaches.Add(obj.Attach);
				}
			}
			else
			{
				foreach(NjsObject obj in objs)
				{
					if (obj.Attach == null) continue;
					if (!attaches.Contains(obj.Attach))
						attaches.Add(obj.Attach);
				}
			}

		}

		public void LoadModelFile(ObjData.ModelFile file, Motion motion, float animSpeed)
		{
			LoadModelFile(file);
			DisplayTask tsk = objects.Last() as DisplayTask;

			int mdlCount = tsk.obj.GetObjects().Length;
			if (motion.ModelCount > mdlCount)
				throw new ArgumentException($"Motion not compatible with model! \n Motion model count: {motion.ModelCount} \n Model count: {mdlCount}");
			tsk.motion = motion;
			tsk.animSpeed = animSpeed;
		}

		public void LoadLandtable(ObjData.LandTable table)
		{
			if(table.Format > LandtableFormat.SADX)
			{
				foreach(LandEntry le in table.Geometry)
				{
					if (le.Attach.Format == ModelData.AttachFormat.BASIC)
						collision.Add(le);
					else geometry.Add(le);
					if (!attaches.Contains(le.Attach))
						attaches.Add(le.Attach);
				}
			}
			else
			{
				foreach (LandEntry le in table.Geometry)
				{
					if (le.SurfaceFlags.IsCollision()) collision.Add(le);
					if (le.SurfaceFlags.HasFlag(SurfaceFlags.Visible)) geometry.Add(le);
					if (!attaches.Contains(le.Attach))
						attaches.Add(le.Attach);
				}
			}
		}

		public void Update(float delta)
		{
			time += delta;
			foreach(GameTask tsk in objects)
			{
				tsk.Update(time);
			}
		}
	}
}
