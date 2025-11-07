using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TradeConverter.Importers
{
  public class WarriorTradingSimImporter : IImporter
  {
    public bool TryRead(string filePath, out TradeEntry[] entries)
    {
      if (!IsValidFile(filePath))
      {
        entries = Array.Empty<TradeEntry>();
        return false;
      }

      Console.WriteLine($"Importing Warrior Trading Sim file '{Path.GetFileName(filePath)}'...");

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
        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();

          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          return TryParseLine(line, out var entry) && entry is not null;
        }
      }

      return false;
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
      // Assume EST
      string combined = $"{date} {time}";

      var format = "MM/dd/yy HH:mm:ss";
      entry.DateTimeEST = DateTime.ParseExact(combined,
                                              format,
                                              CultureInfo.InvariantCulture,
                                              DateTimeStyles.None);

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