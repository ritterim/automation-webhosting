using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RimDev.Automation.WebHosting.Utilities
{
    public static class Check
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(parameterName, "The parameter cannot be null.");
            }

            return value;
        }
    }
}
