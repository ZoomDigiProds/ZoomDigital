using InternalProj.Models;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Collections.Generic;
using System.Drawing;

namespace InternalProj.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Customer-related tables
        public DbSet<CustomerReg> CustomerRegs { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerContact> CustomerContacts { get; set; }
        public DbSet<RegionMaster> RegionMasters { get; set; }
        public DbSet<StateMaster> StateMasters { get; set; }
        public DbSet<PhoneType> PhoneTypes { get; set; }
        public DbSet<CustomerCategory> CustomerCategories { get; set; }
        public DbSet<RateTypeMaster> RateTypeMasters { get; set; }

        // Staff-related tables
        public DbSet<StaffReg> StaffRegs { get; set; }
        public DbSet<StaffAddress> StaffAddresses { get; set; }
        public DbSet<StaffContact> StaffContacts { get; set; }
        public DbSet<StaffCredentials> StaffCredentials { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<StaffPasswordHistory> StaffPasswordHistories { get; set; }
        public DbSet<LoginLogs> LoginLogs { get; set; }

        // Many-to-many for staff
        public DbSet<StaffDepartment> StaffDepartments { get; set; }
        public DbSet<StaffDesignation> StaffDesignations { get; set; }

        // Master tables
        public DbSet<DeptMaster> DeptMasters { get; set; }
        public DbSet<DesignationMaster> DesignationMasters { get; set; }
        public DbSet<Branch> Branches { get; set; }

        // Work Order–related tables
        public DbSet<AlbumSizeDetails> Albums { get; set; }
        public DbSet<ChildSubHead> ChildSubHeads { get; set; }
        public DbSet<DeliveryMaster> DeliveryMasters { get; set; }
        public DbSet<DeliveryMode> DeliveryModes { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MainHeadReg> MainHeads { get; set; }
        public DbSet<OrderVia> OrderVias { get; set; }
        public DbSet<SubHeadDetails> SubHeads { get; set; }
        public DbSet<TaxMaster> TaxMasters { get; set; }
        public DbSet<WorkOrderMaster> WorkOrders { get; set; }
        public DbSet<WorkType> WorkTypes { get; set; }
        public DbSet<WorkDetails> WorkDetails { get; set; }
        public DbSet<RateMaster> RateMasters { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<ModeOfPayment> ModeOfPayments { get; set; }
        public DbSet<OutstandingAmount> OutstandingAmounts { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<StudioCallLog> StudioCallLogs { get; set; }
        //GeneralSettings and AuditLogs
        public DbSet<GeneralSettings> GeneralSettings { get; set; }

        public DbSet<GeneralSettingsAuditLog> GeneralSettingsAuditLog { get; set; }


        //settings
        public DbSet<Dictionary> Dictionary { get; set; }
        public DbSet<Models.Region> Region { get; set; }
        public DbSet<Models.Size> Size { get; set; }
        public DbSet<Models.State> State { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Region-State Relationship
            modelBuilder.Entity<RegionMaster>()
                .HasOne(r => r.State)
                .WithMany(s => s.Regions)
                .HasForeignKey(r => r.StateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StateMaster>()
                .Property(s => s.Active)
                .HasDefaultValue("Y");

            // Customer Relationships
            modelBuilder.Entity<CustomerReg>()
                .HasOne(c => c.Address)
                .WithOne(a => a.Customer)
                .HasForeignKey<CustomerAddress>(a => a.CustomerId);

            modelBuilder.Entity<CustomerReg>()
                .HasMany(c => c.Contacts)
                .WithOne(cc => cc.Customer)
                .HasForeignKey(cc => cc.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerContact>()
                .HasOne(cc => cc.PhoneType)
                .WithMany()
                .HasForeignKey(cc => cc.PhoneTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerReg>()
                .HasOne(cc => cc.CustomerCategory)
                .WithMany()
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerReg>()
                .HasOne(c => c.Branch)
                .WithMany(b => b.CustomerRegs)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerReg>()
                .HasOne(c => c.RateTypeMaster)
                .WithMany(r => r.CustomerRegs)
                .HasForeignKey(c => c.RateTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Staff Relationships
            modelBuilder.Entity<StaffReg>()
                .HasMany(s => s.Addresses)
                .WithOne(a => a.Staff)
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffReg>()
                .HasMany(s => s.Contacts)
                .WithOne(c => c.Staff)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffReg>()
                .HasMany(s => s.Credentials)
                .WithOne(c => c.Staff)
                .HasForeignKey(c => c.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffReg>()
                .HasMany(s => s.AuditLogs)
                .WithOne(a => a.Staff)
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffContact>()
                .HasOne(c => c.PhoneType)
                .WithMany()
                .HasForeignKey(c => c.PhoneTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffReg>()
                .HasOne(s => s.Branch)
                .WithMany(b => b.StaffRegs)
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-many for Departments
            modelBuilder.Entity<StaffDepartment>()
                .HasKey(sd => new { sd.StaffId, sd.DeptId });

            modelBuilder.Entity<StaffDepartment>()
                .HasOne(sd => sd.Staff)
                .WithMany(s => s.StaffDepartments)
                .HasForeignKey(sd => sd.StaffId);

            modelBuilder.Entity<StaffDepartment>()
                .HasOne(sd => sd.Department)
                .WithMany()
                .HasForeignKey(sd => sd.DeptId);

            // Many-to-many for Designations
            modelBuilder.Entity<StaffDesignation>()
                .HasKey(sd => new { sd.StaffId, sd.DesignationId });

            modelBuilder.Entity<StaffDesignation>()
                .HasOne(sd => sd.Staff)
                .WithMany(s => s.StaffDesignations)
                .HasForeignKey(sd => sd.StaffId);

            modelBuilder.Entity<StaffDesignation>()
                .HasOne(sd => sd.Designation)
                .WithMany()
                .HasForeignKey(sd => sd.DesignationId);

            // Work Order Relationships
            modelBuilder.Entity<WorkOrderMaster>()
                .HasOne(w => w.Machine)
                .WithMany()
                .HasForeignKey(w => w.MachineId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrderMaster>()
                .HasOne(w => w.AlbumSize)
                .WithMany()
                .HasForeignKey(w => w.AlbumSizeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrderMaster>()
                .HasOne(w => w.Customer)
                .WithMany()
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrderMaster>()
                .HasOne(w => w.WorkType)
                .WithMany()
                .HasForeignKey(w => w.WorkTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrderMaster>()
                .HasOne(w => w.Staff)
                .WithMany()
                .HasForeignKey(w => w.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubHeadDetails>()
                .HasOne(s => s.MainHead)
                .WithMany()
                .HasForeignKey(s => s.MainHeadId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SubHeadDetails>()
                .HasOne(s => s.Machine)
                .WithMany()
                .HasForeignKey(s => s.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChildSubHead>()
                .HasOne(c => c.SubHead)
                .WithMany(s => s.ChildSubHeads)
                .HasForeignKey(c => c.SubHeadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values
            modelBuilder.Entity<StaffReg>()
                .Property(s => s.Active)
                .HasDefaultValue("Y");

            modelBuilder.Entity<CustomerReg>()
                .Property(c => c.Active)
                .HasDefaultValue("Y");

            modelBuilder.Entity<WorkOrderMaster>()
                .Property(w => w.Active)
                .IsRequired()
                .HasMaxLength(1)
                .IsFixedLength()
                .HasDefaultValue("Y")
                .HasAnnotation("RegularExpression", "Y|N");

            // Unique Indexes
            modelBuilder.Entity<StaffReg>()
                .HasIndex(s => s.StaffId)
                .IsUnique();
        }
    }
}
