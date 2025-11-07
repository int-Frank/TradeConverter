namespace TradeConverter
{
  public class TradeEntry
  {
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime DateTimeEST { get; set; }
    public Side Side { get; set; }
    public double Quantity { get; set; }
    public double Price { get; set; }
    public double Commision { get; set; }
    public double Fee { get; set; }
    public string Exchange { get; set; } = string.Empty;
  }
}