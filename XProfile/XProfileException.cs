using System;

namespace NoDev.XProfile
{
    internal class XProfileException : Exception
    {
        internal XProfileException(string message)
            : base("XProfile: " + message)
        {

        }
    }
}
