
using BookStore.DataAccess.Seeds;
using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //this section for seeding data in database------ Start here ------
            modelBuilder.Entity<Category>().HasData(
                DefaultCategory.Categories
                //Alternatives
                //new Category { Id = 1, Name = "Action", DisplayOrder = 100 } // If want to add single data row--            
                );

            modelBuilder.Entity<Product>().HasData(
                DefaultProduct.Products
                );

            //this section for seeding data in database------ End here ------

        }


        public DbSet<Category> Categories { get; set; } 
        public DbSet<Product>  Products { get; set; }
        public DbSet<Company>   Companies { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}
