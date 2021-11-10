namespace NordChecker.Models
{
    public interface IPayloadFormatter<TPayload, TOutput>
    {
        public TOutput Format(TPayload payload);
    }
}
