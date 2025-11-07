namespace TradeConverter.Exporters
{
  public interface IExporter
  {
    string Name { get; }
    bool TryWriteToFile(string filePath, TradeEntry[] entries);
  }
}