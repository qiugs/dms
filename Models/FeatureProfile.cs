//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RMA_Docker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FeatureProfile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FeatureProfile()
        {
            this.FeatureAccessProfiles = new HashSet<FeatureAccessProfile>();
        }
    
        public System.Guid FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string FeatureRemarks { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FeatureAccessProfile> FeatureAccessProfiles { get; set; }
    }
}