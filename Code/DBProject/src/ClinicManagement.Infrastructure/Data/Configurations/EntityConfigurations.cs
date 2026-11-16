using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class LoginTableConfiguration : IEntityTypeConfiguration<LoginTable>
{
    public void Configure(EntityTypeBuilder<LoginTable> builder)
    {
        builder.ToTable("LoginTable");
        builder.HasKey(l => l.LoginId);
        builder.Property(l => l.LoginId).HasColumnName("LoginID").ValueGeneratedOnAdd();
        builder.Property(l => l.Password).IsRequired().HasMaxLength(20);
        builder.Property(l => l.Email).IsRequired().HasMaxLength(30);
        builder.HasIndex(l => l.Email).IsUnique();
        builder.Property(l => l.Type).IsRequired();
    }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patient");
        builder.HasKey(p => p.PatientId);
        builder.Property(p => p.PatientId).HasColumnName("PatientID").ValueGeneratedNever();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(30);
        builder.Property(p => p.Phone).HasMaxLength(11);
        builder.Property(p => p.Address).HasMaxLength(40);
        builder.Property(p => p.BirthDate).IsRequired();
        builder.Property(p => p.Gender).IsRequired().HasMaxLength(1);

        builder.HasOne(p => p.Login)
               .WithOne(l => l.Patient)
               .HasForeignKey<Patient>(p => p.PatientId);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Department");
        builder.HasKey(d => d.DeptNo);
        builder.Property(d => d.DeptNo).ValueGeneratedNever();
        builder.Property(d => d.DeptName).IsRequired().HasMaxLength(30);
        builder.HasIndex(d => d.DeptName).IsUnique();
        builder.Property(d => d.Description).HasMaxLength(1000);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctor");
        builder.HasKey(d => d.DoctorId);
        builder.Property(d => d.DoctorId).HasColumnName("DoctorID").ValueGeneratedNever();
        builder.Property(d => d.Name).IsRequired().HasMaxLength(30);
        builder.Property(d => d.Phone).HasMaxLength(11);
        builder.Property(d => d.Address).HasMaxLength(40);
        builder.Property(d => d.BirthDate).IsRequired();
        builder.Property(d => d.Gender).IsRequired().HasMaxLength(1);
        builder.Property(d => d.ChargesPerVisit).HasColumnName("Charges_Per_Visit").IsRequired();
        builder.Property(d => d.MonthlySalary).HasColumnName("MonthlySalary");
        builder.Property(d => d.ReputeIndex).HasColumnName("ReputeIndex");
        builder.Property(d => d.PatientsTreated).HasColumnName("Patients_Treated").HasDefaultValue(0);
        builder.Property(d => d.Qualification).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Specialization).HasMaxLength(100);
        builder.Property(d => d.WorkExperience).HasColumnName("Work_Experience");
        builder.Property(d => d.Status).IsRequired();

        builder.HasOne(d => d.Department)
               .WithMany(dept => dept.Doctors)
               .HasForeignKey(d => d.DeptNo);
    }
}

public class OtherStaffConfiguration : IEntityTypeConfiguration<OtherStaff>
{
    public void Configure(EntityTypeBuilder<OtherStaff> builder)
    {
        builder.ToTable("OtherStaff");
        builder.HasKey(s => s.StaffId);
        builder.Property(s => s.StaffId).HasColumnName("StaffID").ValueGeneratedOnAdd();
        builder.Property(s => s.Name).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Phone).HasMaxLength(11);
        builder.Property(s => s.Address).HasMaxLength(30);
        builder.Property(s => s.Designation).IsRequired().HasMaxLength(15);
        builder.Property(s => s.Gender).IsRequired().HasMaxLength(1);
        builder.Property(s => s.HighestQualification).HasColumnName("Highest_Qualification").HasMaxLength(50);
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointment");
        builder.HasKey(a => a.AppointId);
        builder.Property(a => a.AppointId).HasColumnName("AppointID").ValueGeneratedOnAdd();
        builder.Property(a => a.AppointmentStatus).HasColumnName("Appointment_Status");
        builder.Property(a => a.BillAmount).HasColumnName("Bill_Amount");
        builder.Property(a => a.BillStatus).HasColumnName("Bill_Status").HasMaxLength(10);
        builder.Property(a => a.Disease).HasMaxLength(100);
        builder.Property(a => a.Progress).HasMaxLength(100);
        builder.Property(a => a.Prescription).HasMaxLength(100);

        builder.HasOne(a => a.Doctor)
               .WithMany(d => d.Appointments)
               .HasForeignKey(a => a.DoctorId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
