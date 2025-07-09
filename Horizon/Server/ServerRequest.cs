using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NoDev.Horizon.Net
{
    using SegmentData = Tuple<int, int, byte[]>;

    internal abstract class ServerRequest
    {
        private readonly int _moduleId;

        private readonly HttpWebRequest _request;

        internal readonly Dictionary<string, object> Parameters;

        private readonly List<SegmentData> _segments;

        private static string AuthenticationPacket;

        protected ServerRequest(int moduleId)
        {
            this._request = WebRequest.CreateHttp("http://client-prod.xboxmb.com/");
            this._request.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            this._request.Method = "POST";

            this._moduleId = moduleId;

            this.Parameters = new Dictionary<string, object>();

            this._segments = new List<SegmentData>(1);
        }

        internal void AddSegment(byte[] segmentData)
        {
            this._segments.Add(new SegmentData(segmentData.Length, 0, segmentData));
        }

        private static readonly byte[] PublicKey =
        {
            0x00
        };

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert,  
            X509Chain chain, SslPolicyErrors policyErrors)
        {
            if (policyErrors != SslPolicyErrors.None)
                return false;

            return cert.GetCertHash().SequenceEqual(PublicKey);
        }

        private static async Task<byte[]> EncryptBuffer(byte[] buffer)
        {
            var rijndael = new RijndaelManaged();
            rijndael.GenerateIV();
            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.Zeros;
            rijndael.Key = InitialKey;

            var enc = rijndael.CreateEncryptor();

            var memStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memStream, enc, CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(buffer, 0, buffer.Length);
            cryptoStream.Close();
            memStream.Close();

            return memStream.ToArray();
        }

        internal async Task<dynamic> SendAsync()
        {
            byte[] machineId = Program.MachineID;

            var requestDescriptor = new Dictionary<string, object>(4);
            requestDescriptor.Add("machineId", machineId);
            requestDescriptor.Add("sessionId", 0);
            requestDescriptor.Add("moduleId", this._moduleId);

            byte[] json = Encoding.Unicode.GetBytes(await JsonConvert.SerializeObjectAsync(this.Parameters));

            byte[] jsonEnc = await EncryptBuffer(json);

            var segments = new List<object>(this._segments.Count + 1)
            {
                new
                {
                    length = jsonEnc.Length,
                    checksum = 0
                }
            };

            long contentLength = jsonEnc.Length;

            foreach (var segmentData in this._segments)
            {
                contentLength += segmentData.Item1;
                segments.Add(new { length = segmentData.Item1, checksum = segmentData.Item2 });
            }

            requestDescriptor.Add("segments", segments);

            var rawRequestDescriptor = Encoding.Unicode.GetBytes(await JsonConvert.SerializeObjectAsync(requestDescriptor));

            byte[] encRequestDescriptor = await EncryptBuffer(rawRequestDescriptor);

            //var authKey = new byte[keyLength];

            //for (int x = 0; x < keyLength; x++)
            //    authKey[x] = (byte)(_authKey[x] ^ machineId[x]);
            
            //this._request.Headers.Add("Auth", Convert.ToBase64String(authKey));

            this._request.ContentLength = contentLength;

            var reqStream = await this._request.GetRequestStreamAsync();

            await reqStream.WriteAsync(encRequestDescriptor, 0, encRequestDescriptor.Length);

            foreach (var segmentData in this._segments)
                await reqStream.WriteAsync(segmentData.Item3, 0, segmentData.Item1);

            reqStream.Close();

            var response = (HttpWebResponse)await this._request.GetResponseAsync();

            var resStream = response.GetResponseStream();

            if (resStream == null)
                throw new NullReferenceException("HttpWebResponse is null.");

            var responseData = new byte[resStream.Length];

            await resStream.ReadAsync(responseData, 0, responseData.Length);

            return null;
        }

        private static readonly byte[] Salt = 
        {
            0x51, 0x45, 0xB1, 0xC4, 
            0x55, 0xAE, 0x62, 0x61, 
            0x4B, 0x4F, 0x20, 0x8A, 
            0x5C, 0xCA, 0x9C, 0x5A
        };

        private static readonly byte[] InitialKey =
        {
            0x19, 0x5B, 0xF9, 0x87,
            0xFF, 0x86, 0x4B, 0xC6,
            0xD9, 0xBC, 0x57, 0x1A,
            0xE7, 0x5A, 0x89, 0x25,
            0x8D, 0xBE, 0x8C, 0xD7,
            0xF8, 0x79, 0x16, 0xE6,
            0xFD, 0x96, 0xA0, 0x4E,
            0xD7, 0xF6, 0x25, 0x48
        };

        /*internal static byte[] ComputeHash(byte[] data)
        {
            ManagedHash.TransformBlock(data, 0, data.Length, null, 0);
            ManagedHash.TransformBlock(Program.MachineID, 0, 0x20, null, 0);
            ManagedHash.TransformFinalBlock(Salt, 0, Salt.Length);
            byte[] hash = ManagedHash.Hash;
            ManagedHash.Clear();
            return hash;
        }*/
    }
}
