using System.IO;

namespace WeBullTradeConverter.Args
{
  public class ConverterContext
  {
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public bool FixDates { get; set; } = false;
  }

  public interface IParserState
  {
    (bool, IParserState?) ProcessToken(string token, ref ConverterContext context);
  }

  public class InputParser : IParserState
  {
    public (bool, IParserState?) ProcessToken(string token, ref ConverterContext context)
    {
      if (!Directory.Exists(token))
      {
        Console.Write($"Invalid input path: '{token}'");
        throw new Exception();
      }

      context.InputPath = token;
      return (true, null);
    }
  }

  public class OutputParser : IParserState
  {
    public (bool, IParserState?) ProcessToken(string token, ref ConverterContext context)
    {
      if (!Directory.Exists(token))
      {
        Console.Write($"Invalid output path: '{token}'");
        throw new Exception();
      }

      context.OutputPath = token;
      return (true, null);
    }
  }

  public class FixDatesParser : IParserState
  {
    public (bool, IParserState?) ProcessToken(string token, ref ConverterContext context)
    {
      var falseFlags = new[] { "false", "0" };
      var trueFlags = new[] { "true", "1" };

      if (falseFlags.Contains(token.ToLower()))
      {
        context.FixDates = false;
      }
      else if (trueFlags.Contains(token.ToLower()))
      {
        context.FixDates = true;
      }
      else
      {
        Console.Write($"Invalid token for fix dates flag: '{token}'");
        throw new Exception();
      }

      return (true, null);
    }
  }

  public class BaseParserState : IParserState
  {
    public (bool, IParserState?) ProcessToken(string token, ref ConverterContext context)
    {
      IParserState? newState = null;
      var finished = false;

      if (token is null)
      {
        finished = true;
      }
      else if (token == "-i")
      {
        newState = new InputParser();
      }
      else if (token == "-o")
      {
        newState = new OutputParser();
      }
      else if (token == "-f")
      {
        newState = new FixDatesParser();
      }
      else
      {
        Console.Write($"Unrecognised token while reading arguments: '{token}'");
      }

      return (finished, newState);
    }
  }

  public class ArgService
  {
    public ConverterContext Execute(string[] args)
    {
      var context = new ConverterContext();
      var states = new Stack<IParserState>();
      states.Push(new BaseParserState());

      foreach (var token in args)
      {
        (var done, var newState) = states.Peek().ProcessToken(token, ref context);

        if (done)
        {
          states.Pop();
        }

        if (newState is not null)
        {
          states.Push(newState);
        }
      }

      return context;
    }
  }
}