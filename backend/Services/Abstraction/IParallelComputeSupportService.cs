using backend.Models;

namespace backend.Services.Abstraction
{
    public interface IParallelComputeSupportService
    {
        /// <summary>
        /// Перевіряє підтримку всіх технологій.
        /// </summary>
        Task<IReadOnlyList<ComputeSupportInfo>> GetAllSupportInfoAsync();

        /// <summary>
        /// Перевіряє підтримку конкретної технології.
        /// </summary>
        Task<ComputeSupportInfo> CheckSupportAsync(ComputeTechnology tech);
    }
}
