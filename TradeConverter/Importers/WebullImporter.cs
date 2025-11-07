using System.Globalization;

namespace TradeConverter.Importers
{
  public class WebullImporter : IImporter
  {
    private static class HeaderStrings
    {
      public static string Symbol = "Symbol";
      public static string Name = "Name";
      public static string Currency = "Currency";
      public static string Type = "Type";
      public static string TradeDate = "Trade Date";
      public static string Time = "Time";
      public static string BuySell = "Buy/Sell";
      public static string Quantity = "Quantity";
      public static string TradePrice = "Trade Price";
      public static string GrossAmount = "Gross Amount";
      public static string NetAmount = "Net Amount";
      public static string CommFeeTax = "Comm/Fee/Tax";
      public static string GST = "GST";
      public static string Exchange = "Exchange";

      public static string[] Columns => [Symbol, Name, Currency, Type, TradeDate, Time, BuySell, Quantity, TradePrice, GrossAmount, NetAmount, CommFeeTax, GST, Exchange];
    }

    public bool TryRead(string filePath, out TradeEntry[] entries)
    {
      if (!IsValidFile(filePath))
      {
        entries = Array.Empty<TradeEntry>();
        return false;
      }

      Console.WriteLine($"Importing WeBull file '{Path.GetFileName(filePath)}'...");

      using (var reader = new StreamReader(filePath))
      {
        bool isFirstLine = true;
        var dtos = new List<TradeEntry>();
        var header = new Dictionary<string, int>();

        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();

          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          if (isFirstLine)
          {
            header = ReadColumns(line);
            isFirstLine = false;
            continue;
          }

          var parts = ParseLine(line);
          var dto = new TradeEntry();
          int index;

          if (!header.TryGetValue(HeaderStrings.Symbol, out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Symbol from CSV");
            entries = [];
            return false;
          }
          dto.Symbol = Trim(parts[index]);

          if (!header.TryGetValue(HeaderStrings.Name, out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Name from CSV");
            entries = [];
            return false;
          }
          dto.Name = Trim(parts[index]);

          if (!header.TryGetValue(HeaderStrings.Currency, out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Currency from CSV");
            entries = [];
            return false;
          }
          dto.Currency = Trim(parts[index]);

          if (!header.TryGetValue(HeaderStrings.Type, out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Type from CSV");
            entries = [];
            return false;
          }
          dto.Type = Trim(parts[index]);

          if (!header.TryGetValue(HeaderStrings.TradeDate, out index) ||
              index >= parts.Length ||
              !header.TryGetValue(HeaderStrings.Time, out var timeIndex) ||
              timeIndex >= parts.Length ||
              !TryGetDateTime(Trim(parts[index]), Trim(parts[timeIndex]), out var dateTime))
          {
            Console.WriteLine($"Failed to read Date and Time from CSV");
            entries = [];
            return false;
          }
          dto.DateTimeEST = dateTime;

          if (!header.TryGetValue(HeaderStrings.Type, out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Type from CSV");
            entries = [];
            return false;
          }
          dto.Type = Trim(parts[index]);

          if (!header.TryGetValue(HeaderStrings.BuySell, out index) ||
              index >= parts.Length ||
              !TryGetSide(Trim(parts[index]), out var side))
          {
            Console.WriteLine($"Failed to read Buy/Sell from CSV");
            entries = [];
            return false;
          }
          dto.Side = side;

          if (!header.TryGetValue(HeaderStrings.Quantity, out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var quantity))
          {
            Console.WriteLine($"Failed to read Quantity from CSV");
            entries = [];
            return false;
          }
          dto.Quantity = quantity;

          if (!header.TryGetValue(HeaderStrings.TradePrice, out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var price))
          {
            Console.WriteLine($"Failed to read Trade Price from CSV");
            entries = [];
            return false;
          }
          dto.Price = price;

          //if (!header.TryGetValue(HeaderStrings.GrossAmount, out index) ||
          //    index >= parts.Length ||
          //    !double.TryParse(Trim(parts[index]), out var grossAmount))
          //{
          //  Console.WriteLine($"Failed to read Gross Amount from CSV");
          //  errors++;
          //  continue;
          //}
          //dto.GrossAmount = grossAmount;

          //if (!header.TryGetValue(HeaderStrings.NetAmount, out index) ||
          //    index >= parts.Length ||
          //    !double.TryParse(Trim(parts[index]), out var netAmount))
          //{
          //  Console.WriteLine($"Failed to read Net Amount from CSV");
          //  errors++;
          //  continue;
          //}
          //dto.NetAmount = netAmount;

          if (!header.TryGetValue(HeaderStrings.CommFeeTax, out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var fee))
          {
            Console.WriteLine($"Failed to read Comm/Fee/Tax from CSV");
            entries = [];
            return false;
          }
          dto.Fee = fee;

          //if (header.TryGetValue(HeaderStrings.GST, out index) &&
          //    index < parts.Length &&
          //    double.TryParse(Trim(parts[index]), out var gst))
          //{
          //  dto.GST = gst;
          //}

          if (!header.TryGetValue(HeaderStrings.Exchange, out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Exchange from CSV");
            entries = [];
            return false;
          }
          dto.Exchange = Trim(parts[index]);

          dtos.Add(dto);
        }

        entries = dtos.ToArray();
        Console.WriteLine("Done!");
        return true;
      }
    }

    private static bool IsValidFile(string filePath)
    {
      // Quick hack for now, just confirm the first line.
      string firstLine = File.ReadLines(filePath).First();
      var header = $"{HeaderStrings.Symbol},{HeaderStrings.Name},{HeaderStrings.Currency},{HeaderStrings.Type},{HeaderStrings.TradeDate},{HeaderStrings.Time},{HeaderStrings.BuySell},{HeaderStrings.Quantity},{HeaderStrings.TradePrice},{HeaderStrings.GrossAmount},{HeaderStrings.NetAmount},{HeaderStrings.CommFeeTax},{HeaderStrings.GST},{HeaderStrings.Exchange}";
      return firstLine == header;
    }

    private bool TryGetSide(string str, out Side side)
    {
      side = str.ToLowerInvariant() == "buy" ? Side.Buy : Side.Sell;
      return true;
    }

    private bool TryGetDateTime(string date, string time, out DateTime dateTime)
    {
      // Assume EST, so only need the time itself
      var timeParts = time.Split(',');
      if (timeParts.Length < 1)
      {
        dateTime = default;
        return false;
      }

      string combined = $"{date} {timeParts[0]}";

      var format = "yyyy/MM/dd HH:mm:ss";

      dateTime = DateTime.ParseExact(combined,
                                     format,
                                     CultureInfo.InvariantCulture,
                                     DateTimeStyles.None);

      return true;
    }

    private static string Trim(string str)
    {
      bool trimmed;

      do
      {
        trimmed = false;

        var tempStr = str.Trim('"');
        tempStr = tempStr.Trim('\'');

        if (tempStr != str)
        {
          trimmed = true;
          str = tempStr;
        }

      } while (trimmed);

      return str;
    }

    private Dictionary<string, int> ReadColumns(string line)
    {
      var parts = line.Split(',');
      var header = new Dictionary<string, int>();

      for (int i = 0; i < parts.Length; i++)
      {
        var key = Trim(parts[i]);
        header[key] = i;
      }

      return header;
    }

    private string[] ParseLine(string line)
    {
      var parts = new List<string>();
      var field = "";
      var readingString = false;

      foreach (var c in line)
      {
        if (c == ',' && !readingString)
        {
          parts.Add(field);
          field = "";
          continue;
        }

        if (c == '"')
        {
          readingString = !readingString;
          continue;
        }

        field += c;
      }

      parts.Add(field);

      return parts.ToArray();
    }
  }
}