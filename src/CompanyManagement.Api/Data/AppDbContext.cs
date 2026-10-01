using CompanyManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CompanyManagement.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options) {
 public DbSet<Cliente> Clientes => Set<Cliente>(); public DbSet<Orden> Ordenes => Set<Orden>();
 protected override void OnModelCreating(ModelBuilder b) { base.OnModelCreating(b); b.Entity<Cliente>(e=>{e.ToTable("Clientes"); e.HasKey(x=>x.Id); e.Property(x=>x.Nombre).HasMaxLength(150).IsRequired(); e.Property(x=>x.Email).HasMaxLength(256).IsRequired(); e.HasIndex(x=>x.Email).IsUnique();}); b.Entity<Orden>(e=>{e.ToTable("Ordenes"); e.HasKey(x=>x.Id); e.Property(x=>x.MontoTotal).HasPrecision(18,2); e.HasOne(x=>x.Cliente).WithMany(x=>x.Ordenes).HasForeignKey(x=>x.ClienteId).OnDelete(DeleteBehavior.Restrict);}); }
}
