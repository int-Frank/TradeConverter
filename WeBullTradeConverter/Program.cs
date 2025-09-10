
namespace WebullConverter
{

  public class Program
  {
    static void Main(string[] args)
    {
      var program = new Program();
      program.Run();
    }

    public void Run()
    {
      var reader = new WebullReader();
      reader.Read("C:/Users/frank/Downloads/Webull_Trade_Record_01_01_2025_to_10_09_2025.csv");
    }
  }
}