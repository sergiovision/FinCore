using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FinCore;

public class Startup
{
    private readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Cors
        services.AddCors(o => o.AddPolicy(MyAllowSpecificOrigins, builder =>
        {
            string[] hosts = {"localhost", "127.0.0.1", "www.sergego.com"};

            string[] protocols = {"http", "https", Configuration["WSProtocol"]};
            var urls = new List<string>();
            foreach (var host in hosts)
            foreach (var protocol in protocols)
            {
                urls.Add(protocol + "://" + host);
                urls.Add(protocol + "://" + host + ":" + Configuration["WebPort"]);
                urls.Add(protocol + "://" + host + ":" + Configuration["MessagingPort"]);
            }

            urls.Add(Configuration["DebugClientURL"]);
#if DEBUG
            urls.Add("http://localhost:4200");
#endif
            urls.Add(Configuration["ExternalClientURL"]);
            var withoutDuplicates = new HashSet<string>();
            foreach (var url in urls) withoutDuplicates.Add(url);

            var array = new string[withoutDuplicates.Count];
            withoutDuplicates.CopyTo(array);
            builder.WithOrigins(array)
                // .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }));

        var jwtSettings = JwtSettings.FromConfiguration(Configuration);

        services.AddSingleton(jwtSettings);

        services.AddHttpContextAccessor();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddAutoMapper(GetType().Assembly);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser().Build());
            options.AddPolicy("admin", policy => policy.RequireClaim("can_delete", "true"));
            options.AddPolicy("user", policy => policy.RequireClaim("can_view", "true"));
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = jwtSettings.TokenValidationParameters);

        services.AddControllersWithViews().AddNewtonsoftJson(options =>
        {
            // Return JSON responses in LowerCase?
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            // Resolve Looping navigation properties
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });

        var angularFolder = AngularPath(Configuration["AngularDir"]);

        // In production, the Angular files will be served from this directory
        services.AddSpaStaticFiles(configuration => { configuration.RootPath = angularFolder; });

        services.AddHostedService<MessagingBackgroundService>();
        services.AddSingleton<IMessagingService, MessagingService>();
    }


    public string GetApplicationRoot()
    {
        var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return exePath;
    }

    public string AngularPath(string restPath)
    {
        // string pathToExe = "";
        //if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        //     pathToExe = Process.GetCurrentProcess().MainModule.FileName;
        // else
        //    pathToExe = GetApplicationRoot();
        var currentDir = GetApplicationRoot(); //  Path.GetDirectoryName(pathToExe);
        return Path.Combine(currentDir, restPath);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        Program.Container = app.ApplicationServices.GetAutofacRoot();
        loggerFactory.AddLog4Net();

        var angularFolder = AngularPath(Configuration["AngularDir"]);
        QuartzServer.Server.Initialize(angularFolder, env.EnvironmentName);

        app.UseRouting();

        app.UseCors(MyAllowSpecificOrigins);


        app.Use(async (context, next) =>
        {
            // Add Header
            context.Response.Headers["FinCore"] = "FinCore Web Api Self Host";
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            // Call next middleware
            await next.Invoke();
        });


        app.UseAuthentication();
        app.UseAuthorization();

#if DEBUG
        app.UseDeveloperExceptionPage();
#else
        app.UseSpaStaticFiles();
        SetupStaticAngular(app, angularFolder);
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
#endif

        // app.UseHttpsRedirection();
        app.UseDefaultFiles();

        app.UseStaticFiles();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        app.UseSpa(spa =>
        {
            // To learn more about options for serving an Angular SPA from ASP.NET Core,
            // see https://go.microsoft.com/fwlink/?linkid=864501

            spa.Options.SourcePath = angularFolder;

#if DEBUG
            //if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            //{ 
            //spa.UseAngularCliServer(npmScript: "start");
            spa.UseProxyToSpaDevelopmentServer(Configuration["DebugClientURL"]);
            //spa.UseProxyToSpaDevelopmentServer(Configuration["ExternalClientURL"]);
            //}
            //else
            //{
            //    spa.UseProxyToSpaDevelopmentServer(Configuration["DebugClientURL"]);
            //}
#endif
        });
    }

    public void SetupStaticAngular(IApplicationBuilder app, string angularFolder)
    {
        var options = new FileServerOptions();
        options.EnableDefaultFiles = true;
        options.StaticFileOptions.FileProvider = new PhysicalFileProvider(angularFolder);
        options.StaticFileOptions.ServeUnknownFileTypes = true;
        options.DefaultFilesOptions.DefaultFileNames = new[] {"index.html"};

        app.UseFileServer(options);
    }
}
