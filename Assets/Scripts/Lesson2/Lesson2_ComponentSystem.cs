using Unity.Entities;

namespace Lesson2
{
    [UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.Update))]
    class Lesson2Group{}

    [UpdateInGroup(typeof(Lesson2Group))]    
    sealed class DisplaySystem : ComponentSystem
    {
        public UnityEngine.UI.Text MessageText;

        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.ReadOnly<RiderData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<RiderData>();

            string msg = string.Empty;

            for(int i=0; i<source.Length; i++)
            {
                var countData = source[i];
                msg += countData.Distance.ToString() + " ";
            }

            MessageText.text = msg;
        }
    }
}