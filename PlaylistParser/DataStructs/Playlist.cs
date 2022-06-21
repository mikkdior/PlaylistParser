using System;
using System.Collections.Generic;

namespace PlaylistParser.DataStructs;
public record Playlist(string Name, DateOnly PublishDate, List<Song> Songs);