using System;

namespace opennlp.tools.cmdline.parameters
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OptionalAttribute : Attribute
    {
    }
}
