namespace System.CodeDom.Compiler;

/// <summary>
/// A discriminated union passed to <see cref="IndentedTextWriterExtensions.Block"/> to represent
/// either a string header fragment or an <see cref="Action"/> callback that writes a braced block.
/// Implicit conversions allow callers to pass either type without explicit wrapping.
/// </summary>
/// <param name="Text">The string header text, or <c>null</c> when this instance wraps a callback.</param>
/// <param name="Callback">The action callback, or <c>null</c> when this instance wraps a string.</param>
public record IndentedTextWriterBlock(string Text, Action Callback)
{
	/// <summary><c>true</c> when this instance wraps a string header.</summary>
	public bool IsText => Text is not null;

	/// <summary><c>true</c> when this instance wraps a callback.</summary>
	public bool IsCallback => Callback is not null;

	/// <inheritdoc cref="IndentedTextWriterBlock"/>
	public IndentedTextWriterBlock(string Text) : this(Text, default!) { }

	/// <inheritdoc cref="IndentedTextWriterBlock"/>
	public IndentedTextWriterBlock(Action Callback) : this(default!, Callback) { }

	/// <summary>Wraps a string as a text block.</summary>
	public static implicit operator IndentedTextWriterBlock(string text) => new(text);

	/// <summary>Wraps a callback as an action block.</summary>
	public static implicit operator IndentedTextWriterBlock(Action callback) => new(callback);
}
