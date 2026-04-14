using Core;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Infrastructure;
public class DBInteract : DbContext
{
    
    public DbSet<ProgramData> ProgramDataTable { get; set; } = null!;

    public DbSet<User> UserTable { get; set; } = null!;
    
    public DbSet<ProgramDataHistorical> PDHTable { get; set; } = null!;
    
    public static Action? OnProgramAdded;
    
    private static readonly object _dbLock;

    public delegate void ProgramListAddedDelegate(List<ProgramData> programs, bool isDataLocal);

    public static event ProgramListAddedDelegate? OnProgramListAdded;
    
    /// <summary>
    ///Database interaction constructor. Each method creates and deletes the interaction object.
    /// </summary>
    public DBInteract()
    {
        
    }

    /// <summary>
    /// Database creation.
    /// </summary>
    /// <param name="options">Database name.</param> 
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NetVine", MockDataProducer.IsBeingUsed()? "AppMeticsMock.db" : "AppMetics.db");

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
        
        modelBuilder.Entity<User>()
            .HasKey(u => new {u.Username});
        
        modelBuilder.Entity<ProgramDataHistorical>()
            .HasKey(u => new {u.SystemName, u.ProcessName});
    }

    /// <summary>
    /// Returns a list of DB ProgramData contents
    /// </summary>
    /// <returns></returns>
    public static List<ProgramData> ListAllProgramData()
    {
        using (var db = new DBInteract())
        {
            return db.ProgramDataTable.ToList();
        }
    }
    
    /// <summary>
    /// Returns a list of DB user contents
    /// </summary>
    /// <returns></returns>
    public static List<User> ListAllUser()
    {
        using (var db = new DBInteract())
        {
            return db.UserTable.ToList();
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
    /// <param name="start">Start date.</param> 
    /// <param name="end">End date.</param> 
    /// <returns></returns>
    public static List<ProgramData> ListBetweenDates(DateTime? start, DateTime? end)
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
    public static void ClearAllProgramData()
    {
        using (var db = new DBInteract())
        {
            db.ProgramDataTable.RemoveRange(db.ProgramDataTable);
            db.SaveChanges();
        }
    }
    
    /// <summary>
    /// Wipes Database and resets it
    /// </summary>
    public static void WipeDB()
    {
        lock (_dbLock)
        {
            using (var db = new DBInteract())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
    }
    
    /// <summary>
    /// Submit an entry to the DB without an existing context.
    /// </summary>
    /// <param name="entry">Submission.</param> 
    public static void SubmitEntry<T>(T entry) where T : class
    {
        using (var db = new DBInteract())
            SubmitEntry(entry, db);
        OnProgramAdded?.Invoke();
    }
    
    /// <summary>
    /// Submit an entry to the DB with an existing context.
    /// </summary>
    /// <param name="entry">Submission.</param> 
    /// <param name="db">Context.</param> 
    public static void SubmitEntry<T>(T entry, DBInteract db) where T : class
    {
        if(EntryExists(entry, db))
        {
            //Console.WriteLine("Entry already exists!");
            return;
        }
        db.Set<T>().Add(entry);
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
    /// <returns>Bool. True if exists, false if not.</returns> 
    public static bool EntryExists<T>(T entry, DBInteract db)
    {
        if (entry is ProgramData pdEntry)
        { 
            return (db.ProgramDataTable.Find(pdEntry.SystemName, pdEntry.Date, pdEntry.ProcessName) != null);   
        }
        if (entry is User uEntry)
        { 
            return (db.UserTable.Find(uEntry.Username) != null);   
        }
        return false;
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
    /// <returns>Older entry.</returns> 
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
        if (ListAllProgramData().Count == 0)
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

            Store(dummyList, true);    
        }
        else
        {
            Console.WriteLine("DB must be empty to populate dummy data;");
        }
    }
    /// <summary>
    ///Store a list of IProgramData
    /// </summary>
    /// <param name="programs"></param>
    public static void Store(IEnumerable<ProgramData> programs, bool isLocal)
    {
        using (var db = new DBInteract())
        {
            foreach (var data in programs)
            {
                SubmitEntry(data, db);
            }    
            DBArithmetic.UpdatePDHTable(programs.ToList(), db);
        }
        OnProgramListAdded?.Invoke(programs.ToList() ,isLocal);
    }

    /// <summary>
    /// Adds unique users to the database.
    /// </summary>
    /// <remarks>This does not call save on the database, please use SaveChanges or SaveChangesAsync()</remarks>
    public void AddUser(User user)
    {
        if (!EntryExists(user, this))
            UserTable.Add(user);    
    }

    public static void Initialize()
    {
        using (var db = new DBInteract())
        {
            //Submit our local machine as a user.
            db.Database.EnsureCreated();
            User localUser = new User(SystemHistory.Instance.SystemName);
            db.AddUser(localUser);
            db.SaveChanges();
            
        }
    }

    /// <summary>
    /// Returns true if the Program Data Historical Table is empty
    /// </summary>
    public bool PDHIsEmpty()
    {
        {
            return !this.PDHTable.Any();
        }
    }
    
    /// <summary>
    /// Cecks if a specific value is contained within the Program Data Historical Table
    /// </summary>
    /// 
    public bool ValueInPDH(ProgramData data)
    {
        return this.PDHTable.Find(data.SystemName, data.ProcessName) != null;
    }
    
    /// <summary>
    /// Adds entry to Program Data Historical Table
    /// </summary>
    /// 
    public void AddPDHEntry(ProgramDataHistorical data)
    {
        this.PDHTable.Add(data);
        this.SaveChanges();
    }
}