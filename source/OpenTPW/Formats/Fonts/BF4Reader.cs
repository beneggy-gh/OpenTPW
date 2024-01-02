using System.Drawing;
using System.Drawing.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Vortice.D3DCompiler;

namespace OpenTPW;
public class BF4Reader : BaseFormat
{
	private BF4Stream memoryStream;
	public byte[] buffer;
	public PrivateFontCollection pfc;

	public BF4Reader( string path )
	{
		using var fileStream = File.OpenRead( path );
		ReadFromStream( fileStream );
	}

	public BF4Reader( Stream stream )
	{
		ReadFromStream( stream );
	}
	public void Dispose()
	{
		memoryStream.Dispose();
	}

	protected override void ReadFromStream( Stream stream )
	{
		// Set up read buffer
		var tempStreamReader = new StreamReader( stream );
		var fileLength = (int)tempStreamReader.BaseStream.Length;

		buffer = new byte[fileLength];
		tempStreamReader.BaseStream.Read( buffer, 0, fileLength );
		tempStreamReader.Close();
		memoryStream = new BF4Stream( buffer );

		ReadFile();
	}

	private void ReadFile()
	{
		memoryStream.Seek( 0, SeekOrigin.Begin );

		/*
			Header
				4 bytes: magic number - "F4FB"
				1 byte: max width
				1 byte: max height
				2 bytes: offset count
				n bytes: offset list
			Font entry
				2 bytes: character
				2 bytes: unknown
				4 bytes: data size
				4 bytes: unknown
				4 bytes: unknown
				2 bytes: width
				2 bytes: height
				1 byte: offset X
				1 byte: offset Y
				2 bytes: outer width
		*/

		var magicNumber = memoryStream.ReadString( 4 );
		if ( magicNumber != "F4FB" )
			throw new Exception( $"Magic number did not match: {magicNumber}" );

		var maxWidth = memoryStream.ReadByte();
		Log.Info( $"Max Width:  {maxWidth}", true );

		var maxHeight = memoryStream.ReadByte();
		Log.Info( $"Max Height:  {maxHeight}", true );

		var offsetCount = BitConverter.ToInt16( memoryStream.ReadBytes( 2 ), 0 );
		List<int> offsetList = new List<int>();

		for ( int i = 0; i < offsetCount; i++ )
		{
			offsetList.Add( memoryStream.ReadInt32() );
		}

		List<BFFontCharacter> characterInfo = new List<BFFontCharacter>();

		// FONT DATA
		foreach ( var offset in offsetList )
		{
			Log.Info( $"______________________________________________________", true );

			memoryStream.Seek( offset, SeekOrigin.Begin );
			var character = memoryStream.ReadString( 1, false );
			Log.Info( $"Character:  {character}", true );

			// Null bytes
			_ = memoryStream.ReadBytes( 3 );

			// data
			var dataSize = memoryStream.ReadInt32();
			Log.Info( $"Data Size:  {dataSize}", true );

			// Unknown
			_ = memoryStream.ReadInt32();

			// Not sure what this is - changes value, but could be connected to previous unknown
			// Possibly bezel?
			_ = memoryStream.ReadInt32();

			var width = BitConverter.ToInt16( memoryStream.ReadBytes( 2 ) );
			Log.Info( $"Width:  {width}", true );

			var height = BitConverter.ToInt16( memoryStream.ReadBytes( 2 ) );
			Log.Info( $"Height:  {height}", true );

			var offsetX = BitConverter.ToInt16( memoryStream.ReadBytes( 2, true ) );
			Log.Info( $"Offset X:  {offsetX}", true );

			var offsetY = BitConverter.ToInt16( memoryStream.ReadBytes( 2 ) );
			Log.Info( $"Offset Y:  {offsetY}", true );

			//var bezel = BitConverter.ToInt16( memoryStream.ReadBytes( 2 ) );
			var bezel = 1;
			//Log.Info( $"Bezel:  {bezel}", true );

			var data = memoryStream.ReadBytes( dataSize );
			Log.Info( $"Data Length:  {data.Length}", true );
			Log.Info( $"", true );

			characterInfo.Add( new BFFontCharacter( character, dataSize, width, height, offsetX, offsetY, bezel, data ) );

			//var fontData = memoryStream.ReadBytes( dataSize + 18 );
			//unsafe
			//{
			//	fixed ( byte* pFontData = fontData )
			//	{
			//		pfc.AddMemoryFont( (IntPtr)pFontData, fontData.Length );
			//	}
			//}
		}

		BFFont font = new BFFont( characterInfo );
		font.ExportFontToTTF( $"content//fonts/BF4//font" );
	}
}
