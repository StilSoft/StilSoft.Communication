using System;
using System.Threading.Tasks;

namespace StilSoft.Communication
{
    public interface ICommunicationChannel
    {
        Task OpenAsync();
        Task CloseAsync();
        Task<bool> IsOpenAsync();
        Task SendAsync(IRequest request);
        Task<IResponse> ReceiveAsync();
        Task<IResponse> SendReceiveAsync(IRequest request);
        Task SetConfigurationAsync(IChannelConfiguration configuration);
        Task<int> StartPeriodicMessageAsync(IRequest request, TimeSpan interval);
        Task StopPeriodicMessageAsync(int messageId);
    }
}