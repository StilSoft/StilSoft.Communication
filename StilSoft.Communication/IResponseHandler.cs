using System;
using System.Threading.Tasks;

namespace StilSoft.Communication
{
    public interface IResponseHandler
    {
        Task<ResponseHandlerResult> HandleAsync(IResponse response, IRequest request, ICommunicationChannel communicationChannel,
            TimeSpan? receiveTimeout = default);
    }
}