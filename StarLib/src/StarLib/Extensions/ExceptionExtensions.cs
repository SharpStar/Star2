using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StarLib.Extensions
{
    public static class ExceptionExtensions
    {
        public static void LogError(this Exception ex)
        {
            StackTrace st = new StackTrace(ex, true);
            StackFrame[] sf = st.GetFrames();
            
            //if (sf != null)
            //{
            //    foreach (StackFrame f in sf)
            //    {
            //        StarLog.DefaultLogger.Error("{0} ({1}) - {2}", f.GetFileName(), f.GetFileLineNumber(), f.GetMethod().Name);
            //    }
            //}
        }

    }
}
