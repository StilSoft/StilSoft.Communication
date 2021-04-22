namespace StilSoft.Communication
{
    public interface IResponseValidator
    {
        bool IsOptional { get; }
        string ErrorDescription { get; }

        bool Validate(IResponse response, IRequest request);
    }
}