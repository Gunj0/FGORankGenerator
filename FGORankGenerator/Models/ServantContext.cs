using Microsoft.EntityFrameworkCore;

namespace FGORankGenerator.Models
{
  public class ServantContext : DbContext
  {
    public DbSet<Servant>? Servants { get; set; }

    public string DbPath { get; }

    public ServantContext()
    {
      var folder = Environment.SpecialFolder.LocalApplicationData;
      var path = Environment.GetFolderPath(folder);
      DbPath = Path.Join(path, "servant.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
  }
}
