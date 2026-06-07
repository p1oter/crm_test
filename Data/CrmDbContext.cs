using Microsoft.EntityFrameworkCore;
using CRM.Models;

namespace CRM.Data
{
    public class CrmDbContext : DbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<BillInfo> BillInfos { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeSkills> EmployeeSkills { get; set; }
        public DbSet<GuiUser> GuiUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceGroup> ServiceGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja kluczy
            modelBuilder.Entity<BillInfo>().HasKey(b => b.ClientId);
            modelBuilder.Entity<GuiUser>().HasKey(g => g.EmployeeId);
            modelBuilder.Entity<EmployeeSkills>()
                .HasKey(es => new { es.EmployeeId, es.ServiceGroupId });

            // Konfiguracja tabel
            modelBuilder.Entity<Client>().ToTable("CLIENT");
            modelBuilder.Entity<BillInfo>().ToTable("BILLINFO");
            modelBuilder.Entity<Employee>().ToTable("EMPLOYEE");
            modelBuilder.Entity<EmployeeSkills>().ToTable("EMPLOYEE_SKILLS");
            modelBuilder.Entity<GuiUser>().ToTable("GUI_USER");
            modelBuilder.Entity<Role>().ToTable("ROLE");
            modelBuilder.Entity<Invoice>().ToTable("INVOICE");
            modelBuilder.Entity<InvoiceLine>().ToTable("INVOICE_LINE");
            modelBuilder.Entity<Payment>().ToTable("PAYMENT");
            modelBuilder.Entity<Reservation>().ToTable("RESERVATION");
            modelBuilder.Entity<Service>().ToTable("SERVICE");
            modelBuilder.Entity<ServiceGroup>().ToTable("SERVICE_GROUP");

            // Property names - mapowanie na kolumny z bazy
            modelBuilder.Entity<Client>()
                .Property(c => c.Id).HasColumnName("ID");
            modelBuilder.Entity<Client>()
                .Property(c => c.FirstName).HasColumnName("FIRST_NAME");
            modelBuilder.Entity<Client>()
                .Property(c => c.LastName).HasColumnName("LAST_NAME");
            modelBuilder.Entity<Client>()
                .Property(c => c.Email).HasColumnName("EMAIL");
            modelBuilder.Entity<Client>()
                .Property(c => c.Street).HasColumnName("STREET");
            modelBuilder.Entity<Client>()
                .Property(c => c.HouseNumber).HasColumnName("HOUSE_NUMBER");
            modelBuilder.Entity<Client>()
                .Property(c => c.ApartmentNumber).HasColumnName("APARTMENT_NUMBER");
            modelBuilder.Entity<Client>()
                .Property(c => c.PostalCode).HasColumnName("POSTAL_CODE");
            modelBuilder.Entity<Client>()
                .Property(c => c.City).HasColumnName("CITY");
            modelBuilder.Entity<Client>()
                .Property(c => c.Country).HasColumnName("COUNTRY");

            // Relacje
            modelBuilder.Entity<Client>()
                .HasOne(c => c.BillInfo)
                .WithOne(b => b.Client)
                .HasForeignKey<BillInfo>(b => b.ClientId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Supervisor)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
