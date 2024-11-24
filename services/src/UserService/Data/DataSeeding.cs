using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class DataSeeding
	{
		public static async void SeedDefaultUser(IApplicationBuilder app)
		{
			// Getting the DBContext.
			var context
				= app.ApplicationServices.CreateScope()
				.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			// Checking for any pending migrations to be done.
			if (context.Database.GetPendingMigrations().Any())
			{
				context.Database.Migrate();
			}

			// Getting a userManager instance.
			UserManager<ApplicationUser> userManager
				= app.ApplicationServices.CreateScope()
				.ServiceProvider
				.GetRequiredService<UserManager<ApplicationUser>>();

			// Checking if there is any users. If there is no user
			// we create a new one. 
			if (!userManager.Users.Any())
			{
				Console.WriteLine("User Management Service ==> Creating Default User");
				// Defining the user.
				var newUser = new ApplicationUser()
				{
					FirstName = "Default",
					LastName = "User",
					Email = "defaultuser@itam.com",
					UserName = "defaultuser@itam.com",
					PhoneNumber = "00000000"
				};

				// Creating the user.
				var isCreated = await userManager.CreateAsync(newUser, "Start1234!");

				// Checking if the user-creation was successful.
				if (isCreated.Succeeded)
				{
					Console.WriteLine("User Management Service ==> Default User was created");
				}

				Console.WriteLine("User Management Service ==> Creating admin role");

				// Seeding admin role.
				var role = new IdentityRole();
				role.Name = "Admin";

				RoleManager<IdentityRole> roleManager
				= app.ApplicationServices.CreateScope()
				.ServiceProvider
				.GetRequiredService<RoleManager<IdentityRole>>();

				isCreated = await roleManager.CreateAsync(role);

				if (isCreated.Succeeded)
				{
					Console.WriteLine("User Management Service ==> Admin role was created.");

					var user = await userManager.FindByEmailAsync(newUser.Email);

					var isAssigned = await userManager.AddToRoleAsync(user, "Admin");

					if (isAssigned.Succeeded)
					{
						Console.WriteLine("User Management Service ==> Default User was assigned to Admin role");
					}
				}
			}
			else
			{
				Console.WriteLine("Users where found!");
			}
		}
	}