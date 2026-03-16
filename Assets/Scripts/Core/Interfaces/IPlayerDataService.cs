using SledSurfers.Data.Models;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for persisting player progression data.
    /// OCP - new storage backends can be added without modifying consumers.
    /// </summary>
    public interface IPlayerDataService
    {
        PlayerData Load();
        void Save(PlayerData data);
        void Reset();
    }
}
