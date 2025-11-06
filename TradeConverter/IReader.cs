namespace TradeConverter
{
  public interface IReader
  {
    bool TryRead(string filePath, out TradeEntry[] entries);
  }
}