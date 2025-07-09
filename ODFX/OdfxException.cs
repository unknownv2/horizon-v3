using System;

namespace NoDev.Odfx
{
    internal class OdfxException : Exception
    {
        internal OdfxException(string message)
            : base("ODFX: " + message)
        {

        }
    }
}
