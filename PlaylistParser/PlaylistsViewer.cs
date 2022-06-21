using PlaylistParser.DataStructs;
using System.ComponentModel;
using System.Linq;

namespace PlaylistParser;
/// <summary>
///     This class is for manipulations with controls of MainWindow.
/// </summary>
internal class PlaylistsViewer
{
    private MainWindow _mainWindow;
    private Playlist? currPlaylist;

    public PlaylistsViewer(MainWindow _mainWindow) { this._mainWindow = _mainWindow; }
    public void UpdatePlaylistSongs(string playlistName)
    {
        currPlaylist = _mainWindow.Playlists.Where(p => p.Name == playlistName).FirstOrDefault();
        _mainWindow.songs_dataGrid.ItemsSource = currPlaylist == null ? null : new BindingList<Song>(currPlaylist.Songs);
    }
    public void UpdatePlaylistsList()
        => _mainWindow.playlists_listBox.ItemsSource = _mainWindow.Playlists == null ? null : new BindingList<Playlist>(_mainWindow.Playlists);

    public void UpdatePublishDateLabel()
        => _mainWindow.publishDate_label.Content = currPlaylist == null ? "" : currPlaylist.PublishDate.ToString();
    
    public void SetParserNames(PlaylistsParser _playlistsParser)
        => _mainWindow.parserNames_comboBox.ItemsSource = new BindingList<string>(_playlistsParser.parsers.Keys.ToArray());

    public void PrepareUpdating()
    {
        _mainWindow.Playlists = null;
        currPlaylist = null;
        _mainWindow.spinner_label.Visibility = System.Windows.Visibility.Visible;
        _mainWindow.playlists_listBox.ItemsSource = new BindingList<Playlist>();
        _mainWindow.songs_dataGrid.ItemsSource = new BindingList<Song>();
        _mainWindow.publishDate_label.Content = "";
        LockLoading();
    }
    public void Awake()
    {
        UnLockLoading();
        _mainWindow.playlists_listBox.IsEnabled = true;
        _mainWindow.spinner_label.Visibility = System.Windows.Visibility.Hidden;
    }
    public void LockLoading() 
    {
        _mainWindow.load_button.IsEnabled = false;
        _mainWindow.url_textBox.IsEnabled = false;
    }
    public void UnLockLoading() 
    {
        _mainWindow.load_button.IsEnabled = true;
        _mainWindow.url_textBox.IsEnabled = true;
    }
}
