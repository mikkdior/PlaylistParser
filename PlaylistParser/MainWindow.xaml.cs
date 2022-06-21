using PlaylistParser.DataStructs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlaylistParser;
public partial class MainWindow : Window
{
    private event Action _Update;
    private PlaylistsParser _playlistsParser = new();
    private PlaylistsViewer _playlistsViewer;
    public Playlist[] Playlists { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        _playlistsViewer = new(this);
        _playlistsViewer.SetParserNames(_playlistsParser);
        _playlistsViewer.LockLoading();
        _Update += _playlistsViewer.UpdatePlaylistsList;
        _Update += _playlistsViewer.UpdatePublishDateLabel;
        _Update += _playlistsViewer.Awake;
    }
    private void load_button_Click(object sender, RoutedEventArgs e)
    {
        if (url_textBox.Text == "") return;

        var parserName = parserNames_comboBox.SelectedItem.ToString();

        if (!url_textBox.Text.Contains(parserName)) 
        {
            ErrorMessage();
            return;
        }

        UpdatePlaylists(url_textBox.Text, parserName);
    }
    public void UpdatePlaylists(string uri, string parserName)
    {
        _playlistsViewer.PrepareUpdating();

        Task.Factory.StartNew((prObj) =>
        {
            var pr = (ParseRequest)prObj;

            if (!Uri.IsWellFormedUriString(pr.Uri, UriKind.Absolute))
                Dispatcher.Invoke(ErrorMessage);
            else
            {
                Playlists = _playlistsParser.ParsePlaylists((ParseRequest)prObj);
                if (Playlists == null) ErrorMessage();
            }
            Dispatcher.Invoke(() => _Update?.Invoke());

        }, new ParseRequest(url_textBox.Text, parserName));
    }
    
    private void playlists_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (playlists_listBox.SelectedItem == default) return;

        _playlistsViewer.UpdatePlaylistSongs(((Playlist)playlists_listBox.SelectedItem).Name);
        _Update?.Invoke();
    }
    public void ErrorMessage() => MessageBox.Show("Page doesn`t contains playlists or Uri is not valid", "Load is failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Cancel);
    private void url_textBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            load_button_Click(sender as Button, e);
    }
    private void parserNames_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ((sender as ComboBox).SelectedItem == null) 
            _playlistsViewer.LockLoading();
        else _playlistsViewer.UnLockLoading();
    }
}
