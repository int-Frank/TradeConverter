using System.Globalization;

namespace TradeConverter.Exporters
{
  public class TradervueExporter : IExporter
  {
    public string Name => "Tradervue";

    public bool TryWriteToFile(string filePath, TradeEntry[] entries)
    {
      using (StreamWriter writer = new StreamWriter(filePath))
      {
        writer.WriteLine($"Date,Time,Symbol,Quantity,Price,Side,Commision,TransFee,ECNFee");

        foreach (var entry in entries)
        {
          string estDate = entry.DateTimeEST.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
          string estClock = entry.DateTimeEST.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

          writer.Write($"{estDate},");
          writer.Write($"{estClock},");
          writer.Write($"{entry.Symbol},");
          writer.Write($"{entry.Quantity},");
          writer.Write($"{entry.Price},");
          writer.Write($"{entry.Side},");
          writer.Write($"{entry.Commision},");
          writer.Write($"{entry.Fee},");
          writer.WriteLine("0"); // ECNFee
        }
      }

      return true;
    }
  }
}