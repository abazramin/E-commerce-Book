using Bullky.DataAccess.Data;
using Bullky.Repositry;
using Bullky.Repositry.IRepositry;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bullky.Utilty;
using Microsoft.AspNetCore.Identity.UI.Services;
using Bullky.DataAccess.DBinitalizer;

namespace Bullky
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();



            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //	options.UseSqlServer(builder.Configuration.GetConnectionString("cs")));

            builder.Services.AddDbContext<ApplicationDbContext>(o =>
            {
                o.UseSqlServer(builder.Configuration.GetConnectionString("cs"));
            });

			
			builder.Services.AddIdentity<IdentityUser , IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
			

			builder.Services.AddDistributedMemoryCache();
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = $"/Identity/Account/Login";
				options.LogoutPath = $"/Identity/Account/Logout";
				options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
			});

			builder.Services.AddScoped<IUintOfWork, UintOfWork>();

            builder.Services.AddScoped<IEmailSender, EmailSender>();


			builder.Services.AddScoped<IDBinitalizer, DBinitalizer>();
			builder.Services.AddRazorPages();

			builder.Services.AddSession(options => {
				options.IdleTimeout = TimeSpan.FromMinutes(100);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
			app.UseAuthorization();
			SeedDatabase();
			app.UseSession();

            app.MapRazorPages();


            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();

			void SeedDatabase()
			{
				using (var scope = app.Services.CreateScope())
				{
					var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBinitalizer>();
					dbInitializer.Initialize();
				}
			}
		}
    }
}
