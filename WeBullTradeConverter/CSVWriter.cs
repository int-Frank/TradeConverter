using System.Globalization;

namespace WebullConverter
{
  public class CSVWriter
  {
    public void WriteToFile(string filePath, WebullReader.DTO[] entries)
    {
      using (StreamWriter writer = new StreamWriter(filePath))
      {
        writer.WriteLine($"Date,Time,Symbol,Quantity,Price,Side,Commision,TransFee,ECNFee");

        foreach (var entry in entries)
        {
          string nyDate = entry.DateTimeEST.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
          string nyClock = entry.DateTimeEST.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

          writer.Write($"{nyDate},");
          writer.Write($"{nyClock},");
          writer.Write($"{entry.Symbol},");
          writer.Write($"{entry.Quantity},");
          writer.Write($"{entry.Price},");
          writer.Write($"{entry.Side},");
          writer.Write("0,"); // Commision
          writer.Write($"{entry.Fee},");
          writer.WriteLine("0"); // ECNFee
        }
      }
    }
  }
}