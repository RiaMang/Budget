namespace Budget.Migrations
{
    using Budget.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Budget.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Budget.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));
            ApplicationUser user;
            if (!context.Users.Any(r => r.Email == "ria@manglani.com"))
            {
                //var user = new ApplicationUser { UserName = "ria@manglani.com", Email = "ria@manglani.com" };
                //UserManager.CreateAsync(user, "Budget-1");
                Household h = new Household();
                h.Name = "Manglani";
                context.Households.Add(h);
                context.SaveChanges();
                user = new ApplicationUser
                {
                    UserName = "ria@manglani.com",
                    Email = "ria@manglani.com",

                    Name = "Ria Mang",
                    HouseholdId = h.Id
                };
                
                userManager.Create(user, "Budget-1");
                

                userManager.AddToRole(user.Id, "Admin");
            }
        }
    }
}
