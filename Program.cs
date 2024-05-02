namespace TFScriptToAliasWriter;
using System.Diagnostics;
using System.Text;

public static class Program
{
	public static void Main(params string[] args)
	{
		string fromPath = args.Length >= 1 ? args[0] : @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\script.txt";
		string toPath = args.Length >= 2 ? args[1] : @$"{Environment.CurrentDirectory}\autoexec.cfg";
		string aliasName = args.Length >= 3 ? args[2] : "scpt";
		string waitDelay = args.Length >= 3 ? args[3] : "1450";

		Console.WriteLine($"Reading {fromPath}..");
		Console.WriteLine($"Planning to write to {toPath}..");



		string fullReadString = File.ReadAllText(fromPath);

		int maxCharacterLimit = 120;
		string[] strings = fullReadString.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		Console.WriteLine($"Read file, using '{aliasName}' as sequential alias name, and using '{waitDelay}' as wait delay..\n\n");
		StringBuilder stringBuilder = NewBuilder();
		List<string> splitSentences = new();
		char[] punctuation = new char[] { '.', ',', '?', '!' };
		for (int i = 0; i < strings.Length; i++)
		{
			string currentString = strings[i];
			List<StringBuilder> punctuationSplit = new() { new StringBuilder() };
			for (int ii = 0; ii < currentString.Length; ii++)
			{
				punctuationSplit[^1].Append(currentString[ii]);
				if (punctuation.Any(@char => currentString[ii] == @char))
					punctuationSplit.Add(new StringBuilder());
			}
			string[] puncRemake = new string[punctuationSplit.Count];
			for (int ii = 0; ii < punctuationSplit.Count; ii++)
				puncRemake[ii] = punctuationSplit[ii].ToString().Trim();

			for (int ii = 0; ii < puncRemake.Length; ii++)
			{
				int addedCount = stringBuilder.Length + puncRemake[ii].Length;
				if (addedCount > maxCharacterLimit)
				{
					splitSentences.Add(stringBuilder.ToString().Trim());
					stringBuilder = NewBuilder();
				}
				stringBuilder.Append($"{puncRemake[ii]} ");
			}
		}
		if (stringBuilder.Length > 0)
			splitSentences.Add(stringBuilder.ToString().Trim());

		string fullStream = string.Join($"; wait {waitDelay}; ", splitSentences);
		string[] resplit = fullStream.Split(';');
		int maxAliasLimit = byte.MaxValue;
		List<StringBuilder> builderList = new() { NewAliasBuilder(0) };
		for (int i = 0; i < resplit.Length; i++)
		{
			int currentLength = builderList[^1].Length;
			if (currentLength + resplit[i].Length > maxAliasLimit)
			{
				builderList[^1].Replace("[1]", "");
				builderList.Add(NewAliasBuilder(builderList.Count));
			}
			builderList[^1].Replace("[1]", $"{resplit[i]};[1]");
		}
		string lastString = builderList[^1].ToString();
		lastString = lastString.Remove(lastString.LastIndexOf('[')) + '"';
		builderList[^1] = new(lastString);

		using (StreamWriter stream = new(File.Create(toPath)))
		{
			for (int i = 0; i < builderList.Count; i++)
				stream.WriteLine(builderList[i].ToString());
			stream.Close();
		}

		Console.WriteLine("Succeeded! Press any key to continue.");
		Console.ReadKey();

		StringBuilder NewBuilder() => new StringBuilder(maxCharacterLimit).Append("say ");
		StringBuilder NewAliasBuilder(int currentIndex)
		{
			string subsequentBee = $"{aliasName}{currentIndex + 1}";
			return new StringBuilder($"alias {aliasName}{currentIndex} \"[1]echo {subsequentBee};{subsequentBee}\"");
		}
	}
}