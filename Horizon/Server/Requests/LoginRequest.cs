

using System.Collections.Generic;

namespace NoDev.Horizon.Net
{
    internal class LoginRequest : ServerRequest
    {
        internal LoginRequest(string username, string password) : base(1)
        {
            this.Parameters.Add("username", username);
            this.Parameters.Add("password", password);
        }
    }
}
