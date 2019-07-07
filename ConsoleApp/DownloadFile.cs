using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public partial class DownloadFile
    {
        private string _FilePath;

        public string FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(value);

                if (directoryInfo.Exists)
                {
                    _FilePath = value;
                }
                else
                {
                    directoryInfo.Create();
                    throw new DirectoryNotFoundException("找不到檔案路徑 已創建資料夾 請重新執行");
                }
            }
        }

        public DownloadFile(string FilePath)
        {
            this.FilePath = FilePath;
        }

        public async void StartDownload(string FileContent, string FileName)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                using (StreamWriter sw = new StreamWriter(_FilePath + FileName + ".txt"))
                {
                    Console.WriteLine("Count:" + FileName);
                    await WriteFile(sw, FileContent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        public async Task WriteFile(StreamWriter sw, string fileContent)
        {
            await sw.WriteAsync(fileContent);
        }
    }
}
