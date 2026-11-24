namespace backend.ExceptionHandling;

public class MarketDataProviderException : Exception
{
    public MarketDataProviderException(string message) : base(message)
    {
    }

    public MarketDataProviderException(string message, Exception innerException) : base(message, innerException)
    {

    }

}
