using System.Threading.Tasks;

namespace StilSoft.Communication
{
    public interface ICommunicationInterface
    {
        Task OpenAsync();
        Task CloseAsync();
        Task<bool> IsOpenAsync();
        Task<ICommunicationChannel> CreateChannelAsync(IChannelConfiguration configuration);
    }
}