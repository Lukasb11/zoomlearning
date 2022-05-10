using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LearningSystem.Models;

namespace LearningSystem.ViewModel
{
    public class ModelsList
    {

        public class CreateModel
        {
            public course cmodel { get; set; }
            public CreateCourse CCourse { get; set; }

        }
    }
}