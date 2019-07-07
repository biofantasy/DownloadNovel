using System;

namespace ConsoleApp
{
    public partial class DownloadFile
    {
        public int StartCharpter { get; private set; }
        public int EndCharpter { get; private set; }
        public int CharpterRange { get; private set; }

        public void SetRules(int Start, int End, int totalCount, int Range)
        {
            if (Start > totalCount)
            {
                StartCharpter = totalCount - 1;
            }
            else
            {
                StartCharpter = (Start > 0) ? Start - 1 : 0;
            }
            

            if (End > totalCount)
            {
                EndCharpter = totalCount;
            }
            else
            {
                EndCharpter = (End > 0) ? End : totalCount;
            }


            int DiffRange = EndCharpter - StartCharpter;

            if (DiffRange > 0)
            {
                CharpterRange = (DiffRange > Range) ? Range : DiffRange;
            }
            else
            {
                throw new ArgumentException("起始章節大於完結章節");
            }
        }
    }
}
