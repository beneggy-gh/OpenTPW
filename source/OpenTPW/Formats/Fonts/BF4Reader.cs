using System.IO;
using System;

namespace OpenTPW;
public class BF4Reader : BaseFormat
{
	private BF4Stream memoryStream;
	public byte[] buffer;

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
		Log.Info( $"Data Size:  {maxWidth}", true );

		var maxHeight = memoryStream.ReadByte();
		Log.Info( $"Data Size:  {maxHeight}", true );

		var offsetCount = memoryStream.ReadUIntN( 2 );
		List<int> offsetList = new List<int>();
		
		for(int i = 0; i < offsetCount; i++ )
		{
			offsetList.Add( memoryStream.ReadInt32() );
		}
		
		// FONT DATA
		foreach( var offset in offsetList )
		{
			var character = memoryStream.ReadChars( 1 );
			Log.Info( $"Character:  {character}", true );

			// pass through null byte
			_ = memoryStream.ReadByte();

			// data
			var dataSize = memoryStream.ReadInt32();
			Log.Info( $"Data Size:  {dataSize}", true );


			// unknown
			_ = memoryStream.ReadInt32();

			var width = memoryStream.ReadUIntN( 2 );
			Log.Info( $"Width:  {width}", true );

			var height = memoryStream.ReadUIntN( 2 );
			Log.Info( $"Height:  {height}", true );

			var offsetX = memoryStream.ReadUIntN( 1 );
			Log.Info( $"Offset X:  {offsetX}", true );

			var offsetY = memoryStream.ReadUIntN( 1 );
			Log.Info( $"Offset Y:  {offsetY}", true );

			var bezel = memoryStream.ReadUIntN( 2 );
			Log.Info( $"Bezel:  {bezel}", true );
		}
	}
}
