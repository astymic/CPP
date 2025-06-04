namespace IRT.Interfaces;
public interface IPrice
{
    decimal Price { get; set; }

    event EventHandler<decimal>? PriceChanged;
}
