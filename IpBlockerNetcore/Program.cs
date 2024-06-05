using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IpBlockerNetcore.Data;
using Quartz;
using IpBlockerNetcore.Code;
using System.Configuration;
using System;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<IpBlockerNetcoreContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("IpBlockerNetcoreContext") ?? throw new InvalidOperationException("Connection string 'IpBlockerNetcoreContext' not found."))
//);

builder.Services.AddDbContextFactory<IpBlockerNetcoreContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("IpBlockerNetcoreContext")), ServiceLifetime.Scoped);


// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddLogging();




builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("IpAddJob");
    q.AddJob<IpAddJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("IpAddJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(15)
            .RepeatForever())
        .WithDescription("Runs every 15 seconds")
    );
});

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var firewallAddJobKey = new JobKey("FirewallAddJob");
    q.AddJob<FirewallAddJob>(opts => opts.WithIdentity(firewallAddJobKey));

    q.AddTrigger(opts => opts
        .ForJob(firewallAddJobKey)
        .WithIdentity("FirewallAddJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(60)
            .RepeatForever())
        .WithDescription("Runs every 60 seconds")
    );

    //var firewallTxtAddJobKey = new JobKey("FirewallTxtAddJob");
    //q.AddJob<FirewallTxtAddJob>(opts => opts.WithIdentity(firewallTxtAddJobKey));

    //q.AddTrigger(opts => opts
    //    .ForJob(firewallTxtAddJobKey)
    //    .WithIdentity("FirewallTxtAddJob-trigger")
    //    .StartNow()
    //    .WithSimpleSchedule(x => x
    //        .WithIntervalInHours(24)
    //        .RepeatForever())
    //    .WithDescription("Runs every 24 hours")
    //);

    var firewallListRearrangeJobKey = new JobKey("FirewallListRearrangeJob");
    q.AddJob<FirewallListRearrangeJob>(opts => opts.WithIdentity(firewallListRearrangeJobKey));

    q.AddTrigger(opts => opts
        .ForJob(firewallListRearrangeJobKey)
        .WithIdentity("FirewallListRearrangeJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithIntervalInHours(24)
            .RepeatForever())
        .WithDescription("Runs every 24 hours")
    );
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
