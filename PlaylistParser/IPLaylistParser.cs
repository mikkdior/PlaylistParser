using PlaylistParser;
using PlaylistParser.DataStructs;

namespace PlaylistParser;
internal interface IPLaylistParser
{
    Playlist[]? ParsePlaylists(string uri);
}
