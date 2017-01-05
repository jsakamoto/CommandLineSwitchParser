# CommandLineSwitchParser [![NuGet Package](https://img.shields.io/nuget/v/CommandLineSwitchParser.svg)](https://www.nuget.org/packages/CommandLineSwitchParser/)

## Summary

This is a simple parser for command line options for C# (.NET).

What you should do is only define your option class and write code like "`var option = CommandLineSwicth.Parse<YourOptionClass>(ref args);`".

There are no need to annotate your option class with any attributes.

## Install

    PM> Install-Package CommandLineSwitchParser

## Example

```csharp
// Declare your own class that represents command line switch.
public class Option1
{
    public bool Recuruce { get; set; }

    public int Port { get; set; } = 8080;
}

// if args is "--port 80 c:\wwwroot\inetpub -r", then ...
public static void Main(string[] args)
{
  var options = CommandLineSwitch.Parse<Option1>(ref args);

  // Short switch matches a initial of option class property name.
  Console.WriteLine(options.Recuruce); // True

  // Long switch matches a full name of option class property name.
  Console.WriteLine(options.Port);     // 80

  // Switches in "args" is stripped.
  Console.WriteLine(args[0])           // "c:\wwwroot\inetpub"
}

```

## License

[Mozilla Public License, version 2.0](LICENSE)
