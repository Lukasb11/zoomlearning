//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LearningSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class zoom_rec
    {
        public int rec_id { get; set; }
        public int meeting_id { get; set; }
        public string rec_link { get; set; }
    
        public virtual zoom_meeting zoom_meeting { get; set; }
    }
}
