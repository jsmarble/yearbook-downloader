using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YearbookDownloader
{
    public static class Extensions
    {
        public static void AddParameters(this WebRequest source, params KeyValuePair<string, string>[] parms)
        {
            string parmsString = string.Join("&", parms.Select(p => string.Format("{0}={1}", p.Key, p.Value)));
            source.ContentLength = parmsString.Length;

            using (StreamWriter writer = new StreamWriter(source.GetRequestStream()))
            {
                writer.Write(parmsString);
                writer.Close();
            }
        }
    }
}
