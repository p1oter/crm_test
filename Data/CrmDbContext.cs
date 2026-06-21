using CRM.Models;
using Microsoft.EntityFrameworkCore;

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

            // CLIENT
            modelBuilder.Entity<Client>()
                .ToTable("CLIENT")
                .HasKey(c => c.Id);
            modelBuilder.Entity<Client>()
                .Property(c => c.Id).HasColumnName("ID").ValueGeneratedOnAdd();
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

            // BILLINFO
            modelBuilder.Entity<BillInfo>()
                .ToTable("BILLINFO")
                .HasKey(b => b.ClientId);
            modelBuilder.Entity<BillInfo>()
                .Property(b => b.ClientId).HasColumnName("CLIENT_ID");
            modelBuilder.Entity<BillInfo>()
                .Property(b => b.Balance).HasColumnName("BALANCE");
            modelBuilder.Entity<BillInfo>()
                .Property(b => b.BankAccount).HasColumnName("BANK_ACCOUNT");
            modelBuilder.Entity<BillInfo>()
                .HasOne(b => b.Client)
                .WithOne(c => c.BillInfo)
                .HasForeignKey<BillInfo>(b => b.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // EMPLOYEE
            modelBuilder.Entity<Employee>()
                .ToTable("EMPLOYEE")
                .HasKey(e => e.Id);
            modelBuilder.Entity<Employee>()
                .Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Employee>()
                .Property(e => e.FirstName).HasColumnName("FIRST_NAME");
            modelBuilder.Entity<Employee>()
                .Property(e => e.LastName).HasColumnName("LAST_NAME");
            modelBuilder.Entity<Employee>()
                .Property(e => e.Position).HasColumnName("POSITION");
            modelBuilder.Entity<Employee>()
                .Property(e => e.SupervisorId).HasColumnName("SUPERVISOR_ID");
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Supervisor)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);

            // EMPLOYEE_SKILLS
            modelBuilder.Entity<EmployeeSkills>()
                .ToTable("EMPLOYEE_SKILLS")
                .HasKey(es => new { es.EmployeeId, es.ServiceGroupId });
            modelBuilder.Entity<EmployeeSkills>()
                .Property(es => es.EmployeeId).HasColumnName("EMPLOYEE_ID");
            modelBuilder.Entity<EmployeeSkills>()
                .Property(es => es.ServiceGroupId).HasColumnName("SERVICE_GROUP_ID");
            modelBuilder.Entity<EmployeeSkills>()
                .HasOne(es => es.Employee)
                .WithMany(e => e.Skills)
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EmployeeSkills>()
                .HasOne(es => es.ServiceGroup)
                .WithMany(sg => sg.EmployeeSkills)
                .HasForeignKey(es => es.ServiceGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // GUI_USER
            //modelBuilder.Entity<GuiUser>()
            //    .ToTable("GUI_USER")
            //    .HasKey(g => g.EmployeeId);
            //modelBuilder.Entity<GuiUser>()
            //    .Property(g => g.EmployeeId).HasColumnName("EMPLOYEE_ID");
            //modelBuilder.Entity<GuiUser>()
            //    .Property(g => g.RoleId).HasColumnName("ROLE_ID");
            modelBuilder.Entity<GuiUser>()
                .Property(g => g.Login).HasColumnName("LOGIN");
            modelBuilder.Entity<GuiUser>()
                .Property(g => g.PasswordHash).HasColumnName("PASSWORD_HASH");
            //modelBuilder.Entity<GuiUser>()
            //    .HasOne(g => g.Employee)
            //    .WithOne(e => e.GuiUser)
            //    .HasForeignKey<GuiUser>(g => g.EmployeeId)
            //    .OnDelete(DeleteBehavior.Cascade);
            //modelBuilder.Entity<GuiUser>()
            //    .HasOne(g => g.Role)
            //    .WithMany(r => r.GuiUsers)
            //    .HasForeignKey(g => g.RoleId);

            // ROLE
            modelBuilder.Entity<Role>()
                .ToTable("ROLE")
                .HasKey(r => r.Id);
            modelBuilder.Entity<Role>()
                .Property(r => r.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Role>()
                .Property(r => r.Name).HasColumnName("NAME");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanManageEmployees).HasColumnName("CAN_MANAGE_EMPLOYEES");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanManageClients).HasColumnName("CAN_MANAGE_CLIENTS");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanManagePrices).HasColumnName("CAN_MANAGE_PRICES");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanManageInvoices).HasColumnName("CAN_MANAGE_INVOICES");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanMakeReservations).HasColumnName("CAN_MAKE_RESERVATIONS");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanManageGuiUsers).HasColumnName("CAN_MANAGE_GUI_USERS");
            modelBuilder.Entity<Role>()
                .Property(r => r.CanManageServices).HasColumnName("CAN_MANAGE_SERVICES");

            // INVOICE
            modelBuilder.Entity<Invoice>()
                .ToTable("INVOICE")
                .HasKey(i => i.Id);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Invoice>()
                .Property(i => i.ClientId).HasColumnName("CLIENT_ID");
            modelBuilder.Entity<Invoice>()
                .Property(i => i.CreatedT).HasColumnName("CREATED_T");
            modelBuilder.Entity<Invoice>()
                .Property(i => i.DueT).HasColumnName("DUE_T");
            modelBuilder.Entity<Invoice>()
                .Property(i => i.SentToClient).HasColumnName("SENT_TO_CLIENT");
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // INVOICE_LINE
            modelBuilder.Entity<InvoiceLine>()
                .ToTable("INVOICE_LINE")
                .HasKey(il => il.Id);
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.InvoiceId).HasColumnName("INVOICE_ID");
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.ServiceId).HasColumnName("SERVICE_ID");
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.EmployeeId).HasColumnName("EMPLOYEE_ID");
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.ClientId).HasColumnName("CLIENT_ID");
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.Price).HasColumnName("PRICE");
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.Amount).HasColumnName("AMOUNT");
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(il => il.Invoice)
                .WithMany(i => i.Lines)
                .HasForeignKey(il => il.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(il => il.Service)
                .WithMany(s => s.InvoiceLines)
                .HasForeignKey(il => il.ServiceId);
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(il => il.Employee)
                .WithMany(e => e.InvoiceLines)
                .HasForeignKey(il => il.EmployeeId);
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(il => il.Client)
                .WithMany(c => c.InvoiceLines)
                .HasForeignKey(il => il.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // PAYMENT
            modelBuilder.Entity<Payment>()
                .ToTable("PAYMENT")
                .HasKey(p => p.Id);
            modelBuilder.Entity<Payment>()
                .Property(p => p.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Payment>()
                .Property(p => p.BankAccount).HasColumnName("BANK_ACCOUNT");
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount).HasColumnName("AMOUNT");
            modelBuilder.Entity<Payment>()
                .Property(p => p.CreatedT).HasColumnName("CREATED_T");
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.BillInfo)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BankAccount)
                .HasPrincipalKey(b => b.BankAccount);

            // RESERVATION
            modelBuilder.Entity<Reservation>()
                .ToTable("RESERVATION")
                .HasKey(r => r.Id);
            modelBuilder.Entity<Reservation>()
                .Property(r => r.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Reservation>()
                .Property(r => r.StartT).HasColumnName("START_T");
            modelBuilder.Entity<Reservation>()
                .Property(r => r.EmployeeId).HasColumnName("EMPLOYEE_ID");
            modelBuilder.Entity<Reservation>()
                .Property(r => r.ClientId).HasColumnName("CLIENT_ID");
            modelBuilder.Entity<Reservation>()
                .Property(r => r.ServiceId).HasColumnName("SERVICE_ID");
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Employee)
                .WithMany(e => e.Reservations)
                .HasForeignKey(r => r.EmployeeId);
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.ServiceId);

            // SERVICE
            modelBuilder.Entity<Service>()
                .ToTable("SERVICE")
                .HasKey(s => s.Id);
            modelBuilder.Entity<Service>()
                .Property(s => s.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<Service>()
                .Property(s => s.Name).HasColumnName("NAME");
            modelBuilder.Entity<Service>()
                .Property(s => s.Price).HasColumnName("PRICE");
            modelBuilder.Entity<Service>()
                .Property(s => s.ServiceGroupId).HasColumnName("SERVICE_GROUP_ID");
            modelBuilder.Entity<Service>()
                .HasOne(s => s.ServiceGroup)
                .WithMany(sg => sg.Services)
                .HasForeignKey(s => s.ServiceGroupId);

            // SERVICE_GROUP
            modelBuilder.Entity<ServiceGroup>()
                .ToTable("SERVICE_GROUP")
                .HasKey(sg => sg.Id);
            modelBuilder.Entity<ServiceGroup>()
                .Property(sg => sg.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            modelBuilder.Entity<ServiceGroup>()
                .Property(sg => sg.Name).HasColumnName("NAME");
        }
    }
}