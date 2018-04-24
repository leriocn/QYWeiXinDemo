using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QYWeiXinDemo.Helper
{
    class Logger
    {
        public static void Write(LogLevel level, string content)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "qywx.log", content + Environment.NewLine);
        }
    }

    enum LogLevel
    {
        Debug,
        Error
    }
}
