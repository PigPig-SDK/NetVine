using Microsoft.EntityFrameworkCore;
using Core;

namespace Infrastructure;
public class DBInteract : DbContext
{
    
    public DbSet<IProgramData> ProgramDataTable { get; set; } = null!;
    
    //Database interaction constructor. Each method creates and deletes the interaction object.
    public DBInteract()
    {
        Database.EnsureCreated();
    }

    //Database creation.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=AppMetrics.db");
    
    //Set primary key.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IProgramData>()
            .HasKey(u => new { u.SystemName, u.Date, u.ProcessName });
    }

    //Lists all items in the DB
    public static List<IProgramData>? ListAll()
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.ToList();
        }
    }

    //List items between the two given dates.
    //Will list from a Date onwards given start or end are null
    public static List<IProgramData>? ListBetweenDates(DateTime? start, DateTime? end)
    {
        using var db = new  DBInteract();

        var query = db.ProgramDataTable.AsQueryable();

        if (end != null)
        {
            query = query.Where(p => p.Date < end);
        }
        if (start != null)
        {
            query = query.Where(p => p.Date > start);
        }

        return query.OrderBy(p => p.Date).ToList();

    }

    //Submit an entry to the DB
    public static void SubmitEntry(IProgramData entry)
    {
        using (var db = new DBInteract())
        {
            db.ProgramDataTable.Add(entry);
            db.SaveChanges();
        }
    }
   
    //Delete entry from DB
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
    
    //Check if entry exists in DB. Returns true if so, false if not
    public static bool EntryExists(IProgramData entry)
    {
        using (var db = new DBInteract())
        {
            return (db.ProgramDataTable.Find(entry.SystemName, entry.Date, entry.ProcessName) != null);
        }
    }

    //Checks if first entry passed in is older than second entry. If so, returns first entry, if not, returns second.
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