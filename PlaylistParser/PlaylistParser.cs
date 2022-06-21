using PlaylistParser.DataStructs;
using PlaylistParser.Parsers;
using System.Collections.Generic;

namespace PlaylistParser;
/// <summary>
///     This class is for parsing playlists from websites, which matches with avialable patterns.
/// </summary>
internal class PlaylistsParser
{
    public Dictionary<string, IPLaylistParser> parsers = new();
    /// <summary>
    ///     Constructor for adding patterns.
    /// </summary>
    public PlaylistsParser()
    {
        parsers.Add("djpoolrecords", new DjpoolrecordsPlaylistsParser());
    }
    /// <summary>
    ///     Method is choose pattern and parse playlists then using it.
    /// </summary>
    public Playlist?[]? ParsePlaylists(ParseRequest pr)
    {
        IPLaylistParser? parser;
        parsers.TryGetValue(pr.ParserName, out parser);
        
        return parser == null ? null : parsers[pr.ParserName].ParsePlaylists(pr.Uri);
    }
}
