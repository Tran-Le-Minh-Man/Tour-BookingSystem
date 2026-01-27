using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourBookingSystem.Utils;
using TourBookingSystem.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure session with 30-minute timeout
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".TourBooking.Session";
});

// Initialize database connection path
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "tour_booking.accdb");
string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
DBConnection.init(dbPath);

// Ensure Dual table exists for Jet provider
DatabaseInitializer.Initialize(connectionString);

// Register ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseJetOleDb(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();