using Core;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Infrastructure;
public class DBInteract : DbContext
{
    
    public DbSet<ProgramData> ProgramDataTable { get; set; } = null!;

    public static Action? OnDataAdded;
    
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
    {
        var dbLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetVine", "AppMetics.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbLocation)!);
        
        options.UseSqlite($"Data Source={dbLocation}");
    }
    
    
    
    
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
    /// Returns a list of DB contents
    /// </summary>
    /// <returns></returns>
    public static List<ProgramData> ListAll()
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.ToList();
        }
    }
    
    /// <summary>
    /// Lists all items in DB.
    /// </summary>
    /// <returns></returns>
    public static void ListAllToString()
    {
        using (var db = new DBInteract())
        {
            if(!(db.ProgramDataTable.ToList().Count == 0))
            {
                foreach(var entry in db.ProgramDataTable.ToList()){;
                    Console.WriteLine("System Name: " + entry.SystemName);
                    Console.WriteLine("Date: " + entry.Date);
                    Console.WriteLine("Process Name: " + entry.ProcessName);
                    Console.WriteLine("Cpu Usage: " + entry.CpuUsage);
                    Console.WriteLine("Disk Usage: " + entry.DiskUsage);
                    Console.WriteLine("Network Usage: " + entry.NetworkUsage);
                    Console.WriteLine("Memory Usage: " + entry.MemoryUsage + "\n");
                    
                }
            }
            else
            {
                Console.WriteLine("DB is empty\n");
            }
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
    /// Wipes ProgramDataTable data
    /// </summary>
    public static void ClearAll()
    {
        using (var db = new DBInteract())
        {
            db.ProgramDataTable.RemoveRange(db.ProgramDataTable);
            db.SaveChanges();
        }
    }
    
    /// <summary>
    /// Submit an entry to the DB without an existing context.
    /// </summary>
    /// <param name="entry"></param> Submission
    public static void SubmitEntry(ProgramData entry)
    {
        using (var db = new DBInteract())
        {
            if(EntryExists(entry))
            {
                return;
            }
            db.ProgramDataTable.Add(entry);
            db.SaveChanges();   
        }
        OnDataAdded?.Invoke();
    }
    
    /// <summary>
    /// Submit an entry to the DB with an existing context.
    /// </summary>
    /// <param name="entry"></param> Submission
    /// <param name="db"></param> Context
    public static void SubmitEntry(ProgramData entry, DBInteract db)
    {
        if(EntryExists(entry))
        {
            Console.WriteLine("Entry already exists!");
            return;
        }
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
    /// Returns ProgramData from Primary key if it exists in DB, and null if it does nut
    /// </summary>
    /// <param name="SystemName"></param>
    /// <param name="Date"></param>
    /// <param name="ProcessName"></param>
    /// <returns></returns>
    public static ProgramData? RetrieveEntry(string SystemName, DateTime Date, string ProcessName)
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.AsQueryable().FirstOrDefault(p => p.SystemName == SystemName && p.Date == Date
                && p.ProcessName == ProcessName);
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
    /// Populates a database with dummy data. Database must be empty.
    /// </summary>
    public static void PopulateDummyData()
    {
        if (ListAll().Count == 0)
        {
            List<ProgramData> dummyList = new List<ProgramData>();
            for (int i = 0; i < 10; i++)
            {
                dummyList.Add(new ProgramData()
                {
                    SystemName = Environment.MachineName,
                    Date = DateTime.Now,
                    ProcessName = "dummyProcess",
                    MemoryUsage = i,
                    CpuUsage = i,
                    DiskUsage = i,
                    NetworkUsage = i,
                    Timespan = i,
                });
            }

            Store(dummyList);    
        }
        else
        {
            Console.WriteLine("DB must be empty to populate dummy data;");
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
        OnDataAdded?.Invoke();
    }

    public static List<User> GetUsers()
    {
        List<User> users = [];
        using (var db = new DBInteract())
        {
            var usernames = db.ProgramDataTable.Select(u => u.SystemName).ToList();
            foreach (string? data in usernames)
            {
                if (string.IsNullOrEmpty(data)) continue;//Should not be possible...
                users.Add(new User { Username = data });
            }
        }
        return users;
    }
}