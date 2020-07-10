﻿using DefinitionFileBuillder;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

public static class CocolExtractor
{
    public static DefinitionFileBuilder Builder { get; private set; }

    // all characters that are valid in XML
    public static readonly ImmutableHashSet<char> ANY_CHARACTER_SET = ImmutableHashSet.CreateRange(
            Enumerable.Range(char.MinValue, char.MaxValue - char.MinValue)
            .Select(i => (char)i)
            .Where(c => XmlConvert.IsXmlChar(c)));
    public static readonly ImmutableHashSet<char> ANY_UPPERCASE_SET = ImmutableHashSet.CreateRange(Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i));
    public static readonly ImmutableHashSet<char> ANY_LOWERCASE_SET = ImmutableHashSet.CreateRange(Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i));
    public static readonly ImmutableHashSet<char> ANY_LETTER_SET = ImmutableHashSet.CreateRange(ANY_CHARACTER_SET.Where(ch => char.IsLetter(ch)));
    public static readonly ImmutableHashSet<char> ANY_DIGIT_SET = ImmutableHashSet.CreateRange(ANY_CHARACTER_SET.Where(ch => char.IsDigit(ch)));
    public static readonly ImmutableHashSet<char> ANY_LETTER_OR_DIGIT_SET = ImmutableHashSet.CreateRange(ANY_LETTER_SET.Union(ANY_DIGIT_SET));
    public static readonly ImmutableHashSet<char> ANY_WHITESPACE_SET = ImmutableHashSet.CreateRange(ANY_CHARACTER_SET.Where(ch => char.IsWhiteSpace(ch)));

    public static void Main(string[] args)
    {
        string outputFile;
        if (args.Length == 0)
            outputFile = Console.ReadLine();
        else
            outputFile = args[0];

        Builder = new DefinitionFileBuilder();
        Console.WriteLine("finished set-up");

        Cocol2Extractor.Main(args.Skip(1).ToArray());

        Console.WriteLine("building file");
        var langDef = Builder.Build();

        Console.WriteLine($"While Building the file, there were {Builder.Errors.Count} error{(Builder.Errors.Count == 1 ? "" : "s")}");

        foreach (var error in Builder.Errors)
            Console.WriteLine('\t' + error);


        Console.WriteLine("Verifying result…");
        var errors = langDef.VerifyAndGetErrors();
        if (errors.Length == 0)
            Console.WriteLine("No errors found :)");
        else
            Console.WriteLine($"Found {errors.Length} error{(errors.Length == 1 ? "" : "s")}:");

        foreach (var error in errors)
            Console.WriteLine('\t' + error);

        if (errors.Length != 0)
            return;

        Console.WriteLine("Serializing…");
        var xml = langDef.SerializeToXLinq();

        Console.WriteLine($"Writing xml into {outputFile}…");
        using (var stream = File.OpenWrite(outputFile)) {
            new XDocument(new XDeclaration("1.0", "UTF-16", "yes"), xml).Save(stream, SaveOptions.None);
        }
    }
}
