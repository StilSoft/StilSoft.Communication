namespace StilSoft.Communication
{
    public class RequestAdditionalData
    {
        public byte[] Data { get; }
        public int StartIndex { get; }
        public IValidator Validator { get; }

        public RequestAdditionalData(byte[] data, int startIndex, IValidator validator = null)
        {
            this.Data = data;
            this.StartIndex = startIndex;
            this.Validator = validator;
        }
    }
}