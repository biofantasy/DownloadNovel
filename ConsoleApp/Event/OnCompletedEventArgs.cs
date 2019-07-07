using System;

namespace ConsoleApp.Event
{
    public class OnCompletedEventArgs
    {
        public Uri Uri { get; private set; }

        public int ThreadID { get; private set; }

        public string PageSourceCode { get; private set; }

        public long Milliseconds { get; private set; }

        public OnCompletedEventArgs(Uri Uri, int ThreadID, long Milliseconds, string PageSourceCode)
        {
            this.Uri = Uri;
            this.ThreadID = ThreadID;
            this.Milliseconds = Milliseconds;
            this.PageSourceCode = PageSourceCode;
        }
    }
}
