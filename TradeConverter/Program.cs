using TradeConverter.Args;
using TradeConverter.Exporters;
using TradeConverter.Importers;

namespace TradeConverter
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

    private void Run(ConverterContext context)
    {
      if (!Directory.Exists(context.InputPath))
      {
        Console.WriteLine("Invalid input path");
      }

      if (!Directory.Exists(context.OutputPath))
      {
        Console.WriteLine("Invalid output path");
      }

      string[] extensions = { ".csv", ".txt" };

      var files = Directory
        .EnumerateFiles(context.InputPath)
        .Where(f => extensions.Any(ext =>
            f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        .ToArray();

      var transactions  = files
        .SelectMany(file => ReadEntries(file))
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
          entry.Fee,
          entry.Exchange
        ))
        .ToArray();

      // Group by session
      TimeZoneInfo nyZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
      var grouped = transactions
            .GroupBy(dto => dto.DateTimeEST.Date);

      foreach (var exporter in context.Exporters)
      {
        foreach (var group in grouped)
        {
          var entries = group.Select(dto => dto).ToArray();

          var date = group.Key.ToString("yyyy-MM-dd");
          var fileName = $"{exporter.Name}-{date}.csv";
          var fullPath = Path.Combine(context.OutputPath, fileName);

          if (!exporter.TryWriteToFile(fullPath, entries))
          {
            Console.WriteLine($"Failed to write file '{fileName}'");
          }
        }
      }
    }

    private Transaction[] ReadEntries(string file)
    {
      var entries = Array.Empty<Transaction>();

      if (new WebullImporter().TryRead(file, out entries))
      {
        return entries;
      }

      if (new WarriorTradingSimImporter().TryRead(file, out entries))
      {
        return entries;
      }

      Console.WriteLine($"Failed to identify file '{file}'");
      return Array.Empty<Transaction>();
    }
  }
}