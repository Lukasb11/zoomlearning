using Quartz;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using LearningSystem.Models;
using LearningSystem.ViewModel;
using Newtonsoft.Json.Linq;
using RestSharp;
namespace LearningSystem.Models
{
    public class ExecuteTaskServiceCallJob : IJob
    {
        public static readonly string SchedulingStatus = ConfigurationManager.AppSettings["ExecuteTaskServiceCallSchedulingStatus"];
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                if (SchedulingStatus.Equals("ON"))
                {

                    try
                    {
                        dbEntities _db = new dbEntities(); // 2nd entity of courses
                        List<zoom_meeting> zms = _db.zoom_meeting.ToList();
                        foreach (zoom_meeting zm in zms)
                        {
                            DateTime startDate = Convert.ToDateTime(zm.start_time);
                            DateTime currentDate = DateTime.Now;
                            DateTime futureDate = DateTime.Now.AddMinutes(5);
                            if (startDate > currentDate && startDate < futureDate)
                            {
                                List<course_subscribe> cs = _db.course_subscribe.Where(x => x.course_id.Equals(zm.course_id)).ToList();
                                course course = _db.courses.Where(x => x.course_id.Equals(zm.course_id)).FirstOrDefault();
                                foreach (course_subscribe subsc in cs)
                                {
                                   user user = _db.users.Where(x => x.user_id.Equals(subsc.user_id)).FirstOrDefault();

                                    using (var message = new MailMessage("zoomlearningnotifications@gmail.com", user.email))
                                    {
                                        message.Subject = "Your course meeting is starting soon";
                                        message.Body = "Your course meeting: '" + course.course_name + "' is starting soon. \n"
                                                     + "Please join the meeting via this link: " + zm.meeting_link + "\n";

                                        using (SmtpClient client = new SmtpClient
                                        {
                                            UseDefaultCredentials = false,
                                            EnableSsl = true,
                                            Host = "smtp.gmail.com",
                                            Port = 587,
                                            Credentials = new NetworkCredential("zoomlearningnotifications@gmail.com", "xxx")
                                        })
                                        {
                                            client.Send(message);
                                        }
                                    }
                                }
                            }
                        }                       
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
            return task;
        }
    }
}