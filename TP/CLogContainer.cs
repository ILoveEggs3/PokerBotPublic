using Amigo.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo
{
    public class CLogContainer
    {
        private List<CLog> FFLstLogs;
        private string FFPath;

        private Bitmap FFImage;

        public Bitmap PImage
        {
            set
            {
                if (value != null)
                    FFImage = (Bitmap)value.Clone();
                else
                    FFImage = null;
            }
            get
            {
                return FFImage;
            }
        }

        public DateTime PDateCreated { private set; get; }
        public int PLogsCount { private set; get; }
        public string PPath
        {
            set
            {
                if (value.Contains("."))
                    throw new Exception("The path must not have a extension!");

                FFPath = value;
            }
            get
            {
                return FFPath;
            }
        }

        public CLogContainer(string _path)
        {
            FFLstLogs = new List<CLog>();
            PPath = _path;
            PDateCreated = DateTime.Now;
            PImage = null;
            PLogsCount = 0;
        }

        public void AddLog(CLog _log)
        {
            if (_log == null)
                throw new Exception("A log cannot be null!");

            FFLstLogs.Add(_log);
            ++PLogsCount;
        }

        public void ClearLogs()
        {
            FFLstLogs.Clear();
            PLogsCount = 0;
        }

        public string GetLogs(bool _doNewLineForEachMessage)
        {
            string result = "";

            for (int currentMessageIndex = 0; currentMessageIndex < FFLstLogs.Count; ++currentMessageIndex)
            {
                if (_doNewLineForEachMessage)
                    result += FFLstLogs[currentMessageIndex].PMessage + "\n";
                else
                    result += FFLstLogs[currentMessageIndex].PMessage;
            }

            return result;
        }
    }
}
