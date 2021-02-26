using System.Net.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WebArchive.Models;
using WebArchive.Utils;

namespace WebArchive
{
    public static class StatusChecker
    {

        public static async Task<Status> CheckIfOnline(Models.Project project)
        {
            if (string.IsNullOrEmpty(project.WebAdress))
            {
                return Status.Offline;
            }
            else
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await client.GetAsync(project.WebAdress);
                }
                catch (Exception ex)
                {
                    LogHandler.StatusCodeLog("Failed to check status of \"" + project.Name + "\" (connection refused: " + ex.Message + "), Website: " + project.WebAdress);
                    return Status.Refused; // connection refused or actively blocked
                }
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                    return Status.Online; 
                else
                {
                    LogHandler.StatusCodeLog("Non-success status code of \"" + project.Name + "\" is " + response.StatusCode.ToString() + " (" + response.ReasonPhrase + 
                                             "), Website: " + project.WebAdress);
                    return Status.Offline;
                }
            }
        }
        
        public static async Task UpdateProjectOnlineStatus()
        {
            var options = new DbContextOptionsBuilder<Data.WebDataContext>()
            .UseSqlServer(Startup.connectionString)
            .Options;
            using (var database = new Data.WebDataContext(options))
            {
                foreach (Models.Project project in database.Projects)
                {
                    Status previousStatus = project.Status;
                    Status currentStatus = await CheckIfOnline(project);
                    project.Status = currentStatus;
                    if ((previousStatus == Status.Online && currentStatus == Status.Offline) ||
                        (previousStatus == Status.Offline && currentStatus == Status.Online) ||
                        (previousStatus == Status.FirstRefused && currentStatus == Status.Refused) || 
                        (previousStatus == Status.FirstRefused && currentStatus == Status.Offline) ||  
                        (previousStatus == Status.Refused && currentStatus == Status.Online))
                        SendMailAsync(project);
                    if (previousStatus == Status.Online && currentStatus == Status.Refused) 
                        project.Status = Status.FirstRefused; // first connection refused
                    database.Projects.Attach(project);
                    database.Entry(project).Property(x => x.Status).IsModified = true;
                }
                database.SaveChanges();
            }
            GC.Collect();
        }
        
        public static async Task SendMailAsync(Models.Project project)
        {
            try
            {
                var message = new MailMessage();
                string status = null;
                message.To.Add(new MailAddress(project.Owner));

                if (!string.IsNullOrEmpty(project.Authors))
                {
                    char[] separators = { ',', ' ', ';' };
                    string[] developers = project.Authors.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < developers.Length; i++)
                        message.To.Add(new MailAddress(developers[i]));
                }

                if (project.Status == Status.Online)
                    status = "<i style=\"color:green\">Online</i>";
                else if (project.Status == Status.Refused)
                    status = "<i style=\"color:red\">Offline (connection refused)</i>";
                else
                    status = "<i style=\"color:red\">Offline</i>";

                message.From = new MailAddress(Startup.smtpConfig.Sender);
                message.Subject = "[Service Dashboard] " + project.Name + " changed status";
                message.Body = "<p><b>Project name: </b><a href=\"" + DnsHandler.GetHostDNSAddress() + "/Project/" + project.Keygen + "\">" + project.Name + "</a>" + 
                "</p><p><b>Short description: </b>" + project.ShortDesc +  
                "</p><p><b>Project website: </b>" + project.WebAdress + 
                "</p><p><b>Website status: </b>" + status + "</p>" +
                "<br><p><i style=\"color:#CCCCCC\">" + DnsHandler.GetHostDNSAddress() + "</p>";
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = Startup.smtpConfig.Host;
                    smtp.Port = Startup.smtpConfig.Port;
                    smtp.EnableSsl = Startup.smtpConfig.Ssl;
                    smtp.Credentials = new NetworkCredential(Startup.smtpConfig.Username, Startup.smtpConfig.Password);
                    await smtp.SendMailAsync(message);
                }
                LogHandler.EmailLog("Email has been sent successfully about project " + project.Name);
            }
            catch (Exception exception)
            {
                LogHandler.EmailLog("Error occurred while sending email about project " + project.Name + ", Exception: " + exception.Message);
            }
        }

    }
}
