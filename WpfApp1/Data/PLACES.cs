//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfApp1.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class PLACES
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PLACES()
        {
            this.TICKETS = new HashSet<TICKETS>();
        }
    
        public int place_id { get; set; }
        public Nullable<int> hall_id { get; set; }
        public Nullable<int> row { get; set; }
        public Nullable<int> place { get; set; }
    
        public virtual HALLS HALLS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TICKETS> TICKETS { get; set; }
    }
}
