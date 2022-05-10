using System.ComponentModel.DataAnnotations;
using System;

namespace LearningSystem.ViewModel
{
    public class CreateCourse
    {
        [Required]
        [Display(Name = "Course name")]
        public string course_name { get; set; }

        [Required]
        [Display(Name = "Short course description")]
        public string course_desc { get; set; }

        [Required]
        [Display(Name = "Long course description")]
        public string course_desc_long { get; set; }

        [Required]
        [Display(Name = "Course length")]
        public int course_length { get; set; }

        [Display(Name = "Record on meeting start")]
        public bool auto_recording { get; set; }

        [Required]
        [Display(Name = "Start date")]
        public DateTime start_date { get; set; }
    }

}