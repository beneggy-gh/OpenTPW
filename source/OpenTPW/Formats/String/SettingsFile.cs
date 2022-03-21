﻿namespace OpenTPW;

using SettingsPair = MutablePair<string, string>;

public class SettingsFile
{
	// TODO: Read hoarding & shape data

	private static bool IsWhiteSpace( char character ) => character == ' ' || character == '\t';
	private static bool IsNewLine( char character ) => character == '\n' || character == '\r';

	public List<SettingsPair> Entries { get; set; } = new();

	public SettingsFile( Stream stream )
	{
		using var binaryReader = new BinaryReader( stream );

		var inComment = false;
		var inString = false;
		var isKey = true;

		var wordBuffer = "";
		var lineBuffer = new SettingsPair();
		var samPairs = new List<SettingsPair>();

		while ( binaryReader.BaseStream.Position < binaryReader.BaseStream.Length - 1 )
		{
			var character = binaryReader.ReadChar();
			if ( character == '#' )
			{
				// Comment - ignore until next newline
				inComment = true;
				continue;
			}
			else if ( character == '"' )
			{
				// String - register all characters until the next '"'
				inString = !inString;
				continue;
			}

			if ( (IsWhiteSpace( character ) && !inString) || IsNewLine( character ) )
			{
				if ( inComment )
				{
					if ( IsNewLine( character ) )
						inComment = false;

					wordBuffer = "";
					lineBuffer = new SettingsPair();
					continue;
				}

				if ( !string.IsNullOrEmpty( wordBuffer ) )
				{
					if ( isKey )
					{
						if ( lineBuffer.Item1 == null )
						{
							lineBuffer.Item1 = wordBuffer ?? default;
							isKey = false;
						}
					}
					else
					{
						if ( lineBuffer.Item2 == null )
						{
							lineBuffer.Item2 = wordBuffer ?? default;
							isKey = true;
						}
					}
				}

				if ( IsNewLine( character ) )
				{
					if ( lineBuffer.Item1 != null && lineBuffer.Item2 != null )
						samPairs.Add( lineBuffer );

					lineBuffer = new SettingsPair();
				}

				wordBuffer = "";
				continue;
			}

			wordBuffer += character;
		}

		Entries = samPairs.ToList();
	}
}
