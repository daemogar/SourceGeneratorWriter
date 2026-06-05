namespace System.CodeDom.Compiler;

/// <summary>
/// Adds a <see cref="SourceGeneratorWriter.Block"/> method to facilitate wrapping and indenting code 
/// that should be surrounded by <c>{</c> / <c>}</c>. Implments <seealso cref="IDisposable"/> and 
/// should be used with <c>using</c> or have the <c>Dispose</c> method called. This method does not 
/// respect the <c>TabString</c> value of <see cref="IndentedTextWriter"/>, so is being replaced with
/// <seealso cref="SourceGeneratorWriter"/>.
/// </summary>
[Obsolete("Use SourceGeneratorWriter instead. This extension does not respect the TabString of IndentedTextWriter.")]
public static class IndentedTextWriterExtensions
{
	/// <inheritdoc cref="SourceGeneratorWriter.Block(IndentedTextWriter, string, string, Action, SourceGeneratorWriterBlock[])"/>
	[Obsolete("Use SourceGeneratorWriter instead. This method does not respect the TabString of IndentedTextWriter.")]
	public static void Block(
		this IndentedTextWriter writer,
		string header,
		Action callback = default!,
		params SourceGeneratorWriterBlock[] blocks)
		=> SourceGeneratorWriter.Block(writer, header, "\t", callback, blocks);
}
