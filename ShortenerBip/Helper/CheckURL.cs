using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShortenerBip.Helper
{
    public class CheckURL
    {
        public async Task<bool> Check(string url)
        {            
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                try
                {
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(2);
                    var response = await client.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return false;
        }

        public static string FormatUrl(string Url, bool IncludeHttp = true)
        {

            Url = Url.ToLower();

            switch (IncludeHttp)
            {
                case true:
                    if (!(Url.StartsWith("http://") || Url.StartsWith("https://")))
                        Url = "http://" + Url;
                    break;
                case false:
                    if (Url.StartsWith("http://"))
                        Url = Url.Remove(0, "http://".Length);
                    if (Url.StartsWith("https://"))
                        Url = Url.Remove(0, "https://".Length);
                    break;
            }

            return Url;

        }


    }
}