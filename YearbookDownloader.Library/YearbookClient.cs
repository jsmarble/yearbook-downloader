using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YearbookDownloader
{
    public class YearbookClient
    {
        public static readonly string DefaultDownloadRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Yearbooks");

        public event EventHandler<ImageDownloadedEventArgs> ImageDownloaded;

        private readonly string username;
        private readonly string password;

        public YearbookClient(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public string DownloadRoot { get; set; } = DefaultDownloadRoot;

        public async Task<string> DownloadYearbook(int school, int year)
        {
            string cookie = GetCookie(username, password);

            string downloadDir = Path.Combine(DownloadRoot, school.ToString(), year.ToString());
            Directory.CreateDirectory(downloadDir);

            string baseUrl = $"http://www.e-yearbook.com/books/{school}/{year}/jpg800/";

            for (int i = 1; i < int.MaxValue; i++)
            {
                string fileName = $"{i}.jpg";
                string url = $"{baseUrl}{fileName}";
                string filePath = Path.Combine(downloadDir, fileName);
                try
                {
                    await DownloadFileAsync(url, cookie, filePath);
                    OnImageDownloaded(i);
                }
                catch (WebException ex) when (YearbookClient.Is404(ex))
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    break;
                }
            }

            return downloadDir;
        }

        public async Task<string> GetSchoolName(int school)
        {
            throw new NotImplementedException();
        }

        private async Task DownloadFileAsync(string url, string cookie, string file)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.Cookie, $"EYBSN={cookie}; EYBID={username}");
            wc.Headers.Add(HttpRequestHeader.Host, "www.e-yearbook.com");
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
            wc.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            wc.Headers.Add(HttpRequestHeader.KeepAlive, "300");
            wc.Headers.Add(HttpRequestHeader.Accept, "image/webp,image/*,*/*;q=0.8");
            wc.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
            wc.Headers.Add(HttpRequestHeader.Referer, "http://www.e-yearbook.com/sp/eybb");

            await wc.DownloadFileTaskAsync(url, file);
        }

        private string GetCookie(string username, string password)
        {
            var parms = new Dictionary<string, string>
            {
                { "acct", username },
                { "pass", password },
                { "persist", "1" },
                { "ruri", "/" },
                { "rqs", "" },
                { "rm", "" },
                { "bSignin", "Sign+In" }
            };

            string url = "https://secure.e-yearbook.com/sp/auth";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)";
            req.Method = "POST";
            req.Accept = "text/html, application/xhtml+xml, */*";
            req.Headers.Add("Accept-Language: en-us,en");
            req.Headers.Add("Accept-Encoding: gzip,deflate");
            req.Headers.Add("Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            req.KeepAlive = true;
            req.Host = "secure.e-yearbook.com";
            req.Headers.Add("Keep-Alive: 300");
            req.ContentType = "application/x-www-form-urlencoded";
            req.Referer = "http://www.e-yearbook.com/sp/auth";
            req.AddParameters(parms.ToArray());

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            string cookie = null;

            using (StreamReader reader = new StreamReader(res.GetResponseStream()))
            {
                string response = reader.ReadToEnd();
                cookie = Regex.Match(response, "EYBSN=([a-f0-9]+);")?.Groups[1].Value;
                reader.Close();
            }

            return cookie;
        }

        private static bool Is404(WebException ex)
        {
            return ex.Message.Contains("(404) Not Found");
        }

        private void OnImageDownloaded(int imageNumber)
        {
            OnImageDownloaded(new ImageDownloadedEventArgs(imageNumber));
        }

        protected virtual void OnImageDownloaded(ImageDownloadedEventArgs e)
        {
            ImageDownloaded?.Invoke(this, e);
        }
    }

    public class ImageDownloadedEventArgs : EventArgs
    {
        public ImageDownloadedEventArgs(int imageNumber)
        {
            ImageNumber = imageNumber;
        }

        public int ImageNumber { get; }
    }
}
