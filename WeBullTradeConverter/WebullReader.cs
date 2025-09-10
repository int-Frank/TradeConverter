using System.Globalization;

namespace WebullConverter
{
  public class WebullReader
  {
    public class DTO
    {
      public string Symbol { get; set; } = string.Empty;
      public string Name { get; set; } = string.Empty;
      public string Currency { get; set; } = string.Empty;
      public string Type { get; set; } = string.Empty;
      public DateTime DateTime { get; set; }
      public Side Side { get; set; }
      public double Quantity { get; set; }
      public double Price { get; set; }
      public double GrossAmount { get; set; }
      public double NetAmount { get; set; }
      public double Fee { get; set; }
      public double GST { get; set; }
      public string Exchange { get; set; } = string.Empty;
    }

    public DTO[] Entries { get; set; } = Array.Empty<DTO>();

    public void Read(string filePath)
    {
      using (var reader = new StreamReader(filePath))
      {
        bool isFirstLine = true;
        var dtos = new List<DTO>();
        var header = new Dictionary<string, int>();
        int errors = 0;

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

          // remove comma in the time column
          line = line.Replace(",GMT", " GMT");

          var parts = line.Split(',');
          var dto = new DTO();
          int index;

          if (!header.TryGetValue("Symbol", out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Symbol from CSV");
            errors++;
            continue;
          }
          dto.Symbol = Trim(parts[index]);

          if (!header.TryGetValue("Name", out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Name from CSV");
            errors++;
            continue;
          }
          dto.Name = Trim(parts[index]);

          if (!header.TryGetValue("Currency", out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Currency from CSV");
            errors++;
            continue;
          }
          dto.Currency = Trim(parts[index]);

          if (!header.TryGetValue("Type", out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Type from CSV");
            errors++;
            continue;
          }
          dto.Type = Trim(parts[index]);

          if (!header.TryGetValue("Trade Date", out index) ||
              index >= parts.Length ||
              !header.TryGetValue("Time", out var timeIndex) ||
              timeIndex >= parts.Length ||
              ! TryGetDateTime(Trim(parts[index]), Trim(parts[timeIndex]), out var dateTime))
          {
            Console.WriteLine($"Failed to read Date and Time from CSV");
            errors++;
            continue;
          }
          dto.DateTime = dateTime;

          if (!header.TryGetValue("Type", out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Type from CSV");
            errors++;
            continue;
          }
          dto.Type = Trim(parts[index]);

          if (!header.TryGetValue("Buy/Sell", out index) ||
              index >= parts.Length ||
              !TryGetSide(Trim(parts[index]), out var side))
          {
            Console.WriteLine($"Failed to read Buy/Sell from CSV");
            errors++;
            continue;
          }
          dto.Side = side;

          if (!header.TryGetValue("Quantity", out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var quantity))
          {
            Console.WriteLine($"Failed to read Quantity from CSV");
            errors++;
            continue;
          }
          dto.Quantity = quantity;

          if (!header.TryGetValue("Trade Price", out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var price))
          {
            Console.WriteLine($"Failed to read Trade Price from CSV");
            errors++;
            continue;
          }
          dto.Price = price;

          if (!header.TryGetValue("Gross Amount", out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var grossAmount))
          {
            Console.WriteLine($"Failed to read Gross Amount from CSV");
            errors++;
            continue;
          }
          dto.GrossAmount = grossAmount;

          if (!header.TryGetValue("Net Amount", out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var netAmount))
          {
            Console.WriteLine($"Failed to read Net Amount from CSV");
            errors++;
            continue;
          }
          dto.NetAmount = netAmount;

          if (!header.TryGetValue("Comm/Fee/Tax", out index) ||
              index >= parts.Length ||
              !double.TryParse(Trim(parts[index]), out var fee))
          {
            Console.WriteLine($"Failed to read Comm/Fee/Tax from CSV");
            errors++;
            continue;
          }
          dto.Fee = fee;

          if (header.TryGetValue("GST", out index) &&
              index < parts.Length &&
              double.TryParse(Trim(parts[index]), out var gst))
          {
            dto.GST = gst;
          }

          if (!header.TryGetValue("Exchange", out index) ||
              index >= parts.Length)
          {
            Console.WriteLine($"Failed to read Exchange from CSV");
            errors++;
            continue;
          }
          dto.Exchange = Trim(parts[index]);

          dtos.Add(dto);
        }

        Entries = dtos.ToArray();
      }
    }

    private bool TryGetSide(string str, out Side side)
    {
      side = str.ToLowerInvariant() == "buy" ? Side.Buy : Side.Sell;
      return true;
    }

    private bool TryGetDateTime(string date, string time, out DateTime dateTime)
    {
      // Add :00 as a quick fix for now
      string combined = $"{date} {time}:00";

      // Define format: yyyy/MM/dd HH:mm:ss 'GMT'zzz
      var format = "yyyy/MM/dd HH:mm:ss 'GMT'zzz";
      var provider = CultureInfo.InvariantCulture;

      if (!DateTimeOffset.TryParseExact(combined, format, provider, DateTimeStyles.None, out DateTimeOffset dto))
      {
        dateTime = default;
        return false;
      }

      dateTime = dto.UtcDateTime;
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
  }
}