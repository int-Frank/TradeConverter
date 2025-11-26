using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TradeConverter.Importers
{
  public class WarriorTradingSimImporter : IImporter
  {
    private static class HeaderStrings
    {
      public const string Symbol = "Symbol";
      public const string Date = "Date";
      public const string Time = "Time";
      public const string Side = "Side";
      public const string Quantity = "Quantity";
      public const string Price = "Price";
    }

    public bool TryRead(string filePath, out Transaction[] entries)
    {
      if (!IsValidFile(filePath))
      {
        entries = Array.Empty<Transaction>();
        return false;
      }

      Console.WriteLine($"Importing Warrior Trading Sim file '{Path.GetFileName(filePath)}'...");

      var entriesList = new List<Transaction>();

      using (var reader = new StreamReader(filePath))
      {
        Dictionary<string, int>? header = null;

        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();

          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          if (header is null)
          {
            header = ReadHeader(line);
            continue;
          }

          if (!TryParseLine(line, header, out var entry) || entry is null)
          {
            Console.WriteLine($"Failed!");
            entries = [];
            return false;
          }

          entriesList.Add(entry);
        }
      }

      Console.WriteLine("Done!");
      entries = entriesList.ToArray();

      return true;
    }

    private bool IsValidFile(string filePath)
    {
      using (var reader = new StreamReader(filePath))
      {
        Dictionary<string, int>? header = null;

        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();

          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          if (header is null)
          {
            header = ReadHeader(line);
            continue;
          }

          return TryParseLine(line, header, out var entry) && entry is not null;
        }
      }

      return false;
    }

    private Dictionary<string, int> ReadHeader(string line)
    {
      var header = new Dictionary<string, int>();

      string[] parts = line.Split(',');

      for (int i = 0; i < parts.Length; i++)
      {
        switch (parts[i])
        {
          case HeaderStrings.Time:
            header[HeaderStrings.Time] = i; 
            break;
          case HeaderStrings.Date:
            header[HeaderStrings.Date] = i;
            break;
          case HeaderStrings.Price:
            header[HeaderStrings.Price] = i;
            break;
          case HeaderStrings.Symbol:
            header[HeaderStrings.Symbol] = i;
            break;
          case HeaderStrings.Side:
            header[HeaderStrings.Side] = i;
            break;
          case HeaderStrings.Quantity:
            header[HeaderStrings.Quantity] = i;
            break;
          default:
            break;
        }
      }

      return header;
    }

    private bool TryParseLine(string line, Dictionary<string, int> header, out Transaction? entry)
    {
      string[] parts = line.Split(',');

      entry = new Transaction();

      // Add assumptions
      entry.Currency = "USD";

      return TryAddDateTime(parts, header, ref entry) &&
             TryAddSymbol(parts, header, ref entry) &&
             TryAddSide(parts, header, ref entry) &&
             TryAddQuantity(parts, header, ref entry) &&
             TryAddPrice(parts, header, ref entry);
    }

    private bool TryAddDateTime(string[] parts, Dictionary<string, int> header, ref Transaction entry)
    {
      if (!header.TryGetValue(HeaderStrings.Date, out var dateIndex) ||
          !header.TryGetValue(HeaderStrings.Time, out var timeIndex) ||
          dateIndex >= parts.Length ||
          timeIndex >= parts.Length)
      {
        return false;
      }

      var date = parts[dateIndex];
      var time = parts[timeIndex];

      // Assume EST
      string combined = $"{date} {time}";

      var format = "MM/dd/yy HH:mm:ss";
      entry.DateTimeEST = DateTime.ParseExact(combined,
                                              format,
                                              CultureInfo.InvariantCulture,
                                              DateTimeStyles.None);

      return true;
    }



    private static bool TryAddSymbol(string[] parts, Dictionary<string, int> header, ref Transaction entry)
    {
      if (!header.TryGetValue(HeaderStrings.Symbol, out var index) ||
          index >= parts.Length)
      {
        return false;
      }

      entry.Symbol = parts[index];
      return Regex.IsMatch(entry.Symbol, @"^[A-Z]{1,8}$");
    }

    private static bool TryAddSide(string[] parts, Dictionary<string, int> header, ref Transaction entry)
    {
      if (!header.TryGetValue(HeaderStrings.Side, out var index) ||
          index >= parts.Length)
      {
        return false;
      }

      var side = parts[index];

      if (side == "B") entry.Side = Side.Buy;
      else if (side == "S") entry.Side = Side.Sell;
      else if (side == "SS") entry.Side = Side.Sell;
      else return false;
      return true;
    }

    private static bool TryAddQuantity(string[] parts, Dictionary<string, int> header, ref Transaction entry)
    {
      if (!header.TryGetValue(HeaderStrings.Quantity, out var index) ||
          index >= parts.Length)
      {
        return false;
      }

      var success = int.TryParse(parts[index], out var quantity);

      if (success)
      {
        entry.Quantity = quantity;
      }

      return success;
    }

    private static bool TryAddPrice(string[] parts, Dictionary<string, int> header, ref Transaction entry)
    {
      if (!header.TryGetValue(HeaderStrings.Price, out var index) ||
          index >= parts.Length)
      {
        return false;
      }

      var success = double.TryParse(parts[index], out var price);

      if (success)
      {
        entry.Price = price;
      }

      return success;
    }
  }
}