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
    
    public partial class zoom_meeting
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public zoom_meeting()
        {
            this.zoom_rec = new HashSet<zoom_rec>();
        }
    
        public int meeting_id { get; set; }
        public int course_id { get; set; }
        public int zoom_id { get; set; }
        public string zoom_meet_id { get; set; }
        public string meeting_link { get; set; }
        public string start_time { get; set; }
        public int auto_recording { get; set; }
    
        public virtual course course { get; set; }
        public virtual zoom zoom { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<zoom_rec> zoom_rec { get; set; }
    }
}