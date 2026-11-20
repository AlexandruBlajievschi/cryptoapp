using System;

namespace backend.ExceptionHandling;

public class MarketDataProviderTimeoutException : MarketDataProviderException
{
    public MarketDataProviderTimeoutException(string message, Exception innerException) : base(message, innerException)
    {

    }


}
