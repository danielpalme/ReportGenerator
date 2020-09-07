using System;

namespace Test
{
    [CoverageExclude]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
    public class CoverageExcludeAttribute : Attribute
    {
    }
}
