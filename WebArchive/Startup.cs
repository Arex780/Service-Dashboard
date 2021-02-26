using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Timers;
using WebArchive.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using WebArchive.Models.Ldap;
using WebArchive.Interfaces;
using WebArchive.Security;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace WebArchive
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public static Timer timer;
        public static string connectionString;
        public static int UpdateInterval;
        public static Models.Smtp.SmtpConfig smtpConfig;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            UpdateInterval = (int)TimeSpan.FromMinutes(Convert.ToInt32(Configuration.GetSection("UpdateInterval").Value)).TotalMilliseconds;
            smtpConfig = Configuration.GetSection("smtp").Get<Models.Smtp.SmtpConfig>();
            PingTimer();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Database config
            services.AddDbContext<Data.WebDataContext>(options =>
            {
                connectionString = Configuration.GetConnectionString("WebDataContext");

                // Try to set the same password for not-local database from environment variable if not provided in appsettings.json
                if (!connectionString.Contains("Password=") && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SA_PASSWORD")))
                {
                        connectionString = connectionString + ";Password=" + Environment.GetEnvironmentVariable("SA_PASSWORD");
                }

                options.UseSqlServer(connectionString);
            });

            // Admin users
            services.AddMvc(options => options.Filters.Add(new AdminFilter(Configuration.GetSection("AdminConfig"))));

            // Kestrel web hosting
            services.Configure<KestrelServerOptions>(
                Configuration.GetSection("Kestrel"));

            // Cookies Policy
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.ConsentCookie.IsEssential = true;
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // LDAP service
            services.Configure<LdapConfig>(Configuration.GetSection("ldap"));
            services.AddScoped<IAuthenticationService, LdapAuthenticationService>();
            services.AddMvc(config =>
            {
                // Requiring authenticated users on the site globally
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Authetication
            var cookiesConfig = this.Configuration.GetSection("cookies").Get<CookiesConfig>();
            services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.IsEssential = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.Name = cookiesConfig.CookieName;
                    options.LoginPath = cookiesConfig.LoginPath;
                    options.LogoutPath = cookiesConfig.LogoutPath;
                    options.AccessDeniedPath = cookiesConfig.AccessDeniedPath;
                    options.ReturnUrlParameter = cookiesConfig.ReturnUrlParameter;
                });

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsBlueBadge", policy =>
                                  policy.RequireClaim(System.Security.Claims.ClaimTypes.AuthorizationDecision, "BB")
                                        .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                                      );
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
            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.Context.Request.Path.StartsWithSegments("/assets/logs"))
                    {
                        ctx.Context.Response.Headers.Add("Cache-Control", "no-store");
                        if (!ctx.Context.User.Identity.IsAuthenticated)
                            ctx.Context.Response.Redirect("/account/login");
                    }
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Project}/{action=Index}/{id?}");
            });
        }

        public void PingTimer()
        {
            timer = new Timer(UpdateInterval); 
            timer.Elapsed += new ElapsedEventHandler(TimerTick); 
            timer.Start();
        }

        void TimerTick(object source, ElapsedEventArgs e)
        {
            StatusChecker.UpdateProjectOnlineStatus();
        }
    }
}
