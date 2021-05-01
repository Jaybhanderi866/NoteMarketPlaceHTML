﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoteMarketPlace.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class NoteMarketPlaceEntities : DbContext
    {
        public NoteMarketPlaceEntities()
            : base("name=NoteMarketPlaceEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BuyerRequest> BuyerRequests { get; set; }
        public virtual DbSet<ContactU> ContactUs { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<NoteAttachement> NoteAttachements { get; set; }
        public virtual DbSet<NoteCategory> NoteCategories { get; set; }
        public virtual DbSet<NoteDetail> NoteDetails { get; set; }
        public virtual DbSet<NoteStatu> NoteStatus { get; set; }
        public virtual DbSet<NoteType> NoteTypes { get; set; }
        public virtual DbSet<SpamReport> SpamReports { get; set; }
        public virtual DbSet<Statistic> Statistics { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserProfileDetail> UserProfileDetails { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<SystemConfiguratioin> SystemConfiguratioins { get; set; }
    }
}
