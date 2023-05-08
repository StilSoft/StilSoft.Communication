using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StilSoft.Exceptions;

namespace StilSoft.Communication
{
    public class Command : ICommand
    {
        private readonly ICommunicationChannel communicationChannel;

        public CommandType CommandType { get; set; }
        public TimeSpan DelayAfterSend { get; set; }
        public TimeSpan? ReceiveTimeout { get; set; }
        public IRequest Request { get; set; }
        public IResponse Response { get; private set; }
        public IReadOnlyCollection<IValidator> RequestValidators { get; set; }
        public IReadOnlyCollection<IResponseHandler> ResponseHandlers { get; set; }
        public IReadOnlyCollection<IResponseValidator> ResponseValidators { get; set; }

        public Command(ICommunicationChannel communicationChannel)
        {
            this.communicationChannel = communicationChannel;
        }

        public virtual async Task ExecuteAsync(int retryCount = 0, Action<int> onRetry = null,
            CancellationToken cancellationToken = default)
        {
            int totalRetries = 0;

            if (this.communicationChannel == null)
            {
                throw new InvalidOperationException("Communication channel cannot be null");
            }

            if (this.CommandType == CommandType.Send || this.CommandType == CommandType.SendReceive)
            {
                ValidateRequest();
                ValidateAndSetAdditionalRequestData();
            }

            IResponse response = null;
            bool sendSuccessful = false;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    switch (this.CommandType)
                    {
                        case CommandType.SendReceive:
                            response = await this.communicationChannel.SendReceiveAsync(this.Request, this.ReceiveTimeout);
                            break;
                        case CommandType.Send:
                            await this.communicationChannel.SendAsync(this.Request);
                            break;
                        case CommandType.Receive:
                            response = await this.communicationChannel.ReceiveAsync(this.ReceiveTimeout);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    sendSuccessful = true;

                    await Task.Delay(this.DelayAfterSend, cancellationToken);
                }
                catch
                {
                    if (retryCount <= 0)
                    {
                        throw;
                    }

                    retryCount--;
                    totalRetries++;

                    onRetry?.Invoke(totalRetries);
                }
            } while (!sendSuccessful);

            if (response != null)
            {
                response = await ProcessResponseAsync(response, this.ReceiveTimeout);

                ValidateResponse(response);

                this.Response = response;
            }
        }

        private void ValidateRequest()
        {
            if (this.Request?.Data == null || this.Request.Data.Length == 0)
            {
                throw new InvalidOperationException("Request cannot be null or empty");
            }

            if (this.RequestValidators == null || this.RequestValidators.Count == 0)
            {
                return;
            }

            bool atLeastOneValid = false;

            foreach (IValidator validator in this.RequestValidators)
            {
                bool isValid = validator.Validate(this.Request.Data);

                if (isValid)
                {
                    atLeastOneValid = true;
                }
                else
                {
                    if (validator.IsOptional)
                    {
                        continue;
                    }

                    string errorMessage = string.IsNullOrWhiteSpace(validator.ErrorDescription)
                        ? "Invalid request data"
                        : validator.ErrorDescription;

                    throw new ValidationException(errorMessage);
                }
            }

            if (!atLeastOneValid)
            {
                throw new ValidationException("Data you trying to send is invalid");
            }
        }

        private void ValidateAndSetAdditionalRequestData()
        {
            if (!(this.Request is IRequestWithAdditionalData request) || request.AdditionalData == null ||
                request.AdditionalData.Count == 0)
            {
                return;
            }

            foreach (RequestAdditionalData additionalData in request.AdditionalData)
            {
                if (additionalData?.Data == null || additionalData.Data.Length == 0)
                {
                    continue;
                }

                IValidator validator = additionalData.Validator;

                if (validator != null)
                {
                    bool isValid = validator.Validate(additionalData.Data);

                    if (!isValid)
                    {
                        string errorMessage = string.IsNullOrWhiteSpace(validator.ErrorDescription)
                            ? "Invalid request additional data"
                            : validator.ErrorDescription;

                        throw new ValidationException(errorMessage);
                    }
                }

                byte[] requestData = request.Data;
                int requestDataSize = requestData.Length;
                int additionalDataSize = additionalData.Data.Length;
                int additionalDataSizeWithOffset = additionalData.StartIndex + additionalDataSize;

                if (requestDataSize < additionalDataSizeWithOffset)
                {
                    Array.Resize(ref requestData, additionalDataSizeWithOffset);
                }

                for (int i = 0; i < additionalDataSize; i++)
                {
                    requestData[additionalData.StartIndex + i] = additionalData.Data[i];
                }

                request.Data = requestData;
            }
        }

