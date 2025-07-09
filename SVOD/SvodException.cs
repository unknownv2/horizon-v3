using System;

namespace NoDev.Svod
{
    internal class SvodException : Exception
    {
        internal SvodException(string message)
            : base("SVOD: " + message)
        {

        }
    }
}
