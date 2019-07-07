using ConsoleApp.Event;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace ConsoleApp
{
    public class SimpleCrawler
    {
        /// <summary>
        /// 爬蟲啟動事件
        /// </summary>
        public event EventHandler<OnStartEventArgs> OnStart;

        /// <summary>
        /// 爬蟲完成事件
        /// </summary>
        public event EventHandler<OnCompletedEventArgs> OnComplete;

        /// <summary>
        /// 爬蟲出錯事件
        /// </summary>
        public event EventHandler<Exception> OnError;

        /// <summary>
        /// 定義Cookie容器
        /// </summary>
        public CookieContainer CookiesContainer { get; set; }

        public async Task<string> Start(Uri uri, string proxy=null)
        {
            return await Task.Run(() => {
                var pageSource = string.Empty;
                try
                {
                    if (OnStart != null)
                        OnStart(this, new OnStartEventArgs(uri));

                    //計時的
                    var watch = new Stopwatch();
                    watch.Start();

                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Accept = "*/*";
                    request.ServicePoint.Expect100Continue = false; //加快載入速度
                    request.ServicePoint.UseNagleAlgorithm = false; //禁止Nagle算法加快載入速度
                    request.AllowWriteStreamBuffering = false;//禁止緩衝加快載入速度
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,defalte");//定義gzip壓縮頁面支持
                    request.ContentType = "applicatrion/x-www-form-urlencoded";//定義文檔類型及編碼
                    request.AllowAutoRedirect = false; //禁止自動跳轉
                    //設置User-Agent，伪裝成Google Chrome瀏覽器
                    //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
                    request.Timeout = 5000;//定義請求超时时間為5秒
                    request.KeepAlive = true;//啟用長連接
                    request.Method = "GET";

                    //設置代理伺服器IP 伪裝請求地址
                    if (proxy != null)
                        request.Proxy = new WebProxy(proxy);
                    request.CookieContainer = this.CookiesContainer;
                    request.ServicePoint.ConnectionLimit = int.MaxValue;//定義最大連接數

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        //将Cookie加入容器，保存登录状态
                        foreach (Cookie cookie in response.Cookies)
                        {
                            this.CookiesContainer.Add(cookie);
                        }

                        if (response.ContentEncoding.ToLower().Contains("gzip"))
                        {
                            using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("GBK")))
                                {
                                    pageSource = ConvertReadStream(reader);
                                }
                            }
                        }
                        else if (response.ContentEncoding.ToLower().Contains("deflate"))
                        {
                            using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("GBK")))
                                {
                                    pageSource = ConvertReadStream(reader);
                                }
                            }
                        }
                        else
                        {
                            using (Stream stream = response.GetResponseStream())//原始
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("GBK")))
                                {
                                    pageSource = ConvertReadStream(reader);
                                }
                            }
                        }

                        request.Abort();
                        watch.Stop();

                        //獲取當前任務Thread ID
                        var threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                        var milliseconds = watch.ElapsedMilliseconds;//執行時間
                        if (this.OnComplete != null)
                            OnComplete(this, new OnCompletedEventArgs(uri,threadID, milliseconds, pageSource));
                    }
                }
                catch (Exception ex)
                {
                    if (this.OnError != null)
                        this.OnError(this, ex);
                }

                return pageSource;
            });
        }

        private string ConvertReadStream(StreamReader reader)
        {
            byte[] test = Encoding.GetEncoding("GBK").GetBytes(reader.ReadToEnd());

            test = Encoding.Convert(Encoding.GetEncoding("GBK"), Encoding.UTF8, test);

            return Strings.StrConv(Encoding.UTF8.GetString(test), VbStrConv.TraditionalChinese, 0x0804);
        }
    }
}
