using System.CodeDom.Compiler;

namespace SourceGeneratorWriter.Tests;

public class IndentedTextWriterBlockTests
{
	/// <summary>Runs <paramref name="emit"/> against a tab-indented writer and returns the
	/// produced text with newlines normalized to <c>\n</c> for stable assertions.</summary>
	private static string Emit(Action<IndentedTextWriter> emit)
	{
		StringWriter text = new();
		IndentedTextWriter writer = new(text, "\t");
		emit(writer);
		return writer.InnerWriter.ToString()!.Replace(writer.NewLine, "\n").TrimEnd();
	}

	[Fact]
	public void Header_only_writes_a_single_line_with_no_braces()
	{
		var result = Emit(writer => writer.Block("class Foo"));

		Assert.Equal("""
			class Foo
			""", result);
	}

	[Fact]
	public void Header_with_callback_wraps_body_in_braces_and_indents_one_level()
	{
		var result = Emit(writer => writer.Block("class Foo", () => writer.WriteLine("int x;")));

		Assert.Equal("""
			class Foo
			{
				int x;
			}
			""", result);
	}

	[Fact]
	public void Additional_block_carrying_both_text_and_callback_emits_a_second_braced_section()
	{
		// This is how SQuiL writes try/catch: the params block carries BOTH a header and a callback,
		// so the header is written and then its own braced body — producing catch { } after try { }.
		var result = Emit(writer => writer.Block(
			"try",
			() => writer.WriteLine("a();"),
			"catch (Exception e)",
			(Action)(() => writer.WriteLine("b();"))));

		Assert.Equal("""
			try
			{
				a();
			}
			catch (Exception e)
			{
				b();
			}
			""", result);
	}

	[Fact]
	public void Leading_tabs_in_a_multiline_header_adjust_indentation_per_line()
	{
		// The header logic counts leading tabs per line and bumps writer.Indent for that line,
		// then restores it — so raw-string headers with tab indentation format correctly.
		var result = Emit(writer => writer.Block("public void M()\n\t=> inner();"));

		Assert.Equal("""
			public void M()
				=> inner();
			""", result);
	}

	[Fact]
	public void Nested_blocks_compound_indentation()
	{
		var result = Emit(writer => writer.Block("namespace N", () =>
			writer.Block("class C", () => writer.WriteLine("int x;"))));

		Assert.Equal("""
			namespace N
			{
				class C
				{
					int x;
				}
			}
			""", result);
	}

	[Fact]
	public void Implicit_conversion_from_string_produces_a_text_block()
	{
		IndentedTextWriterBlock block = "header";

		Assert.True(block.IsText);
		Assert.False(block.IsCallback);
		Assert.Equal("header", block);
	}

	[Fact]
	public void Implicit_conversion_from_action_produces_a_callback_block()
	{
		var ran = false;
		IndentedTextWriterBlock block = (Action)(() => ran = true);

		Assert.True(block.IsCallback);
		Assert.False(block.IsText);

		block.Callback();
		Assert.True(ran);
	}
}
