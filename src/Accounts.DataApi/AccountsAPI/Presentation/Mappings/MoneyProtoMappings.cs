using Google.Type;

namespace AccountsAPI.Presentation.Mappings;

/// <summary>
/// Maps amounts to the <c>google.type.Money</c> proto message.
/// </summary>
public static class MoneyProtoMappings
{
    /// <summary>
    /// Converts a decimal amount and currency code into <see cref="Money"/>.
    /// The amount is split into whole <see cref="Money.Units"/> and fractional <see cref="Money.Nanos"/>
    /// (billionths), truncating toward zero so both always have the same sign, as the type requires.
    /// For example, <c>-12.50</c> becomes <c>Units = -12, Nanos = -500_000_000</c>.
    /// </summary>
    /// <param name="amount">The amount to convert.</param>
    /// <param name="currencyCode">The ISO 4217 currency code, e.g. <c>"EUR"</c>.</param>
    /// <returns>The amount as <see cref="Money"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="currencyCode"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="currencyCode"/> is null.</exception>
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