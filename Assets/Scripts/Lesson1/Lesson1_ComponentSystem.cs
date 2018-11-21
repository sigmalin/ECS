using Unity.Entities;

namespace Lesson1
{
    [UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
    class Lesson1Group{}

    [UpdateInGroup(typeof(Lesson1Group))]
    [DisableAutoCreation]
    sealed class CountSystem : ComponentSystem
    {
        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<CountData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<CountData>();

            string msg = string.Empty;

            for(int i=0; i<source.Length; i++)
            {
                var countData = source[i];
                countData.count++;
                source[i] = countData;
            }
        }
    }

    [UpdateInGroup(typeof(Lesson1Group))]
    [UpdateAfter(typeof(CountSystem))]
    [DisableAutoCreation]
    sealed class ThresholdSystem : ComponentSystem
    {
        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.Create<CountData>(), ComponentType.ReadOnly<RangeData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<CountData>();
            var range = group.GetSharedComponentDataArray<RangeData>();

            for(int i=0; i<source.Length; i++)
            {
                var countData = source[i];
                if(range[i].max < countData.count) 
                {
                    countData.count = range[i].min;
                    source[i] = countData;
                }
            }
        }

    }

    [UpdateAfter(typeof(Lesson1Group))]
    [DisableAutoCreation]
    sealed class DisplaySystem : ComponentSystem
    {
        readonly UnityEngine.UI.Text countDownText;

        ComponentGroup group;

        public DisplaySystem(UnityEngine.UI.Text countDownText) => this.countDownText = countDownText;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.ReadOnly<CountData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<CountData>();

            string msg = string.Empty;

            for(int i=0; i<source.Length; i++)
            {
                var countData = source[i];
                msg += countData.count.ToString() + " ";
            }

            countDownText.text = msg;
        }
    }
}


