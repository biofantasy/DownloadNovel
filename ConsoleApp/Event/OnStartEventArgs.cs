using System;

namespace ConsoleApp.Event
{
    public class OnStartEventArgs
    {
        /// <summary>
        /// 爬蟲URL網址
        /// </summary>
        public Uri Uri { get; set; }

        public OnStartEventArgs(Uri uri)
        {
            this.Uri = uri;
        }

    }
}
