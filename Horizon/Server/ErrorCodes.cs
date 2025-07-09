namespace NoDev.Horizon.Net
{
    internal static class ErrorCodes
    {
        internal const uint Unreadable = 0x01;
        internal const uint Invalid = 0x02;
        internal const uint UnknownModule = 0x03;
        internal const uint UnsupportedVersion = 0x04;
        internal const uint DatabaseFailure = 0x05;
        internal const uint NoAccess = 0x06;
        internal const uint UnknownRequest = 0x07;
        internal const uint Incomplete = 0x08;

        internal static string GetMessage(uint errorCode)
        {
            switch (errorCode)
            {
                case Unreadable:
                    return "Failed to parse client data.";
                case Invalid:
                    return "Failed to validate client data.";
                case UnknownModule:
                    return "Unknown module specified.";
                case UnsupportedVersion:
                    return "This module does not support the request.";
                case DatabaseFailure:
                    return "Failed to connect to the database.";
                case NoAccess:
                    return "You do not have permission to access this module.";
                case UnknownRequest:
                    return "Unsupported request.";
                case Incomplete:
                    return "The request was incomplete.";
                default:
                    return null;
            }
        }
    }
}
