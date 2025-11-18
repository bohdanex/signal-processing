using backend.Models;

namespace backend.Services.Abstraction
{
    public interface IOSWorkloadService
    {
        OsWorkloadInfo GetWorkload();
    }

}
