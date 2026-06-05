namespace System.CodeDom.Compiler;

/// <summary>
/// <inheritdoc cref="IndentedTextWriter"/> <seealso cref="SourceGeneratorWriter"/>
/// adds a <see cref="SourceGeneratorWriter.Block"/> method to facilitate wrapping 
/// and indenting code that should be surrounded by <c>{</c> / <c>}</c>. Implments 
/// <seealso cref="IDisposable"/> and should be used with <c>using</c> or have the 
/// <c>Dispose</c> method called.
/// </summary>
/// <param name="TabString">Specifies the default value used for the tab output.</param>
public class SourceGeneratorWriter(string TabString = "\t")
	: IndentedTextWriter(new StringWriter(), TabString)
{
	private string Tab { get; } = TabString;

	/// <inheritdoc cref="Block(string, Action, SourceGeneratorWriterBlock[])" />
	public void Block(string header, params SourceGeneratorWriterBlock[] blocks)
		=> Block(header, default!, blocks);

	/// <inheritdoc cref="Block(string, Action, SourceGeneratorWriterBlock[])" />
	public void Block(
		string header,
		Action callback = default!,
		params SourceGeneratorWriterBlock[] blocks)
		=> Block(this, Tab, header, callback, blocks);

	/// <summary>
	/// Writes <paramref name="header"/> line(s) respecting leading-tab indentation, then invokes
	/// <paramref name="callback"/> (if provided) surrounded by <c>{</c> / <c>}</c>, and repeats
	/// the same pattern for any additional <paramref name="blocks"/>.
	/// </summary>
	/// <param name="writer"><inheritdoc cref="IndentedTextWriter"/></param>
	/// <param name="tabString"><inheritdoc cref="SourceGeneratorWriter"/></param>
	/// <param name="header">Text to be written verbatium with respect to leading tabs.</param>
	/// <param name="callback">A block of indented code surrounded by <c>{</c> / <c>}</c>.</param>
	/// <param name="blocks">Chainable <paramref name="header"/> and <paramref name="callback"/>.</param>
	internal static void Block(
		IndentedTextWriter writer,
		string tabString,
		string header,
		Action callback = default!,
		params SourceGeneratorWriterBlock[] blocks)
	{
		Header(header);
		Invoke(callback);

		foreach (var block in blocks)
		{
			Header(block.Text);
			Invoke(block.Callback);
		}

		void Header(string? text)
		{
			if (text is null)
				return;

			foreach (var header in text.Replace(writer.NewLine, "\n").Split('\n'))
			{
				CountTabs(header, out var index, out var tabs);

				writer.Indent += tabs;
				writer.WriteLine(header[index..]);
				writer.Indent -= tabs;
			}

			void CountTabs(string header, out int index, out int tabs)
			{
				index = 0;
				tabs = 0;

				while (index < header.Length)
				{
					if (!header[index..].StartsWith(tabString))
						return;

					index += tabString.Length;
					tabs++;
				}
			}
		}

		void Invoke(Action? callback)
		{
			if (callback is null)
				return;

			writer.WriteLine("{");
			writer.Indent++;
			callback();
			writer.Indent--;
			writer.WriteLine("}");
		}
	}

	public override string ToString() => InnerWriter.ToString();

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		InnerWriter.Dispose();
	}
}
