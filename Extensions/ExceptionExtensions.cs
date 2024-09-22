using System;
using System.IO;

namespace Reporter_vCLabs
{
    public static class ExceptionExtensions
    {
        public static void Log(this Exception exception, string className, string methodName)
        {
            string logPath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\ReporterExceptionsLog.txt";

            using (StreamWriter sw = System.IO.File.AppendText(logPath))
            {
                sw.WriteLine($"{DateTime.Now}- Class Name: {className}; Method Name: {methodName}; Message: {exception.Message}");
            }

        }
    }
}
