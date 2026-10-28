namespace PortfolioApplicationAPI.Infrastructure.Mappings;

public static class MoneyMappings
{
    public static decimal ToDecimal(Google.Type.Money value)
    {
       ArgumentNullException.ThrowIfNull(value);
       return  value.Units + (value.Nanos / 1_000_000_000m);

    }
}