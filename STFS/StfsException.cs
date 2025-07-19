using System;

namespace NoDev.Stfs
{
    internal class StfsException : Exception
    {
        internal StfsException(string message)
            : base("STFS: " + message)
        {

        }

        internal StfsException(string format, params object[] args)
            : base(string.Format("STFS: " + format, args))
        {

        }
    }
}
