using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StilSoft.Communication
{
    public interface ICommand
    {
        bool SendOnly { get; }
        TimeSpan DelayAfterSend { get; }
        IRequest Request { get; }
        IReadOnlyCollection<IValidator> RequestValidators { get; }
        IReadOnlyCollection<IResponseHandler> ResponseHandlers { get; }
        IReadOnlyCollection<IResponseValidator> ResponseValidators { get; }
        IResponse Response { get; }

        Task ExecuteAsync(int retryCount = 0, TimeSpan timeout = default, Action<int> onRetry = null,
            CancellationToken cancellationToken = default);
    }

    public interface ICommand<out TResult> : ICommand
    {
        IResponseParser ResponseParser { get; }
        IResponseConverter<TResult> ResponseConverter { get; }
        TResult Result { get; }
    }
}