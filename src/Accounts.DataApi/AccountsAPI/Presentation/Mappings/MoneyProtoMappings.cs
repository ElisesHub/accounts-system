using Google.Type;

namespace AccountsAPI.Presentation.Mappings;

public static class MoneyProtoMappings
{
    public static Money ToMoney(decimal amount, string currencyCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);

        var units = (long)amount;
        var nanos = (int)((amount - units) * 1_000_000_000m);

        return new Money
        {
            CurrencyCode = currencyCode,
            Units = units,
            Nanos = nanos
        };
    }
}