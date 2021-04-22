namespace StilSoft.Communication
{
    public interface IValidator
    {
        bool IsOptional { get; }
        string ErrorDescription { get; }

        bool Validate(byte[] data);
    }
}