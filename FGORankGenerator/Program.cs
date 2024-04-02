using FGORankGenerator.Models;

namespace FGORankGenerator
{
  public class Program
  {
    public static void Main(string[] args)
    {
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

      //BuildDB();

      app.Run();
    }

    static private void BuildDB()
    {
      using var db = new ServantContext();

      Console.WriteLine($"Database path: {db.DbPath}.");

      // Create
      Console.WriteLine("Inserting a new servant");
      db.Add(new Servant { Name = "test1" });
      db.Add(new Servant { Name = "test2" });
      db.SaveChanges();

      // Read
      Console.WriteLine("Querying for a blog");
      var servant = db.Servants
          .OrderBy(b => b.Id)
          .First();
      Console.WriteLine($"1: {servant.Name}");

    }
  }
}