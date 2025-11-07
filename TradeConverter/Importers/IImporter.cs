namespace TradeConverter.Importers
{
  public interface IImporter
  {
    bool TryRead(string filePath, out TradeEntry[] entries);
  }
}