        private async Task<IResponse> ProcessResponseAsync(IResponse response, TimeSpan? receiveTimeout = default)
        {
            ResponseHandlerResult result;

            do
            {
                result = await HandleResponseAsync(response, receiveTimeout);

                if (result.State == ResponseHandlerState.ResponseChanged)
                {
                    response = result.Response;
                }
            } while (result.State == ResponseHandlerState.ResponseChanged);

            return result.Response;
        }

        private async Task<ResponseHandlerResult> HandleResponseAsync(IResponse response, TimeSpan? receiveTimeout = default)
        {
            if (this.ResponseHandlers == null || this.ResponseHandlers.Count == 0)
            {
                return new ResponseHandlerResult(ResponseHandlerState.Complete, response);
            }

            foreach (IResponseHandler responseHandler in this.ResponseHandlers)
            {
                ResponseHandlerResult responseHandlerResult =
                    await responseHandler.HandleAsync(response, this.Request, this.communicationChannel, receiveTimeout);

                if (responseHandlerResult.State == ResponseHandlerState.ResponseChanged ||
                    responseHandlerResult.State == ResponseHandlerState.Complete)
                {
                    return responseHandlerResult;
                }
            }

            return new ResponseHandlerResult(ResponseHandlerState.Complete, response);
        }

        private void ValidateResponse(IResponse response)
        {
            if (this.ResponseValidators == null || this.ResponseValidators.Count == 0)
            {
                return;
            }

            bool atLeastOneValid = false;

            foreach (IResponseValidator validator in this.ResponseValidators)
            {
                bool isValid = validator.Validate(response, this.Request);

                if (isValid)
                {
                    atLeastOneValid = true;
                }
                else
                {
                    if (validator.IsOptional)
                    {
                        continue;
                    }

                    string errorMessage = validator.ErrorDescription.Length > 0
                        ? validator.ErrorDescription
                        : "Received invalid data";

                    throw new ValidationException(errorMessage);
                }
            }

            if (!atLeastOneValid)
            {
                throw new ValidationException("Received invalid data");
            }
        }
    }

    public class Command<TResult> : Command, ICommand<TResult>
    {
        public IResponseParser ResponseParser { get; set; }
        public IResponseConverter<TResult> ResponseConverter { get; set; }
        public TResult Result { get; private set; }

        public Command(ICommunicationChannel communicationChannel)
            : base(communicationChannel)
        {
        }

        public override async Task ExecuteAsync(int retryCount = 0, Action<int> onRetry = null,
            CancellationToken cancellationToken = default)
        {
            await base.ExecuteAsync(retryCount, onRetry, cancellationToken);

            byte[] parsedResponse = ParseResponse(this.Response, this.ResponseParser);

            TResult convertedResponse = ConvertParsedResponse(parsedResponse, this.ResponseConverter);

            this.Result = convertedResponse;
        }

        private byte[] ParseResponse(IResponse response, IResponseParser responseParser)
        {
            if (responseParser == null)
            {
                return response.Data;
            }

            byte[] parsedData = responseParser.Parse(response);

            return parsedData;
        }

        private TResult ConvertParsedResponse(byte[] parsedData, IResponseConverter<TResult> responseConverter)
        {
            if (responseConverter == null)
            {
                return (TResult)Convert.ChangeType(parsedData, typeof(TResult));
            }

            TResult convertedData = responseConverter.Convert(parsedData);

            return convertedData;
        }
    }
}