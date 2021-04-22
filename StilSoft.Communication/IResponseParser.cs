namespace StilSoft.Communication
{
    public interface IResponseParser
    {
        byte[] Parse(IResponse data);
    }
}