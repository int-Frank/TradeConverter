namespace TradeConverter.Exporters
{
  public interface IExporter
  {
    string Name { get; }
    bool TryWriteToFile(string filePath, Transaction[] entries);
  }
}