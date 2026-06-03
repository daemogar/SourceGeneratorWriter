# SourceGeneratorWriter

Reusable [`IndentedTextWriter`](https://learn.microsoft.com/dotnet/api/system.codedom.compiler.indentedtextwriter)
helpers for authoring Roslyn source generators. It provides a small `Block(...)` extension that makes
emitting indentation-aware, brace-wrapped C# source readable — so generator projects don't each
re-implement their own indenting writer.

## Install

```
dotnet add package SourceGeneratorWriter
```

Target `netstandard2.0` from your analyzer/source-generator project (the standard generator runtime
requirement). Because the API lives in the `System.CodeDom.Compiler` namespace, the `Block`
extension is available wherever you already have a `using System.CodeDom.Compiler;` for
`IndentedTextWriter`.

## Usage

```csharp
using System.CodeDom.Compiler;
using System.IO;

StringWriter text = new();
IndentedTextWriter writer = new(text, "\t");

writer.Block("namespace Demo", () =>
    writer.Block("public sealed class Greeter", () =>
        writer.Block("public string Hello(string name)", () =>
            writer.WriteLine("return $\"Hello, {name}!\";"))));
```

produces:

```csharp
namespace Demo
{
	public sealed class Greeter
	{
		public string Hello(string name)
		{
			return $"Hello, {name}!";
		}
	}
}
```

### Multiple sections (`try` / `catch`)

`Block` takes a header, an optional callback, then any number of additional `IndentedTextWriterBlock`
params. A params block can carry **both** a header and a callback, which emits a second braced
section — ideal for `try { } catch { }`:

```csharp
writer.Block(
	"try",
	() => writer.WriteLine("Risky();"),
	"catch (Exception e)",
	((Action)() => writer.WriteLine("Log(e);")));
```

```csharp
try
{
	Risky();
}
catch (Exception e)
{
	Log(e);
}
```

### Header indentation

A header string is split on the writer's newline and each line's **leading tabs** adjust the indent
level for that line, so multi-line raw-string headers format correctly.

## API

| Member | Description |
| --- | --- |
| `IndentedTextWriter.Block(header, callback?, params blocks)` | Writes the header, optionally a `{ }`-wrapped callback body, then repeats for each additional block. |
| `IndentedTextWriterBlock` | A union of *either* a string header *or* an `Action` callback, with implicit conversions both ways. Construct with both to emit a header followed by its own braced body. |

## License

[GNU AGPL v3](LICENSE.txt).
