using ConsoleApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
    class Program
    {
        public static Config Config = null;

        static void Main(string[] args)
        {
            if (Config == null)
                Config = new Config();
            try
            {
                List<Novel> novels = NovelCrawler(Config.DLConfig.UrlIndexPath);

                var download = new DownloadFile(Config.DLConfig.DownloadFilePath);
                download.SetRules(Config.DLConfig.StartCharpter, Config.DLConfig.EndCharpter,
                    novels.Count, Config.DLConfig.CharpterRangeToFile);

                int count = download.StartCharpter;
                int endpoint = download.EndCharpter;
                int range = download.CharpterRange;
                while (count < endpoint)
                {

                    List<Novel> novelsPart = novels.GetRange(count, range);

                    List<Chapter> charpterList = new List<Chapter>();
                    foreach (Novel novel in novelsPart)
                        charpterList.Add(CharpterCrawler(novel));

                    string content = string.Empty;
                    foreach (Chapter chapter in charpterList)
                    {
                        content += "===========================\r\n";
                        content += chapter.ChapterName + "\r\n";
                        content += "\r\n";
                        content += chapter.ChapterContent;
                    }

                    string filename = (range > 1) ? "No." + count + "-No." + (count + range) : "No."+(count + range);

                    download.StartDownload(content, filename);

                    count += range;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("爬蟲抓取出錯：" + ex.Message);
            }

            Console.WriteLine("================");
            Console.WriteLine("全部執行結束 請按任意鍵退出視窗");
            Console.ReadKey();
        }

        public static List<Novel> NovelCrawler(string novelUrl)
        {
            var novelStartUrl = novelUrl.Replace("index.html", "");
            var novelList = new List<Novel>();
            var novelCrawler = new SimpleCrawler();

            novelCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬蟲開始抓取地址：" + e.Uri.ToString());
            };
            novelCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬蟲抓取出錯：" + e.Message);
            };
            novelCrawler.OnComplete += (s, e) =>
            {
                //使用正則表達式清洗網頁源代碼中的數據
                var links = Regex.Matches(e.PageSourceCode, @"<a href=""(?<href>[^>s]+)"">(?<text>[^>s]+)</a>", RegexOptions.IgnoreCase);

                foreach (Match match in links)
                {
                    var novel = new Novel
                    {
                        ChapterName = match.Groups["text"].Value,
                        Uri = new Uri(novelStartUrl + match.Groups["href"].Value)
                    };

                    //將數據加入到泛型列表
                    if (!novelList.Contains(novel))
                        novelList.Add(novel);

                    //將小說章節名稱與URL顯示到Console
                    //Console.WriteLine(novel.ChapterName + "|" + novel.Uri);
                }

                Console.WriteLine("================================================");
                Console.WriteLine("Novel 爬蟲抓取完成：");
                Console.WriteLine("耗時：" + e.Milliseconds + "毫秒");
                Console.WriteLine("Thread數：" + e.ThreadID);
                Console.WriteLine("網址：" + e.Uri.ToString());
            };

            novelCrawler.Start(new Uri(novelUrl)).Wait();
            //沒被封鎖就别使用代理：60.221.50.118:8090
            //novelCrawler.Start(new Uri(novelUrl), new WebProxy("60.221.50.11", 8090)).Wait();

            return novelList;
        }

        public static Chapter CharpterCrawler(Novel novel)
        {
            var charpterUrl = novel.Uri;
            var charpter = new Chapter();
            var charpterCrawler = new SimpleCrawler();
            charpter.ChapterName = novel.ChapterName;

            charpterCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬蟲開始抓取地址：" + e.Uri.ToString());
            };
            charpterCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬蟲抓取出錯：" + e.Message);
            };
            charpterCrawler.OnComplete += (s, e) =>
            {
                //使用正則表達式清洗網頁源代碼中的數據
                var links = Regex.Matches(e.PageSourceCode, @"&nbsp;&nbsp;&nbsp;&nbsp;(?<text>[^>s]+)<br />", RegexOptions.IgnoreCase);

                string charpterContent = string.Empty;
                foreach (Match match in links)
                {
                    charpterContent += match.Groups["text"].Value + "\r\n";
                }

                links = Regex.Matches(e.PageSourceCode, @"&nbsp;&nbsp;&nbsp;&nbsp;(?<text>[^>s]+)</div>", RegexOptions.IgnoreCase);
                foreach (Match match in links)
                {
                    charpterContent += match.Groups["text"].Value + "\r\n";
                }

                charpter.ChapterContent = charpterContent;

                Console.WriteLine("================================================");
                Console.WriteLine("Charpter 爬蟲抓取完成：");
                Console.WriteLine("耗時：" + e.Milliseconds + "毫秒");
                Console.WriteLine("Thread數：" + e.ThreadID);
                Console.WriteLine("網址：" + e.Uri.ToString());
            };

            charpterCrawler.Start(charpterUrl).Wait();

            return charpter;
        }
    }

    internal class Config
    {
        /// <summary>
        /// DL is Download
        /// </summary>
        public DownloadConfig DLConfig = null;

        public Config()
        {
            string downloadConfigPath = Directory.GetCurrentDirectory() + "\\novel_download_config.json";
            if (File.Exists(downloadConfigPath))
            {
                using (var reader = new StreamReader(downloadConfigPath))
                {
                    string txt = reader.ReadToEnd();
                    DLConfig = JsonConvert.DeserializeObject<DownloadConfig>(txt);
                }
            }
            else
            {
                Console.WriteLine(string.Format("{0} not found", downloadConfigPath));
                DLConfig = new DownloadConfig()
                {
                    UrlIndexPath = string.Empty,
                    DownloadFilePath = string.Empty
                };
            }
        }

        public class DownloadConfig
        {
            public string UrlIndexPath = string.Empty;
            public string DownloadFilePath = string.Empty;
            public int StartCharpter = 0;
            public int EndCharpter = 0;
            public int CharpterRangeToFile = 250;
        }
    }
}
