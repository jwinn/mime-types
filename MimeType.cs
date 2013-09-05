using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Extensions
{
  /// <summary>
  /// known mime type categories
  /// </summary>
  public enum MimeTypeCategoryType : int
  {
    Application,
    Audio,
    Image,
    Video,
    Unknown
  }

  /// <summary>
  /// interface for mime type
  /// </summary>
  public interface IMimeType
  {
    MimeTypeCategoryType Category { get; }
    string Name { get; }
    string FriendlyName { get; }
    byte[] Signature { get; }
    string[] Extensions { get; }
  }

  /// <summary>
  /// 
  /// </summary>
  public sealed class MimeType : IMimeType
  {
    private static readonly IDictionary<string, IMimeType> mimeTypeMap = new Dictionary<string, IMimeType>( StringComparer.OrdinalIgnoreCase ) {
      { "binary", new MimeType( MimeTypeCategoryType.Application, "application/octet-stream", "Binary", null, string.Empty ) },
      
      { "doc", new MimeType( MimeTypeCategoryType.Application, "application/msword", "Word Document", new byte[] { 208, 207, 17, 224, 161, 177, 26, 225 }, ".doc" ) },
      { "docx", new MimeType( MimeTypeCategoryType.Application, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Word 2007 Document", new byte[] {
        80, 75, 3, 4, 20, 0, 6, 0, 8, 0, 0, 0, 33, 0 }, ".docx" ) },
      { "exe", new MimeType( MimeTypeCategoryType.Application, "application/x-msdownload", "MS Executable", new byte[] {
        77, 90, 144, 0, 3, 0, 0, 0, 4, 0, 0, 0, 255, 255, 0, 0 }, ".exe" ) },
      { "dll", new MimeType( MimeTypeCategoryType.Application, "application/x-msdownload", "Dynamic Link Library", new byte[] {
        77, 90, 144, 0, 3, 0, 0, 0, 4, 0, 0, 0, 255, 255, 0, 0 }, ".dll" ) },
      { "ogx", new MimeType( MimeTypeCategoryType.Application, "application/ogg", "OGG", new byte[] { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 }, ".ogx" ) },
      { "pdf", new MimeType( MimeTypeCategoryType.Application, "application/pdf", "PDF Documents", new byte[] { 37, 80, 68, 70, 45, 49, 46 }, ".pdf" ) },
      { "rar", new MimeType( MimeTypeCategoryType.Application, "application/x-rar-compressed", "RAR Archive", new byte[] { 82, 97, 114, 33, 26, 7, 0 }, ".rar" ) },
      { "swf", new MimeType( MimeTypeCategoryType.Application, "application/x-shockwave-flash", "Shockwave Flash", new byte[] { 70, 87, 83 }, ".swf" ) },
      { "torrent", new MimeType( MimeTypeCategoryType.Application, "application/x-bittorrent", "Bit Torrent", new byte[] { 100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101 }, ".torrent" ) },
      { "ttf", new MimeType( MimeTypeCategoryType.Application, "application/x-font-ttf", "True Type Font", new byte[] { 0, 1, 0, 0, 0 }, ".ttf" ) },
      { "zip", new MimeType( MimeTypeCategoryType.Application, "application/x-zip-compressed", "ZIP Archive", new byte[] { 80, 75, 3, 4, 10 }, ".zip" ) },
      { "png", new MimeType( MimeTypeCategoryType.Image, "image/png", "PNG Image", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 }, ".png" ) },
      { "jpg", new MimeType( MimeTypeCategoryType.Image, "image/jpeg", "JPG Image", new byte[] { 255, 216, 255 }, new string[] { ".jpeg", ".jpg" } ) },
      { "gif", new MimeType( MimeTypeCategoryType.Image, "image/gif", "GIF Image", new byte[] { 71, 73, 70, 56 }, ".gif" ) },
      { "bmp", new MimeType( MimeTypeCategoryType.Image, "image/bmp", "Bitmap Image", new byte[] { 66, 77 }, ".bmp" ) },
      { "ico", new MimeType( MimeTypeCategoryType.Image, "image/x-icon", "Icon", new byte[] { 0, 0, 1, 0 }, ".ico" ) },
      { "tiff", new MimeType( MimeTypeCategoryType.Image, "image/tiff", "TIFF Image", new byte[] { 73, 73, 42, 0 }, ".tiff" ) },
      { "mp3", new MimeType( MimeTypeCategoryType.Audio, "audio/", "MP3", new byte[] { 255, 251, 48 }, ".mp3" ) },
      { "oga", new MimeType( MimeTypeCategoryType.Audio, "audio/ogg", "OGA", new byte[] { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 }, ".oga" ) },
      { "wav", new MimeType( MimeTypeCategoryType.Audio, "audio/x-wav", "WAV", new byte[] { 82, 73, 70, 70 }, ".wav" ) },
      { "wma", new MimeType( MimeTypeCategoryType.Audio, "audio/x-ms-wma", "WMA", new byte[] { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 }, "wma" ) },
      { "ogg", new MimeType( MimeTypeCategoryType.Video, "video/ogg", "OGG", new byte[] { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 }, ".ogg" ) },
      { "avi", new MimeType( MimeTypeCategoryType.Video, "video/x-msvideo", "AVI", new byte[] { 82, 73, 70, 70 }, ".avi" ) },
      { "wmv", new MimeType( MimeTypeCategoryType.Video, "video/x-ms-wmv", "WMV", new byte[] { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 }, ".wmv" ) },

      { "unknown", new MimeType( MimeTypeCategoryType.Unknown, "unknown", "Unknown type", null, string.Empty ) }
    };

    public MimeType( string name, string friendlyName, byte[] signature, string extension )
      : this( MimeTypeCategoryType.Unknown, name, friendlyName, signature, new string[] { extension } ) { }
    public MimeType( MimeTypeCategoryType category, string name, string friendlyName, byte[] signature, string extension )
      : this( category, name, friendlyName, signature, new string[] { extension } ) { }
    public MimeType( string name, string friendlyName, byte[] signature, string[] extensions )
      : this( MimeTypeCategoryType.Unknown, name, friendlyName, signature, extensions ) { }
    public MimeType( MimeTypeCategoryType category, string name, string friendlyName, byte[] signature, string[] extensions )
    {
      this.category = category;
      this.name = name;
      this.friendlyName = friendlyName;
      this.signature = signature;
      this.extensions = extensions;
    }

    #region IMimeType Members

    private MimeTypeCategoryType category;
    public MimeTypeCategoryType Category
    {
      get { return this.category; }
    }

    private string name;
    public string Name
    {
      get { return this.name; }
    }

    private string friendlyName;
    public string FriendlyName
    {
      get { return this.friendlyName; }
    }

    private byte[] signature;
    public byte[] Signature
    {
      get { return this.signature; }
    }

    private string[] extensions;
    public string[] Extensions
    {
      get { return this.extensions; }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    // TODO: change to more efficient (non-LINQ) method
    public static IMimeType GetMimeTypeFromExt( string extension )
    {
      var mimeType = mimeTypeMap.Values
        .FirstOrDefault( mt => mt.Extensions != null &&
          mt.Extensions.SingleOrDefault( e => e.Equals( extension, StringComparison.OrdinalIgnoreCase ) ) != null );

      return ( mimeType == null ) ? mimeTypeMap[ "unknown" ] : mimeType;
    }

    /// <summary>
    /// check an array of bytes against image decoders to determine mime type
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    // TODO: change to more efficient (non-LINQ) method
    public static IMimeType GetImageMimeType( byte[] data )
    {
      Guid id = Guid.Empty;

      using ( MemoryStream stream = new MemoryStream( data, 0, data.Length ) )
      {
        try
        {
          Image img = Image.FromStream( stream, true );
          id = img.RawFormat.Guid;
          foreach ( ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders() )
          {
            if ( codec.FormatID.Equals( id ) )
            {
              return mimeTypeMap.Values.Single( mt => mt.Name.Equals( codec.MimeType, StringComparison.OrdinalIgnoreCase ) );
            }
          }
        }
        catch
        {
        }
      }

      return mimeTypeMap[ "unknown" ];
    }

    /// <summary>
    /// compares the provided byte array to known mime type byte signatures and extension, if provided, to find the mime type
    /// </summary>
    /// <param name="data"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    // TODO: change to more efficient (non-LINQ) method
    public static IMimeType GetMimeType( byte[] data, string extension = null )
    {
      IMimeType mimeType = null;

      var mimeTypes = mimeTypeMap.Values
        .Where( mt => mt.Signature != null && mt.Signature.SequenceEqual( data.Take(mt.Signature.Length) ) );

      // if more than one, with the same signature, are found, try to compare by extension
      if ( mimeTypes.Count() > 1 && string.IsNullOrWhiteSpace( extension ) == false )
      {
        mimeTypes = mimeTypes.Where( mt => mt.Extensions.SingleOrDefault(
          e => e.Equals( extension, StringComparison.OrdinalIgnoreCase ) ) != null );
      }
      else if ( mimeTypes.Count() == 0 && string.IsNullOrWhiteSpace( extension ) == false )
      {
        // try the extension
        return GetMimeTypeFromExt( extension );
      }

      // null or the first found
      // TODO: return all found mimeTypes?
      mimeType = mimeTypes.FirstOrDefault();
      return ( mimeType == null ) ? mimeTypeMap[ "binary" ] : mimeType;
    }
  }
}
