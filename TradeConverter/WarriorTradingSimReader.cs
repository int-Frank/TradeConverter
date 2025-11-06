using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TradeConverter
{
  public class WarriorTradingSimReader : IReader
  {
    public bool TryRead(string filePath, out TradeEntry[] entries)
    {
      var entriesList = new List<TradeEntry>();

      using (var reader = new StreamReader(filePath))
      {
        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();

          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          if (!TryParseLine(line, out var entry) || entry is null)
          {
            entries = [];
            return false;
          }

          entriesList.Add(entry);
        }
      }

      entries = entriesList.ToArray();

      return true;
    }

    private bool TryParseLine(string line, out TradeEntry? entry)
    {
      string[] parts = line.Split(',');

      if (parts.Length != 7)
      {
        entry = null;
        return false;
      }

      // Because there is no header for these files, we are going to be very strict with parsing

      entry = new TradeEntry();

      // Add assumptions
      entry.Currency = "USD";

      return TryAddDateTime(parts[0], parts[1], ref entry) &&
             TryAddSymbol(parts[2], ref entry) &&
             TryAddSide(parts[3], ref entry) &&
             TryAddQuantity(parts[4], ref entry) &&
             TryAddPrice(parts[5], ref entry);
    }

    private bool TryAddDateTime(string date, string time, ref TradeEntry entry)
    {
      string combined = $"{date} {time},GMT-04:00"; // EST

      var format = "dd/MM/yy HH:mm:ss,'GMT'zzz";
      var provider = CultureInfo.InvariantCulture;

      if (!DateTimeOffset.TryParseExact(combined, format, provider, DateTimeStyles.None, out DateTimeOffset dto))
      {
        return false;
      }

      TimeZoneInfo nyZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
      var dateTime = TimeZoneInfo.ConvertTimeFromUtc(dto.UtcDateTime, nyZone);

      entry.DateTimeEST = dateTime;
      return true;
    }

    private static bool TryAddSymbol(string input, ref TradeEntry entry)
    {
      entry.Symbol = input;
      return Regex.IsMatch(input, @"^[A-Z]{1,8}$");
    }

    private static bool TryAddSide(string input, ref TradeEntry entry)
    {
      if (input == "B") entry.Side = Side.Buy;
      else if (input == "S") entry.Side = Side.Sell;
      else return false;
      return true;
    }

    private static bool TryAddQuantity(string input, ref TradeEntry entry)
    {
      var success = int.TryParse(input, out var quantity);

      if (success)
      {
        entry.Quantity = quantity;
      }

      return success;
    }

    private static bool TryAddPrice(string input, ref TradeEntry entry)
    {
      var success = double.TryParse(input, out var price);

      if (success)
      {
        entry.Price = price;
      }

      return success;
    }
  }
}