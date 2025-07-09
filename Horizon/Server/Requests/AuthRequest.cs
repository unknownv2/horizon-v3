using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NoDev.Common.IO;

namespace NoDev.Horizon.Net
{
    internal class AuthRequest : ServerRequest
    {
        internal AuthRequest() : base(2)
        {
            
        }

        private static readonly byte[] Salt =
        {
            0x8D, 0x00, 0xB4, 0x6C,
            0xA4, 0xF9, 0xC1, 0xB1,
            0xE6, 0x69, 0x9A, 0xD8,
            0xFC, 0x55, 0x61, 0x6F
        };

        protected async Task<dynamic> SendAsyncProtected()
        {
            byte[] machineId = Program.MachineID;

            //this._request.Headers.Add("Machine-ID", Convert.ToBase64String(machineId));

            var sha = SHA1.Create();
            sha.TransformBlock(machineId, 0, 0x20, null, 0);
            sha.TransformFinalBlock(Salt, 0, Salt.Length);
            //this.SetIntegrity(sha.Hash);

            //var resp = await this._request.GetResponseAsync();

            //EndianIO io = new EndianIO(resp.GetResponseStream(), EndianType.Big);
            //byte[] serverHash = io.ReadByteArray(0x20);
            //byte[] rawAssembly = io.ReadByteArray(io.Length - 0x20);
            //io.Close();

            return null;
            //return (byte[])Assembly.Load(rawAssembly).GetModules(false)[0].GetTypes()[0].GetMethods()[0].Invoke(null, new object[] {serverHash});
        }
    }
}
