using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace WebArchive.Utils
{
    public static class LogHandler
    {
        private readonly static object LogLock = new object();

        public static void EmailLog(string message)
        {
            string head = "[" + Convert.ToString(DateTime.Now) + "] ";
            LogHandler.CreateLog(@"wwwroot/assets/logs/", "logEmail.txt", head+message);
        }

        public static void StatusCodeLog(string message)
        {
            string head = "[" + Convert.ToString(DateTime.Now) + "] ";
            LogHandler.CreateLog(@"wwwroot/assets/logs/", "logStatusCode.txt", head + message);
        }

        public static void ProjectAddLog(Models.Project project, string user)
        {
            string head = "<hr/><h5>[" + Convert.ToString(DateTime.Now) + "] Submitted by: " + user + "</h5>\n";
            string message = CreateMessage(project);
            LogHandler.CreateLog(@"wwwroot/assets/logs/projects", Convert.ToString(project.Id)+".txt", head+message);
        }

        public static void ProjectEditLog(Models.Project project, Models.Project previous, string user)
        {
            string head = "<hr/><h5>[" + Convert.ToString(DateTime.Now) + "] Edited by: " + user + "</h5>\n";
            string message = CreateMessage(project, previous);
            if (!string.IsNullOrEmpty(message))
                LogHandler.CreateLog(@"wwwroot/assets/logs/projects", Convert.ToString(project.Id) + ".txt", head + message);
        }

        public static void ProjectDeleteLog(Models.Project project, string user)
        {
            string head = "<hr/><h5>[" + Convert.ToString(DateTime.Now) + "] Deleted by: " + user + "</h5>\n";
            LogHandler.CreateLog(@"wwwroot/assets/logs/projects", Convert.ToString(project.Id) + ".txt", head);
        }

        private static string CreateMessage(Models.Project project) // for 1st submit
        {
            string message = null;
            string name = null;
            List<string> exceptions = new List<string>(){ "Keygen", "Id", "ImageUI", "Logo", "PostCreator", "PostTime", "EditTime", "Status" };
            foreach (PropertyInfo info in typeof(Models.Project).GetProperties())
            {
                string currentValue = GetStringValue(info.GetValue(project, null));
                if (!exceptions.Contains(info.Name) && !string.IsNullOrEmpty(currentValue))
                {
                    if (info.GetCustomAttribute<DisplayAttribute>(false) != null)
                        name = info.GetCustomAttribute<DisplayAttribute>(false).Name;
                    else
                        name = info.Name;
                    message += name + ": " + currentValue + "\n";
                }
            }
            return message;
        }

        private static string CreateMessage(Models.Project project, Models.Project previous) // for edit
        {
            string message = null;
            string name = null;
            List<string> exceptions = new List<string>() { "Keygen", "Id", "ImageUI", "Logo", "PostCreator", "PostTime", "EditTime", "Status" };
            foreach (PropertyInfo info in typeof(Models.Project).GetProperties())
            {
                string currentValue = GetStringValue(info.GetValue(project, null));
                string previousValue = GetStringValue(info.GetValue(previous, null));
                if (!exceptions.Contains(info.Name) && previousValue != currentValue)
                {
                    if (info.GetCustomAttribute<DisplayAttribute>(false) != null)
                        name = info.GetCustomAttribute<DisplayAttribute>(false).Name;
                    else
                        name = info.Name;
                    if (currentValue == null && previousValue != null)
                        message += name + ": -deleted- \n";
                    else
                        message += name + ": " + currentValue + "\n";
                }
            }
            return message;
        }

        private static string GetStringValue(object value)
        {
            if (value == null)
                return null;
            else
                return value.ToString();
        }

        private static void CreateLog(string directory, string fileName, string message)
        {
            lock (LogLock)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(message + "\n");
                string path = Path.Combine(Environment.CurrentDirectory, directory, fileName);
                File.AppendAllText(path, sb.ToString());
                sb.Clear();
            }
        }
    }
}
