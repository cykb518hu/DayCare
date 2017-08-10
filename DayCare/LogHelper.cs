using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayCare
{
   public static class LogHelper
    {
        public static ILog log = LogManager.GetLogger("log");
    }
}
