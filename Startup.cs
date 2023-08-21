using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace App
{
    public class Startup
    {

        public static string RootPath {set;get;}

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            RootPath = env.ContentRootPath;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var mailsettings = Configuration.GetSection("MailSettings"); // Đọc thiết lập gửi Mail trong appsettings.json
            services.Configure<MailSettings>(mailsettings);
            services.AddSingleton<IEmailSender, SendMailService>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.Configure<RazorViewEngineOptions>(options => {
                options.ViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);
            });
            services.AddSingleton<ProductService, ProductService>();
            services.AddSingleton(typeof(PlanetService), typeof(PlanetService));

            services.AddDbContext<AppDbContext>(options =>
            {
                string connectString = Configuration.GetConnectionString("AppMvcContext");
                options.UseSqlServer(connectString);
            });

            // Đăng ký Identity với giao diện tùy biến
            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất

                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = false;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = false;         // Xác thực tất cả mới được đăng nhập
            });

            services.ConfigureApplicationCookie(options => {
                options.LoginPath = "/login"; // Trang đăng nhập
                options.LogoutPath = "/logout"; // Trang đăng xuất
                options.AccessDeniedPath = "/khongduoctruycap.html"; // Trang cấm truy cập
            });

            // Thêm dịch vụ đăng nhập từ nguồn bên ngoài
            services.AddAuthentication()
                    .AddGoogle(options => {
                        var googleConfig = Configuration.GetSection("Authentication:Google"); // Đọc options từ appsettings.json
                        options.ClientId = googleConfig["ClientId"];
                        options.ClientSecret = googleConfig["ClientSecret"];
                        options.CallbackPath = "/dang-nhap-tu-google"; // Đường dẫn tới trang đăng nhập với google
                    })
                    .AddFacebook(options => {
                        var fbConfig = Configuration.GetSection("Authentication:Facebook");
                        options.AppId = fbConfig["AppId"];
                        options.AppSecret = fbConfig["AppSecret"];
                        options.CallbackPath = "/dang-nhap-tu-facebook";
                    });
            
            services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

            services.AddAuthorization(options => {
                options.AddPolicy("ViewAdminMenu", builer => {
                    builer.RequireAuthenticatedUser();
                    builer.RequireRole(RoleName.Administrator);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions() {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
                ),
                RequestPath = "/contents"
            });

            app.AddStatusCodePage(); //Tạo ra các response từ lỗi 400 - 599

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/{controller=Home}/{action=Index}/{id?}"
                );
                
                endpoints.MapRazorPages();
            });
        }
    }
}
