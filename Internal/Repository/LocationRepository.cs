using SQLite;
using LocationTrackingApp.Internal.Models;

namespace LocationTrackingApp.Internal.Repository
{
    public class LocationRepository
    {
        private SQLiteConnection _database;

        public LocationRepository()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "locations.db");
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<LocationEntry>();
        }

        public bool SaveLocation(double latitude, double longitude)
        {
            var location = new LocationEntry
            {
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };
            return _database.Insert(location) > 0;
        }

        public List<LocationEntry> GetAllLocations()
        {
            return _database.Table<LocationEntry>().ToList();
        }
    }
}