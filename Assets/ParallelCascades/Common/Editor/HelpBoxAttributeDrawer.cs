using ParallelCascades.Common.Runtime;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ParallelCascades.Common.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : UnityEditor.PropertyDrawer
    {
        public override UnityEngine.UIElements.VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property)
        {
            var root = new VisualElement();
            
            HelpBoxAttribute attr = (HelpBoxAttribute)attribute;
            
            root.Add(new HelpBox(attr.helpText, HelpBoxMessageType.Info));
            root.Add(new PropertyField(property));
            
            return root;
        }
    }
}