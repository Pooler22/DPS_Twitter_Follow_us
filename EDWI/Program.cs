using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EDWI
{
    internal static class Edwi
    {
        private static void Main()
        {
            var urlsList = new List<string>
            {
                "http://uczestnicy.dajsiepoznac.pl/lista"
            };

            const string htmlStartTemplate = @"<!doctype html>
                            <html lang='en'>
                            <head>
                            <meta charset='utf-8'>
                            <title>DPS Twitter Follow us</title>
                            </head>
                            <body>";
            const string htmlEndTemplate = "</body></html>";

            var regexProfileDsp = new Regex("<a [^>]*href=(?:'/profil(?<href>.*?)')|(?:\"/profil(?<href>.*?)\")", RegexOptions.IgnoreCase);
            var regexProfileTwitter = new Regex("<a [^>]*href=(?:'https://twitter.com/(?<href>.*?)')|(?:\"https://twitter.com/(?<href>.*?)\")", RegexOptions.IgnoreCase);
            const string prefixUrlProfileDsp = "http://uczestnicy.dajsiepoznac.pl/profil/";

            urlsList.ForEach(urlAddress =>
            {
                var i = 0;
                var twitterButtons = "";
                var urlsProfileDsp = regexProfileDsp
                                .Matches(urlAddress.GetDataFromUrl())
                                .OfType<Match>()
                                .Select(m => m.Groups["href"].Value);
                
                foreach (var urlProfile in urlsProfileDsp.AsParallel())
                {
                    var urlsTwitter = regexProfileTwitter
                                            .Matches((prefixUrlProfileDsp + urlProfile)
                                            .GetDataFromUrl())
                                            .OfType<Match>()
                                            .Select(m => m.Groups["href"].Value);
                    twitterButtons = Enumerable.Aggregate(urlsTwitter.AsParallel(), twitterButtons, GenerateButton());

                    Console.WriteLine(i++ + "/~800");
                }

                (htmlStartTemplate + twitterButtons + htmlEndTemplate)
                    .SaveToFile(urlAddress
                                    .GetFileNameFormUrl(".html"));
            });
        }

        private static Func<string, string, string> GenerateButton()
        {
            return (current, s) => current + ("<a class='twitter-follow-button' href='https://twitter.com/" + s + "'>Follow @" + s + "</a></br>");
        }

        private static string GetDataFromUrl(this string urlAddress1)
        {
            var request = (HttpWebRequest)WebRequest.Create(urlAddress1);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) return "";
            var receiveStream = response.GetResponseStream();
            StreamReader readStream = null;

            if (receiveStream != null) readStream = new StreamReader(receiveStream, Encoding.UTF8);
            if (readStream == null) return "";

            var data = readStream.ReadToEnd();
            response.Close();
            readStream.Close();
            return data;
        }

        private static void SaveToFile(this string data, string fileName)
        {
            var file = new StreamWriter(fileName);
            file.WriteLine(data);
            file.Close();
        }

        private static string GetFileNameFormUrl(this string url, string extension = ".txt")
        {
            return url.Replace(':', ' ').Replace('/', '_') + extension;
        }
    }
}