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

namespace LearningSystem.Controllers
{
    public class CourseController : Controller
    {
        private dbEntities _db = new dbEntities(); 

        public ActionResult Index(string sortOrder, string searchString)

        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.DescSortParm = sortOrder == "Description" ? "desc_desc" : "Description";
                ViewBag.LengthSortParm = sortOrder == "Length" ? "length_desc" : "Length";
                ViewBag.TimeSortParm = sortOrder == "Start" ? "start_desc" : "Start";
                List<course> courses = _db.courses.ToList();
                if (!String.IsNullOrEmpty(searchString))
                {
                    courses = courses.Where(s => s.course_name.Contains(searchString)
                                           || s.course_desc.Contains(searchString)).ToList();
                }
                switch (sortOrder)
                {
                    case "name_desc":
                        courses = courses.OrderByDescending(s => s.course_name).ToList() ;
                        break;
                    case "desc_desc":
                        courses = courses.OrderByDescending(s => s.course_desc).ToList();
                        break;
                    case "Description":
                        courses = courses.OrderBy(s => s.course_desc).ToList();
                        break;
                    case "length_desc":
                        courses = courses.OrderByDescending(s => s.course_length).ToList();
                        break;
                    case "Length":
                        courses = courses.OrderBy(s => s.course_length).ToList();
                        break;
                    case "start_desc":
                        courses = courses.OrderByDescending(s => s.start_time).ToList();
                        break;
                    case "Start":
                        courses = courses.OrderBy(s => s.start_time).ToList();
                        break;
                    default:
                        courses = courses.OrderBy(s => s.course_name).ToList();
                        break;
                }

