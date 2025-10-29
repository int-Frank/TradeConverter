
using System.IO;
using WeBullTradeConverter.Args;

namespace WebullConverter
{
  public class Program
  {

    static void Main(string[] args)
    {
      var program = new Program();

      var argService = new ArgService();
      var context = argService.Execute(args);

      program.Run(context);
    }

    public void Run(ConverterContext context)
    {
      if (!Directory.Exists(context.InputPath))
      {
        Console.WriteLine("Invalid input path");
      }

      if (!Directory.Exists(context.OutputPath))
      {
        Console.WriteLine("Invalid output path");
      }

      string[] csvFiles = Directory.GetFiles(context.InputPath, "*.csv");

      var transactions  = csvFiles
        .SelectMany(file =>
        {
          var reader = new WebullReader();
          reader.Read(file);
          return reader.Entries;
        })
        .DistinctBy(entry => 
        (
          entry.Symbol,
          entry.Name,
          entry.Currency,
          entry.Type,
          entry.DateTimeEST,
          entry.Side,
          entry.Quantity,
          entry.Price,
          entry.GrossAmount,
          entry.NetAmount,
          entry.Fee,
          entry.GST,
          entry.Exchange
        ))
        .ToArray();

      // Group by session
      TimeZoneInfo nyZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
      var grouped = transactions
            .GroupBy(dto => dto.DateTimeEST.Date);

      foreach (var group in grouped)
      {
        var entries = group.Select(dto => dto).ToArray();

        var date = group.Key.ToString("yyyy-MM-dd");
        var fileName = $"{date}.csv";
        var fullPath = Path.Combine(context.OutputPath, fileName);

        var writer = new CSVWriter();
        writer.WriteToFile(fullPath, entries);
      }
    }
  }
}