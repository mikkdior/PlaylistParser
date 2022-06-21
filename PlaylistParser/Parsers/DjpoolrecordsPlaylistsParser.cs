using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PlaylistParser.DataStructs;

namespace PlaylistParser.Parsers;
/// <summary>
///     Model is realize parsing playlists from djpoolrecords.com website.
/// </summary>
public class DjpoolrecordsPlaylistsParser : IPLaylistParser
{
    /// <summary>
    ///     Method choose way for parsing single playlist page or page with category of playlists. Returns current Playlists or null.
    /// </summary>
    public Playlist[]? ParsePlaylists(string uri)
    {
        Playlist[]? playlists = null;
        if (uri.Contains("/category/")) playlists = GetPlaylistsFromCatPage(uri);
        else
        {
            Playlist? playlist = GetPlaylistFromItsPage(uri);
            if (playlist == null) return null;

            playlists = new Playlist[1] { playlist };
        }

        return playlists;
    }

    /// <summary>
    ///     Method parse playlist page by url and returns current Playlist or null.
    /// </summary>
    public Playlist? GetPlaylistFromItsPage(string uri)
    {
        HtmlNode? post;

        try
        {
            HtmlDocument htmlDoc = new HtmlWeb().Load(uri);
            post = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'single_post')]").FirstOrDefault();
        }
        catch { return null; }

        if (post == null) return null;

        var publishDate = ParsePostDate(post);

        var name = post.ChildNodes.Where(n => n.Name == "header").First()
            .ChildNodes.Where(n => n.Name == "h1").First().InnerText;

        Playlist playlist = new(name, publishDate, new List<Song>());

        SetSongs(post, playlist);

        return playlist;
    }
    /// <summary>
    ///     Method parse category of playlists page by url and returns current Playlists or null.
    /// </summary>
    public Playlist[]? GetPlaylistsFromCatPage(string uri)
    {
        HtmlNodeCollection posts;

        try
        {
            HtmlDocument htmlDoc = new HtmlWeb().Load(uri);
            posts = htmlDoc.DocumentNode.SelectNodes("//article[contains(@class, 'post excerpt')]");
        }
        catch { return null; }

        if (posts == null || posts.Count == 0) return null;

        var playlistsOut = new Playlist[posts.Count];

        Task[] tasks = new Task[posts.Count];

        for (int i = 0; i < posts.Count; i++)
        {
            tasks[i] = Task.Factory.StartNew((index) =>
            {
                var k = (int)index;
                var publishDate = ParsePostDate(posts[k]);

                var name = posts[k].ChildNodes.Where(n => n.Name == "header").First()
                    .ChildNodes.Where(n => n.Name == "h2").First()
                    .ChildNodes.Where(n => n.Name == "a").First()
                    .InnerText;

                playlistsOut[k] = new Playlist(name, publishDate, new List<Song>());

                SetSongs(posts[k], playlistsOut[k]);
            }, i);
        }

        Task.WaitAll(tasks);

        return playlistsOut;
    }
    //-------------------------------------------------------
    /// <summary>
    ///     Method parse and set songs for input playlist.
    /// </summary>
    public void SetSongs(HtmlNode post, Playlist playlist)
    {
        var postContent = post.ChildNodes.Where(n =>
                    n.GetClasses().Contains("post-single-content")
                    || n.GetClasses().Contains("post-content")
                    || n.Id == "content"
                ).First();

        foreach (HtmlNode p in postContent.ChildNodes.Where(n => n.Name == "p"))
        {
            var strongNodes = p.ChildNodes.Where(n => n.Name == "strong");

            foreach (var strongNode in strongNodes)
            {
                string textLine = strongNode.InnerText;

                if (textLine == "") continue;

                textLine = HtmlEntity.DeEntitize(textLine);

                char sep = '–';
                textLine.Replace('-', sep);

                if (textLine.Contains(sep))
                {
                    textLine = RemoveTrackSize(textLine);
                    textLine = RemoveBpmNum(textLine);

                    string[] splittedLine = textLine.Split(sep);
                    int filler;

                    if (!int.TryParse(splittedLine[0].Trim(), out filler) || splittedLine.Length > 2)
                    {
                        textLine = RemoveNumerate(textLine);
                        splittedLine = textLine.Split(sep);
                    }

                    string artistName = splittedLine[0].Trim();
                    string songName = RemoveNumerate(textLine.Substring(splittedLine[0].Length + 1));
                    songName = songName.Replace(artistName + "–", string.Empty).Trim();
                    songName = songName.Replace(artistName + " –", string.Empty).Trim();

                    playlist.Songs.Add(new Song(artistName, songName));
                }
            }
        }
    }
    //-------------------------------------------------------
    /// <summary>
    ///     Method parse date of post and retuns it.
    /// </summary>
    public DateOnly ParsePostDate(HtmlNode post)
    {
        var dateNode = post.ChildNodes.Where(n => n.GetClasses().Contains("post-date-ribbon")).First();

        return DateOnly.Parse(dateNode.InnerText);
    }
    //-------------------------------------------------------
    /// <summary>
    ///     Method removes track size from song name. Samle - "(123.01MB)".
    /// </summary>
    public string RemoveTrackSize(string line)
    {
        var match_coll = new Regex(@"(\s*\([0-9]+[.]?[0-9]*\s*\wB\))$").Matches(line);

        return match_coll.Count == 0 ? line : line.Replace(match_coll.First().Groups[1].Value, string.Empty);
    }
    /// <summary>
    ///     Method removes num of Bpm from song name. Samle - "(123 Bpm)".
    /// </summary>
    public string RemoveBpmNum(string line)
    {
        var match_coll = new Regex(@"(\s*\([0-9]+[.]?[0-9]*\s*Bpm\))$").Matches(line);

        return match_coll.Count == 0 ? line : line.Replace(match_coll.First().Groups[1].Value, string.Empty);
    }
    /// <summary>
    ///     Method removes numeration of the track from line. Sample - "01.".
    /// </summary>
    public string RemoveNumerate(string line)
    {
        var match_coll = new Regex(@"(^\(?[0-9]+\)?\s?\.?\s*-*–?-*\s*)").Matches(line.Trim());

        return match_coll.Count == 0 ? line : line.Replace(match_coll.First().Groups[1].Value, string.Empty);
    }
}
