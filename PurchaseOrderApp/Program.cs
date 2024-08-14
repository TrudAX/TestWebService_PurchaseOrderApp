using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PurchaseOrderApp.Data;
using PurchaseOrderApp.Services;

string baseDir = AppDomain.CurrentDomain.BaseDirectory;
AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(baseDir, "App_Data"));


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlite( builder.Configuration.GetConnectionString("DefaultConnection")));
/*
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    var dbPath = System.IO.Path.Join(path, "purchaseorders.db");
    options.UseSqlite($"Data Source={dbPath}");
});
*/

builder.Services.AddScoped<PurchaseOrderService>();

var app = builder.Build();
// Add this code
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var connection = context.Database.GetDbConnection();
    Console.WriteLine($"Database path: {connection.DataSource}");
}

app.UseHttpsRedirection();
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    if (!context.PurchaseOrders.Any())
    {
        var service = services.GetRequiredService<PurchaseOrderService>();
        service.SeedTestData();
    }
}

app.Run();
/*
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.Run();
*/
