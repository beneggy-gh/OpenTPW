using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenTPW;
public class BFFont
{
	public List<BFFontCharacter> BFFontCharactersList;
	public BFFont(List<BFFontCharacter> bFFontCharacterList)
	{
		BFFontCharactersList = bFFontCharacterList;
	}

	public void AddCharacter( BFFontCharacter character )
	{
		BFFontCharactersList.Add( character );
	}
	
	public void ExportFontToTTF( string path )
	{
		var dataLength = 0;
		foreach( var character in BFFontCharactersList )
		{
			dataLength += character.FontData.Length;
		}

		byte[] data = new byte[dataLength];
		int currentPos = 0;
		int arrayPos = 0;
		foreach( var character in BFFontCharactersList )
		{
			for( var i = 0; i < character.FontData.Length; i++ )
			{
				data[arrayPos + i] = character.FontData[i];
				currentPos = i;
			}
			arrayPos += currentPos;
		}

		File.WriteAllBytes( path, data );
	}

}
