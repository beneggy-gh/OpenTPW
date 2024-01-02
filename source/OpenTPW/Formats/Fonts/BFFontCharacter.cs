namespace OpenTPW;
public class BFFontCharacter
{
	public string Character;	// Using string as easier than converting from char[] :')
	public int DataSize;
	public int Width;
	public int Height;
	public int OffsetX;
	public int OffsetY;
	public int Bezel;
	public byte[] FontData;

	public BFFontCharacter( string character, int dataSize, int width, int height, int offsetX, int offsetY, int bezel, byte[] data )
	{
		Character = character;
		DataSize = dataSize;
		Width = width;
		Height = height;
		OffsetX = offsetX;
		OffsetY = offsetY;
		Bezel = bezel;
		FontData = data;
	}
}
