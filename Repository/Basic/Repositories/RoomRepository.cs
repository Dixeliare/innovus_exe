using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class RoomRepository: GenericRepository<room>, IRoomRepository
{
    public RoomRepository(AppDbContext context) : base(context)
    {
    }
}