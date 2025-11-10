namespace TradeConverter.Importers
{
  public interface IImporter
  {
    bool TryRead(string filePath, out Transaction[] entries);
  }
}