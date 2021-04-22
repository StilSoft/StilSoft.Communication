using System.Collections.Generic;

namespace StilSoft.Communication
{
    public interface IRequestWithAdditionalData : IRequest
    {
        IReadOnlyCollection<RequestAdditionalData> AdditionalData { get; }
    }
}