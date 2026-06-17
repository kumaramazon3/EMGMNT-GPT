using Microsoft.EntityFrameworkCore;

namespace Maruwa_Emgmt.DBcontex
{
    public class LiveHRMISDbContext: DbContext
    {
        public LiveHRMISDbContext(DbContextOptions<LiveHRMISDbContext> options): base(options)
        {
        }
    }
}
