using Books.Areas.Identity.Data;
using Books.Data;
using Books.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;

namespace Books
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<BooksContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("BooksContext") ?? throw new InvalidOperationException("Connection string 'BooksContext' not found.")));

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            builder.Services.AddRazorPages();

            builder.Services.AddIdentity<BooksUser, IdentityRole>()
                .AddEntityFrameworkStores<BooksContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                })
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Books}/{action=Index}/{id?}");

            app.MapRazorPages();

            await InitializeAsync(app.Services);

            app.Run();
        }

        public static async Task CreateUserRoles(UserManager<BooksUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            IdentityResult roleResult;

            // Check if the Admin role exists, and create it if it doesn't
            var roleCheck = await roleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Check if the admin user exists, and create it if it doesn't
            BooksUser adminUser = await userManager.FindByEmailAsync("admin@books.com");
            if (adminUser == null)
            {
                var user = new BooksUser
                {
                    Email = "admin@books.com",
                    UserName = "admin@books.com"
                };
                string userPassword = "Admin123!";
                IdentityResult checkUser = await userManager.CreateAsync(user, userPassword);

                if (checkUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Check if the User role exists, and create it if it doesn't
            var roleCheck2 = await roleManager.RoleExistsAsync("User");
            if (!roleCheck2)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Check if the regular user exists, and create it if it doesn't
            BooksUser regularUser = await userManager.FindByEmailAsync("user@books.com");
            if (regularUser == null)
            {
                var user = new BooksUser
                {
                    Email = "user@books.com",
                    UserName = "user@books.com"
                };
                string userPWD = "Booksuser123!";
                IdentityResult checkUser = await userManager.CreateAsync(user, userPWD);

                if (checkUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<BooksContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<BooksUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();
                await CreateUserRoles(userManager, roleManager);
            }
        }
    }
}