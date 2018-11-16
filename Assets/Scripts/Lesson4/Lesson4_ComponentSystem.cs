using Unity.Entities;

namespace Lesson4
{
    sealed class DisplaySystem : ComponentSystem
    {
        public UnityEngine.UI.Text MessageText;

        ComponentGroup group;

        protected override void OnCreateManager()
        {
            var componentTypes = new ComponentType[] { ComponentType.ReadOnly<DummyData>() };
            group = GetComponentGroup(componentTypes);
        }

        
        protected override void OnUpdate()
        {
            var source = group.GetComponentDataArray<DummyData>();

            MessageText.text = source.Length.ToString();
        }
    }
}