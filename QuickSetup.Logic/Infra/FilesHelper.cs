using System;
using System.IO;
using System.Security.Cryptography;

namespace QuickSetup.Logic.Infra
{
    public class FilesHelper
    {
        public static string CalculateMd5(string path)
        {
            string strComputedHash;
            // generate md5 hash and compare
            // source - http://stackoverflow.com/a/10520086/426315
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    strComputedHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "‌​").ToLower();
                }
            }
            return strComputedHash;
        }
    }
}