using System;

namespace backend.Features.Markets.Exceptions;

public class MarketDataProviderTimeoutException : MarketDataProviderException
{
    public MarketDataProviderTimeoutException(string message, Exception innerException) : base(message, innerException)
    {

    }


}
