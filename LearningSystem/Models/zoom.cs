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
    
    public partial class zoom
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public zoom()
        {
            this.zoom_meeting = new HashSet<zoom_meeting>();
        }
    
        public int zoom_id { get; set; }
        public int user_id { get; set; }
        public string auth_token { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int is_active { get; set; }
    
        public virtual user user { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<zoom_meeting> zoom_meeting { get; set; }
    }
}
