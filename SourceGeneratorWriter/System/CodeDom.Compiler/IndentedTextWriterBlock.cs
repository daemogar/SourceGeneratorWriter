namespace System.CodeDom.Compiler;

/// <inheritdoc cref="SourceGeneratorWriter"/>
[Obsolete("Use SourceGeneratorWriterBlock instead. This is being removed and is a one for one replacement.")]
public record IndentedTextWriterBlock(string Text, Action Callback)
	: SourceGeneratorWriterBlock(Text, Callback)
{ }