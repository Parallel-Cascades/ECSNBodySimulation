using UnityEngine;

namespace ParallelCascades.Common.Runtime
{
    /// <summary>
    ///  Displays a help box above a field in the inspector.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class HelpBoxAttribute : PropertyAttribute
    {
        public readonly string helpText;

        public HelpBoxAttribute(string helpText)
        {
            this.helpText = helpText;
        }
    }
}