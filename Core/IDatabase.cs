namespace Core;

public interface IDatabase
{
    /// <summary>
    /// Save program information into a database.
    /// </summary>
    /// <param name="info">A list of program data for the database to store.</param>
    public void Store(IEnumerable<IProgramData> info); 
}