                return View(courses);
            }
            else
            {
                return View("../Auth/Login");
            }
        }


        public ActionResult MyCourses()

        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
                List<course> crs = _db.courses.Where(x => x.user_id.Equals(userId)).ToList();
                return View(crs);
            }
            else
            {
                return View("../Auth/Login");
            }
        }

        public ActionResult SubscribedCourses()

        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
                List < course_subscribe > sub = _db.course_subscribe.Where(x => x.user_id.Equals(userId)).ToList();
                var idList = new int[sub.Count];
                int i = 0;
                foreach(course_subscribe subsc in sub)
                {
                    idList[i] = subsc.course_id;
                    i++;
                }
                List<course> crs = _db.courses.Where(x => idList.Contains(x.course_id)).ToList();
                return View(crs);
            }
            else
            {
                return View("../Auth/Login");
            }
        }
        public ActionResult Zoom(int? id)

        {
            var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var course = _db.courses.Where(x => x.user_id == userId && x.course_id == id).FirstOrDefault();
            String rez = "";
            if (course != null)
            {
                var zoom = _db.zooms.Where(x => x.user_id == userId && x.is_active.Equals(1)).FirstOrDefault();
                if (zoom != null)
                {

                    var zoom_meeting = _db.zoom_meeting.Where(x => x.course_id.Equals(course.course_id)).FirstOrDefault();

                    if (zoom_meeting != null)
                    {
                        rez = "Zoom link is already created for this meeting: " + zoom_meeting.meeting_link;
                    }
                    else
                    {
                        refreshTokens(zoom);
                       // rez = generateMeeting(zoom, course.course_id);
                    }
                }
                else
                {
                    rez = "Zoom is currently not authenticated for the account. Please connect in settings.";
                }
            }
            else
            {
                rez = "Selected course does not exist or you don't have access to it.";
            }

            return View((object)rez);
        }
    
        public ActionResult ZoomRecording(int? id)

        {
            var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var course = _db.courses.Where(x => x.user_id == userId && x.course_id == id).FirstOrDefault();
            var zoom_meeting = _db.zoom_meeting.Where(x => x.course_id.Equals(course.course_id)).FirstOrDefault();
            String rez = "";

            if (zoom_meeting != null)
            {
                var zoom_rec = _db.zoom_rec.Where(x => x.meeting_id.Equals(zoom_meeting.meeting_id)).FirstOrDefault();
                if (zoom_rec != null)
                {
                    rez = "Recording link for the meeting: " + zoom_meeting.meeting_link;
                }
                else
                {
                    var zoom = _db.zooms.Where(x => x.user_id == userId && x.is_active.Equals(1)).FirstOrDefault();
                    refreshTokens(zoom);
                    rez = requestRecording(zoom, zoom_meeting.meeting_id, zoom_meeting.zoom_meet_id);
                }
            }
            else
            {
                rez = "Meeting has not started yet.";
            }


            return View((object)rez);
        }

        public ActionResult StartCloudRecording(int? id)
        {
            var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var course = _db.courses.Where(x => x.user_id == userId && x.course_id == id).FirstOrDefault();
            var zoom_meeting = _db.zoom_meeting.Where(x => x.course_id.Equals(course.course_id)).FirstOrDefault();
            String rez = "";
            if (zoom_meeting != null)
            {
                var zoom = _db.zooms.Where(x => x.user_id == userId && x.is_active.Equals(1)).FirstOrDefault();
                refreshTokens(zoom);
                rez = requestStartRec(zoom, zoom_meeting.zoom_meet_id);
            }
            else
            {
                rez = "Recording cannot be started for selected meeting.";
            }
            return View((object)rez);
        }

        public void refreshTokens(zoom zoom)
        {
            RestRequest restRequest = new RestRequest();
            var client = new RestClient("https://zoom.us/oauth/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic XXX");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", zoom.refresh_token);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                JObject joResponse = JObject.Parse(response.Content);
                zoom.access_token = joResponse["access_token"].ToString().Replace("{", "").Replace("}", "");
                zoom.refresh_token = joResponse["refresh_token"].ToString().Replace("{", "").Replace("}", "");
                _db.SaveChanges();
            }
            else
            {
                // TODO bad response
            }
        }

        public string generateMeeting(zoom zoom,course course, CreateCourse cd)
        {
            string rez ="";
            zoom_meeting zm = new zoom_meeting();

            var client = new RestClient("https://api.zoom.us/v2/users/me/meetings");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + zoom.access_token);
            JObject job = new JObject();
            JObject settings = new JObject();
            job["topic"] = course.course_name;
            job["type"] = "2";
            job["start_time"] = cd.start_date.ToString("u").Replace(" ","T");
            //job["start_time"] = "2022-04-25T07:32:00Z";
            job["timezone"] = "Europe/Moscow";
            job["duration"] = course.course_length;
            if (cd.auto_recording)
            {
                settings["auto_recording"] = "local";
                job["settings"] = settings;
            }

            request.AddParameter("application/json", job.ToString(), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                Console.WriteLine(response.Content);
                JObject joResponse = JObject.Parse(response.Content);
                rez = joResponse["join_url"].ToString().Replace("{", "").Replace("}", "");
                string zoom_meet_id = joResponse["id"].ToString().Replace("{", "").Replace("}", "");
                zm.meeting_link = rez;
                zm.zoom_meet_id = zoom_meet_id;
                zm.course_id = course.course_id;
                zm.zoom_id = zoom.zoom_id;
                zm.auto_recording = cd.auto_recording ? 1 : 0;
                zm.start_time = cd.start_date.ToString();
                _db.zoom_meeting.Add(zm);
                _db.SaveChanges();
                Subscribe(course.course_id);
            }
            else
            {
                rez = "Something went wrong!";
            }
            return rez;
        }

        public string requestRecording(zoom zoom, int meeting_id, string zoom_meeting_id)
        {
            string rez = "";
            zoom_rec zr = new zoom_rec();

            var client = new RestClient("https://api.zoom.us/v2/meetings/" + zoom_meeting_id + "/recordings");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", "Bearer " + zoom.access_token);

            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                JObject joResponse = JObject.Parse(response.Content);
                rez = joResponse["recording_files"][0]["play_url"].ToString().Replace("{", "").Replace("}", "");
                zr.rec_link = rez;
                zr.meeting_id = meeting_id;
                _db.zoom_rec.Add(zr);
                _db.SaveChanges();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                rez = "Meeting is not ready yet.";
            }
            else
            {
                rez = "Something went wrong!";
            }
            return rez;
        }

        public string requestStartRec(zoom zoom, string zoom_meeting_id)
        {
            string rez = "";

            var client = new RestClient("https://api.zoom.us/v2/live_meetings/" + zoom_meeting_id + "/events");
            client.Timeout = -1;
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + zoom.access_token);
            var body = @"{
                        " + "\n" +
                        @"    ""method"": ""recording.start""
                        " + "\n" +
                        @"}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                rez = "Recording started!";
            }
            else
            {
                rez = "Recording cannot be started for selected meeting.";
            }
            return rez;
        }



        // GET: /Home/Details/5 

        public ActionResult Details(int? id)

        {
            if (id == null)
                return RedirectToAction("Index");
            var meeting = _db.zoom_meeting.Where(x => x.course_id == id).FirstOrDefault();
            if (meeting != null)
            {
                ViewBag.ZoomLink = meeting.meeting_link;
                var recording = _db.zoom_rec.Where(x => x.meeting_id == meeting.meeting_id).FirstOrDefault();
                if (recording != null)
                {
                    ViewBag.RecordingLink = recording.rec_link;
                }
                else
                {
                    ViewBag.RecordingLink = "Recording link has not yet been created";
                }
            }
            else
            {
                ViewBag.ZoomLink = "Zoom meeting is not yet created";
                ViewBag.RecordingLink = "Zoom meeting is not yet created";
            }
           

            var data = _db.courses.Where(x => x.course_id == id).FirstOrDefault();

            var subs = _db.course_subscribe.Where(x => x.course_id.Equals(data.course_id));
            ViewBag.SubAmount = subs.Count();
            ViewBag.CourseID = id;

            return View(data);

        }

        //

        // GET: /Home/Create 

        public ActionResult Create()

        {

            return View("Create");

        }

        //

        // POST: /Home/Create 

        [HttpPost]
        public ActionResult Create(CreateCourse CourseDetails)

        {
            if (ModelState.IsValid)
            {

                course newcourse = new course();

                var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
                newcourse.user_id = userId;
                newcourse.course_name = CourseDetails.course_name;
                newcourse.course_desc = CourseDetails.course_desc;
                newcourse.course_desc_long = CourseDetails.course_desc_long;
                newcourse.course_length = CourseDetails.course_length;
                newcourse.start_time = CourseDetails.start_date.ToString();

                _db.courses.Add(newcourse);
                _db.SaveChanges();

                string rez = null;
                var zoom = _db.zooms.Where(x => x.user_id == userId && x.is_active.Equals(1)).FirstOrDefault();
                if (zoom != null)
                {

                    var zoom_meeting = _db.zoom_meeting.Where(x => x.course_id.Equals(newcourse.course_id)).FirstOrDefault();

                    if (zoom_meeting != null)
                    {
                        rez = "Zoom link is already created for this meeting: " + zoom_meeting.meeting_link;
                    }
                    else
                    {
                        refreshTokens(zoom);
                        rez = generateMeeting(zoom, newcourse, CourseDetails);
                    }
                }
                else
                {
                    rez = "Zoom is currently not authenticated for the account. Please connect in settings.";
                }

                return RedirectToAction("MyCourses");
            }
            else
            {

                return View("Create", CourseDetails);
            }

        }

        public ActionResult Subscribe(int? id)

        {
            string rez;
            var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var course = _db.courses.Where(x => x.course_id == id).FirstOrDefault();
            var sub = _db.course_subscribe.Where(x => x.user_id == userId && x.course_id == id).FirstOrDefault();
            if (sub == null)
            {
                course_subscribe newsub = new course_subscribe();
                newsub.user_id = userId;
                newsub.course_id = course.course_id;

                _db.course_subscribe.Add(newsub);
                _db.SaveChanges();
                rez = "You have been successfully subscribed to selected course.";
            }
            else
            {
                rez = "You are already subscribed to this course.";
            }

            return View((object)rez);
        }

        public ActionResult Unsubscribe(int? id)

        {
            string rez;
            var userId = int.Parse(System.Web.HttpContext.Current.User.Identity.Name);
            var course = _db.courses.Where(x => x.course_id == id).FirstOrDefault();
            var sub = _db.course_subscribe.Where(x => x.user_id == userId && x.course_id == id).FirstOrDefault();
            if (sub != null)
            {
                _db.course_subscribe.Remove(sub);
                _db.SaveChanges();
                rez = "You have been successfully unsubscribed from the selected course.";
            }
            else
            {
                rez = "You are not subscribed to this course.";
            }

            return View((object)rez);
        }

        public ActionResult SubscribersList(int? id)
        {
            List<course_subscribe> cs = _db.course_subscribe.Where(x => x.course_id.Equals((int)id)).ToList();
            List<user> users = new List<user>();
            foreach (course_subscribe subsc in cs)
            {
                users.Add(_db.users.Where(x => x.user_id.Equals(subsc.user_id)).FirstOrDefault());
            }
            return View(users);
        }

        //

        // GET: /Home/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)

        {
            if (id == null)
                return RedirectToAction("Index");

            var data = _db.courses.Where(x => x.course_id == id).FirstOrDefault();
            return View(data);
        }

        //

        // POST: /Home/Edit/5 

        [AcceptVerbs(HttpVerbs.Post)]

        public ActionResult Edit(int id, course Model)
        {
            try
            {
                var data = _db.courses.Where(x => x.course_id == id).FirstOrDefault();
                if (data != null)
                {
                    data.course_name = Model.course_name;
                    data.course_desc = Model.course_desc;
                    data.course_desc_long = Model.course_desc_long;
                    data.course_length = Model.course_length;
                    _db.SaveChanges();
                }

                return RedirectToAction("index");
            }
            catch
            {
                return View();
            }

        }


        public ActionResult Delete(int id)
        {
            var meeting = _db.zoom_meeting.Where(x => x.course_id == id).FirstOrDefault();
            var course = _db.courses.Where(x => x.course_id == id).FirstOrDefault();
            List<course_subscribe> cslist = _db.course_subscribe.Where(x => x.course_id == id).ToList();
            if (cslist != null)
            {
                foreach (course_subscribe cs in cslist)
                {
                    _db.course_subscribe.Remove(cs);
                }
            }
            if (meeting != null)
            {
                var zoom_rec = _db.zoom_rec.Where(x => x.meeting_id == meeting.meeting_id).FirstOrDefault();
                if (zoom_rec != null)
                    _db.zoom_rec.Remove(zoom_rec);
                _db.zoom_meeting.Remove(meeting);
            }
            _db.courses.Remove(course);
            _db.SaveChanges();

            return RedirectToAction("index");
        }
    }
}