using System;

namespace opennlp.tools.cmdline.parameters
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class OptionalAttribute : Attribute
    {
    }
}
