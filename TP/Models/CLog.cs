using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Amigo.Models
{
    public class CLog
    {
        public string PMessage { private set; get; }

        public CLog(string _message, bool _silenceMode = true)
        {
            if (_message == null)
                throw new Exception("The message cannot be null!");

            PMessage = _message;
            if (!_silenceMode)
                Debug.WriteLine(_message);
        }
    }
}
