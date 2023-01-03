using Entities.Configuration;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
	public class RepositoryContext : DbContext
	{
		public RepositoryContext(DbContextOptions options) : base(options)
		{
		}
		public DbSet<Company> Companies { get; set; }
		public DbSet<Employee> Employees { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Order> Orders { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new CompanyConfiguration());
			modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
			modelBuilder.ApplyConfiguration(new OrderConfiguration());
			modelBuilder.ApplyConfiguration(new ProductConfiguration());
		}
	}
}