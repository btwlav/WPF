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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class cinemaEntities : DbContext
    {
        public cinemaEntities()
            : base("name=cinemaEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<COUNTRIES> COUNTRIES { get; set; }
        public virtual DbSet<FILMS> FILMS { get; set; }
        public virtual DbSet<HALLS> HALLS { get; set; }
        public virtual DbSet<PLACES> PLACES { get; set; }
        public virtual DbSet<SESSIONS> SESSIONS { get; set; }
        public virtual DbSet<STYLES> STYLES { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<TICKETS> TICKETS { get; set; }
        public virtual DbSet<USERS> USERS { get; set; }
    }
}
