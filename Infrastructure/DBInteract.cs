using Microsoft.EntityFrameworkCore;
using Core;

namespace Infrastructure;
public class DBInteract : DbContext
{
    
    public DbSet<ProgramData> ProgramDataTable { get; set; } = null!;
    
    /// <summary>
    ///Database interaction constructor. Each method creates and deletes the interaction object.
    /// </summary>
    public DBInteract()
    {
        Database.EnsureCreated();
    }

    /// <summary>
    /// Database creation.
    /// </summary>
    /// <param name="options"></param> Database name.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=AppMetrics.db");
    
    /// <summary>
    /// Sets the primary key.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgramData>()
            .HasKey(u => new { u.SystemName, u.Date, u.ProcessName });
    }

    /// <summary>
    /// Lists all items in DB.
    /// </summary>
    /// <returns></returns>
    public static List<ProgramData>? ListAll()
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.ToList();
        }
    }

    /// <summary>
    ///List items between the two given dates.
    ///Will list from a Date onwards given start or end are null.
    /// </summary>
    /// <param name="start"></param> Start date.
    /// <param name="end"></param> End date.
    /// <returns></returns>
    public static List<ProgramData>? ListBetweenDates(DateTime? start, DateTime? end)
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

    /// <summary>
    /// Submit an entry to the DB without an existing context.
    /// </summary>
    /// <param name="entry"></param>
    public static void SubmitEntry(ProgramData entry)
    {
        using (var db = new DBInteract())
        {
            db.ProgramDataTable.Add(entry);
            db.SaveChanges();
        }
    }
    
    /// <summary>
    /// Submit an entry to the DB with an existing context.
    /// </summary>
    /// <param name="entry"></param>
    public static void SubmitEntry(ProgramData entry, DBInteract db)
    {
        db.ProgramDataTable.Add(entry);
        db.SaveChanges();
    }
   
    /// <summary>
    /// Delete an entry from the DB.
    /// </summary>
    /// <param name="entry"></param>
    public static void DeleteEntry(ProgramData entry)
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
    
    /// <summary>
    /// Check if entry exists in DB.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns> Bool. True if exists, false if not.
    public static bool EntryExists(ProgramData entry)
    {
        using (var db = new DBInteract())
        {
            return (db.ProgramDataTable.Find(entry.SystemName, entry.Date, entry.ProcessName) != null);
        }
    }

    /// <summary>
    /// Checks if first entry passed in is older than second entry.
    /// If so, returns first entry, if not, returns second.
    /// </summary>
    /// <param name="entry1"></param>
    /// <param name="entry2"></param>
    /// <returns></returns> Older entry.
    public static ProgramData? OlderEntry(ProgramData entry1, ProgramData entry2)
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
    /// <summary>
    ///Store a list of IProgramData
    /// </summary>
    /// <param name="info"></param> List
    public static void Store(IEnumerable<IProgramData> info)
    {
        using (var db = new DBInteract())
        {
            foreach (var data in info)
            {
                SubmitEntry((ProgramData)data, db);
            }    
        }
    }
}