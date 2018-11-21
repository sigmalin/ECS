using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering;
using Unity.Collections;


namespace Lesson5
{
    [UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
    class Lesson5Group{}

    [UpdateInGroup(typeof(Lesson5Group))]    
    [DisableAutoCreation]
    public class AnimationSpriteSystem : ComponentSystem
	{
        ComponentGroup group;

		protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<AnimationSpriteData>(), ComponentType.ReadOnly<AnimationData>() };
            group = GetComponentGroup(componentTypes);
        }

        protected override void OnUpdate()
        {
            var sprites = group.GetComponentDataArray<AnimationSpriteData>();
            var anims = group.GetSharedComponentDataArray<AnimationData>();

            float delta = UnityEngine.Time.deltaTime;

            for(int i=0; i<sprites.Length; i++)
            {
                AnimationSpriteData sprite = sprites[i];
                AnimationData anim = anims[i];

                Process(ref sprite, ref anim, delta);

                sprites[i] = sprite;
            }
        }

		void Process(ref AnimationSpriteData sprite, [ReadOnly] ref AnimationData anim, float delta)
		{
			float time = sprite.time;
			int spriteImdx = sprite.spriteIndx;

			float interval = anim.interval;
			float count = anim.count;	

			time += delta;
			while(interval <= time)
			{
				time -= interval;
				spriteImdx += 1;

				//if(count <= spriteImdx)
                if(5 <= spriteImdx)
					spriteImdx = 3;//0;
			}

			sprite.time = time;
			sprite.spriteIndx = spriteImdx;
		}
	}
}