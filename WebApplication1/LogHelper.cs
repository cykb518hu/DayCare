using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public static class LogHelper
    {
        public static ILog log = LogManager.GetLogger("log");
    }
}