namespace StilSoft.Communication
{
    public class ResponseHandlerResult
    {
        public ResponseHandlerState State { get; }
        public IResponse Response { get; }

        public ResponseHandlerResult(ResponseHandlerState state, IResponse response)
        {
            this.State = state;
            this.Response = response;
        }
    }
}