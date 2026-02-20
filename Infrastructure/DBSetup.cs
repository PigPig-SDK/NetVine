using Microsoft.EntityFrameworkCore;
using Core;

namespace Infrastructure;
public class DBInteract : DbContext
{
    
    public DbSet<IProgramData> ProgramDataTable { get; set; } = null!;
    
    public DBInteract()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=AppMetrics.db");
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IProgramData>()
            .HasKey(u => new { u.SystemName, u.Date, u.ProcessName });
    }

    public static List<IProgramData>? ListAll()
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.ToList();
        }
    }

    public static List<IProgramData>? ListBetweenDates(IProgramData? entry1, IProgramData? entry2)
    {
        using var db = new  DBInteract();

        if (entry1 != null && entry2 != null)
        {
            return db.ProgramDataTable.Where(p => p.Date > entry1.Date && p.Date < entry2.Date).OrderBy(p => p.Date).ToList();    
        }

        if (entry1 == null && entry2 != null)
        {
            return db.ProgramDataTable.Where(p => p.Date < entry2.Date).OrderBy(p => p.Date).ToList();
        }

        if (entry2 == null && entry1 != null)
        {
            return db.ProgramDataTable.Where(p => p.Date > entry1.Date).OrderBy(p => p.Date).ToList();
        }

        return null;

    }

    public static void SubmitEntry(IProgramData entry)
    {
        using (var db = new DBInteract())
        {
            db.ProgramDataTable.Add(entry);
            db.SaveChanges();
        }
    }
   
    public static void DeleteEntry(IProgramData entry)
    {
        using (var db = new DBInteract())
        {
            var entryInternal = db.ProgramDataTable.Find(entry.SystemName, entry.Date, entry.ProcessName);
            if(entryInternal != null)
            {
                db.ProgramDataTable.Remove(entryInternal);
                db.SaveChanges();
            }
        }
    }
    
    public static bool EntryExists(IProgramData entry)
    {
        using (var db = new DBInteract())
        {
            return (db.ProgramDataTable.Find(entry.SystemName, entry.Date, entry.ProcessName) != null);
        }
    }

    public static IProgramData? OlderEntry(IProgramData entry1, IProgramData entry2)
    {
        using (var db = new DBInteract())
        {
            if (db.ProgramDataTable.Find(entry1.SystemName, entry1.Date, entry1.ProcessName) != null &&
                (db.ProgramDataTable.Find(entry2.SystemName, entry2.Date, entry2.ProcessName) != null))
            {
                if (entry1.Date < entry2.Date)
                {
                    return entry1;
                }
                return entry2;
            }
            return null;
        }
    }
    
}