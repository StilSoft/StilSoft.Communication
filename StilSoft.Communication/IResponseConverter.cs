namespace StilSoft.Communication
{
    public interface IResponseConverter<out TResponse>
    {
        TResponse Convert(byte[] data);
    }
}