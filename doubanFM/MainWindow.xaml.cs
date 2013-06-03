using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using doubanFMAPI;

namespace doubanFM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //目前显示的对象
        private Canvas visiblePanel;
        //doubanFMAPI对象
        private API api;
        //当前歌曲
        private doubanFMAPI.Entities.Song.SongEntity currentSong = null;
        private doubanFMAPI.Entities.User.UserEntity currentUser = null;
        private doubanFMAPI.Entities.Channel.ChannelEntity currentChannel;
        //private int currentChannelID = 1;

        private enum FMType
        {
            NormFM,
            SingerFM,
            AlbumFM
        };

        private enum ChannelType
        {
            HotChannel,
            UpTrendingChannel,
            FavChannel,
            SearchChannel
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (sender,e) => EventCollection();
            visiblePanel = PlayerPanel;
            api = API.GetInstance();
        }

        public void EventCollection()
        {
            //加载背景图片
            string fileName = Properties.Settings.Default.BKImage;
            BitmapImage source = new BitmapImage();
            if (File.Exists(fileName))
            {
                source.BeginInit();
                source.UriSource = new Uri(fileName);
                source.EndInit();
                ImageBrush brush = new ImageBrush(source);
                brush.Stretch = Stretch.UniformToFill;
                brush.Opacity = 0.8;
                grid.Background = brush;
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);

            int isLogined = api.HasLogined();

            //登陆按钮触发后委托事件
            api.loginCompletedEvent += result =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (result == null)
                    {
                        ct_LoginErrorMessage.Text = "网络异常或者其他异常！";
                        isLogined = -1;
                    }
                    else if (result.R != 0)
                    {
                        isLogined = 0;
                        ct_LoginErrorMessage.Text = result.ErrorMessage;
                        AnycUpdateCaptcha();
                    }
                    else
                    {
                        ct_LoginImage.Source = new BitmapImage(new Uri("Images/LoginUser.png", UriKind.RelativeOrAbsolute));
                        if (visiblePanel.Equals(LoginPanel))
                            ChangeVisibility(UserInfoPanel);
                        ct_LoginedUserName.Content = result.User.Name;
                        ct_Played.Content = result.User.Record.Played.ToString();
                        ct_Like.Content = result.User.Record.Liked.ToString();
                        ct_Ban.Content = result.User.Record.Banned.ToString();
                        currentUser = result.User;
                        ct_MoreFM.Visibility = Visibility.Visible;
                        isLogined = 1;
                    }
                }));

            };

            if (isLogined == 1)
            {
                api.GetLoginedUserInfo();
            }

            
            FMType fmType = FMType.NormFM;

            #region 标题栏事件

            //点击最小化按钮--最小化窗口
            ct_MinusImage.MouseLeftButtonDown += (sender, e) =>
                {
                    this.WindowState = WindowState.Minimized;
                };

            //点击关闭按钮--关闭窗口
            ct_CloseImage.MouseLeftButtonDown += (sender, e) =>
            {
                this.Close();
            };

            //拖动标题栏--拖动窗口
            TitlePanel.MouseLeftButtonDown += (sender, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                        this.DragMove();
                };

            //点击装扮按钮--更改背景图片
            ct_ChangeBGImage.MouseLeftButtonDown += (sender, e) =>
                {
                    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                    dialog.Title = "选择背景图片";
                    dialog.Filter = "图片文件(*.BMP;*.JPG;*.JPEG)|*.BMP;*.JPG;*.JPEG";
                    dialog.Multiselect = false;
                    if (dialog.ShowDialog() == true)
                    {
                        //grid.Background = new ImageBrush(new BitmapImage(new Uri(dialog.FileName, UriKind.RelativeOrAbsolute)));
                        BitmapImage imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.UriSource = new Uri(dialog.FileName);
                        imageSource.EndInit();

                        ImageBrush ib = new ImageBrush(imageSource);
                        ib.Stretch = Stretch.UniformToFill;
                        ib.Opacity = 0.8;
                        grid.Background = ib;

                        Properties.Settings.Default.BKImage = dialog.FileName;
                        Properties.Settings.Default.Save();
                    }
                };

            //点击置顶按钮--切换总是置顶/不置顶状态
            ct_TopImage.MouseLeftButtonDown += (sender, e) =>
                {
                    //this.Topmost = true;
                    if (ct_TopImage.Tag == null || ct_TopImage.Tag.ToString() == "nottop")
                    {
                        this.Topmost = true;
                        ct_TopImage.Source = new BitmapImage(new Uri("images/NotAlwaysTop.png", UriKind.Relative));
                        ct_TopImage.Tag = "top";
                    }
                    else
                    {
                        this.Topmost = false;
                        ct_TopImage.Source = new BitmapImage(new Uri("images/AlwaysTop.png", UriKind.Relative));
                        ct_TopImage.Tag = "nottop";
                    }
                };

            //点击登陆图像--弹出登陆窗口
            ct_LoginImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (visiblePanel.Equals(LoginPanel) || visiblePanel.Equals(UserInfoPanel))
                    {
                        ChangeVisibility(PlayerPanel);
                    }
                    else
                    {
                        if (isLogined == 1)
                        {
                            api.GetLoginedUserInfo();
                            ChangeVisibility(UserInfoPanel);
                        }
                        else if (isLogined == -1)
                        {
                            ct_LoginErrorMessage.Text = "网络故障！";
                            ChangeVisibility(LoginPanel);
                        }
                        else
                        {
                            ct_LoginErrorMessage.Text = string.Empty;
                            ThreadPool.QueueUserWorkItem(new WaitCallback(
                                captchaUrl =>
                                {
                                    string _captchaUrl = api.UpdateCaptcha();
                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        ct_CaptchaImage.Source = new BitmapImage(new Uri(_captchaUrl, UriKind.Absolute));
                                    }));
                                }));
                            ChangeVisibility(LoginPanel);
                        }
                    }
                };
            #endregion

            #region 登陆窗口事件

            //点击验证码图片--更新验证码
            ct_CaptchaImage.MouseLeftButtonDown += (sender, e) =>
                {
                    AnycUpdateCaptcha();
                };
            
            //点击登陆按钮--登陆豆瓣账户
            ct_LoginButton.Click += (sender, e) =>
                {
                    if (api.HasLogined() == 1)
                    {
                        api.GetLoginedUserInfo();
                        return;
                    }
                    string userName = ct_UserName.Text.Trim();
                    string password = ct_Password.Password.Trim();
                    string captcha = ct_Captcha.Text.Trim();
                    if (string.IsNullOrEmpty(userName))
                    {
                        ct_LoginErrorMessage.Text = "用户名不能为空！";
                        ct_UserName.Focus();
                        return;
                    }
                    else if (string.IsNullOrEmpty(password))
                    {
                        ct_LoginErrorMessage.Text = "密码不能为空！";
                        ct_Password.Focus();
                        return;
                    }
                    else if (string.IsNullOrEmpty(captcha))
                    {
                        ct_LoginErrorMessage.Text = "验证码不能为空！";
                        ct_Captcha.Focus();
                        return;
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                        obj =>
                        {
                            api.Login(userName, password, captcha);
                        }));
                        ct_LoginErrorMessage.Text = "正在登陆...";
                    }
                };

            //点击取消按钮--取消登陆界面
            ct_CancelLoginButton.Click += (sender, e) =>
                {
                    ChangeVisibility(PlayerPanel);
                };

            //点击退出登陆按钮--退出豆瓣账户
            ct_LogoutButton.Click += (sender, e) =>
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                        obj =>
                        {
                            api.Logout();
                        }));
                    isLogined = 0;
                    currentUser = null;
                    ChangeVisibility(PlayerPanel);
                    ct_LoginImage.Source = new BitmapImage(new Uri("Images/UnloginUser.png", UriKind.RelativeOrAbsolute));
                    ct_MoreFM.Visibility = Visibility.Collapsed;
                    
                    currentChannel = api.GetInitChannel(false);
                    api.GetNewPlayList(currentChannel.ChannelID);
                    ct_Channel.Content = currentChannel.Name;
                };

            //点击取消按钮--返回播放界面
            ct_CancelLogoutButton.Click += (sender, e) =>
                {
                    ChangeVisibility(PlayerPanel);
                };

            #endregion

            #region 播放相关

            List<TimeSpan> timeSpanList = null;
            int lyricIndex = 0;
            currentChannel = api.GetInitChannel(isLogined == 1);
            SetFavChannel();
            ct_Channel.Content = currentChannel.Name;

            //绑定切换播放列表事件
            api.playListChangedEvent += playList =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    //playListBox.ItemsSource = (ObservableCollection<doubanFMAPI.Entities.Song.SongEntity>)playList;
                    //playListBox.UpdateLayout();
                    List<doubanFMAPI.Entities.Song.SongEntity> n_playList = playList.ToList();
                    playListBox.ItemsSource = n_playList;
                }));
            };

            //绑定切换歌曲事件
            api.currentSongChangedEvent += song =>
                {
                    timeSpanList = new List<TimeSpan>();
                    currentSong = song;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Storyboard sb = (Storyboard)Resources["HideSongInfo"];
                        sb.Begin();
                        sb = (Storyboard)Resources["ShowSongInfo"];
                        sb.Begin();
                        ct_SongName.Content = song.SongName;
                        ct_SingerName.Content = song.SingerName;
                        ct_PublishTime.Content = song.PublishTime;
                        ct_AlbumName.Content = song.AlbumName;
                        SetLike(song.IsLiked == "1" ? true : false);
                        ct_MediaPlayer.Source = new Uri(song.SongUrl, UriKind.Absolute);
                        ct_MediaPlayer.Play();
                    }));
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                        obj =>
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                ct_AlbumPicture.Source = new BitmapImage(new Uri(song.AlbumPicture.Replace("mpic","lpic"), UriKind.Absolute));
                            }));
                            api.GetLyric(ref song);
                            timeSpanList = SortTS((Dictionary<TimeSpan,string>)song.Lyrics);
                        }));
                };

            //绑定播放列表为空事件
            api.playListEmptyedEvent += () =>
                {
                    api.GetNewPlayList(currentChannel.ChannelID);
                };

            //定时器操作
            timer.Tick += (sender, e) =>
                {
                    while (!ct_MediaPlayer.NaturalDuration.HasTimeSpan)
                        return;
                    TimeSpan total = ct_MediaPlayer.NaturalDuration.TimeSpan;
                    TimeSpan hasPlayed = ct_MediaPlayer.Position;
                    string totalString = string.Format("{0:D2}:{1:D2}/{2:D2}:{3:D2}", hasPlayed.Minutes, hasPlayed.Seconds, total.Minutes, total.Seconds);
                    ct_SongTimeSpan.Content = totalString;
                    ct_SongLoadProgress.Value = ct_MediaPlayer.DownloadProgress;
                    ct_SongProgress.Value = hasPlayed.TotalSeconds / total.TotalSeconds;
                    ct_Lyric.Text = ShowCurrentLyric((Dictionary<TimeSpan, string>)currentSong.Lyrics, timeSpanList, hasPlayed, ref lyricIndex);
                };

            //一首歌开始播放
            ct_MediaPlayer.MediaOpened += (sender, e) =>
                {
                    timer.Start();
                    lyricIndex = 0;

                    CutString(ct_SongName.Content.ToString(), ref ct_SongName);
                    CutString(ct_SingerName.Content.ToString(), ref ct_SingerName);
                    CutString(ct_AlbumName.Content.ToString(), ref ct_AlbumName, true);
                };

            //一首歌曲自然结束
            ct_MediaPlayer.MediaEnded += (sender, e) =>
                {
                    timer.Stop();
                    if (fmType == FMType.NormFM)
                    {
                        TimeSpan ts = ct_MediaPlayer.Position;
                        api.NaturalEnd(currentChannel.ChannelID, currentSong.SongID, Math.Round(ts.TotalMilliseconds / 1000, 1));
                    }
                    else
                        api.NaturalEnd();
                };

            //点击音量按钮--改变音量
            ct_VolumeSlider.ValueChanged += (sender, e) =>
                {
                    ct_MediaPlayer.Volume = ct_VolumeSlider.Value;
                };


            //点击喜欢按钮--标记红心/取消标记红心
            ct_LikeImage.MouseLeftButtonDown += (sender, e) =>
                {
                    TimeSpan ts = ct_MediaPlayer.Position;
                    double songPT = Math.Round(ts.TotalMilliseconds / 1000, 1);
                    if (fmType == FMType.NormFM)
                    {

                        if (ct_LikeImage.Tag != null && ct_LikeImage.Tag.ToString() == "1")
                        {
                            SetLike(false);
                            ct_LikeImage.Tag = "0";
                            ThreadPool.QueueUserWorkItem(new WaitCallback(
                                obj =>
                                {
                                    api.UnLike(currentChannel.ChannelID, currentSong.SongID, songPT);
                                }));
                        }
                        else
                        {
                            SetLike(true);
                            ct_LikeImage.Tag = "1";
                            ThreadPool.QueueUserWorkItem(new WaitCallback(
                                obj =>
                                {
                                    api.Like(currentChannel.ChannelID, currentSong.SongID, songPT);
                                }));
                        }
                    }
                    else
                    {
                        if (ct_LikeImage.Tag != null && ct_LikeImage.Tag.ToString() == "1")
                        {
                            SetLike(false);
                            ct_LikeImage.Tag = "0";
                            ThreadPool.QueueUserWorkItem(new WaitCallback(
                                obj =>
                                {
                                    api.UnLike(currentSong.SongID, songPT);
                                }));
                        }
                        else
                        {
                            SetLike(true);
                            ct_LikeImage.Tag = "1";
                            ThreadPool.QueueUserWorkItem(new WaitCallback(
                                obj =>
                                {
                                    api.Like(currentSong.SongID, songPT);
                                }));
                        }
                    }
                };

            //点击不再播放按钮--不再播放
            ct_BanImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (fmType == FMType.NormFM)
                    {
                        TimeSpan ts = ct_MediaPlayer.Position;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.Ban(currentChannel.ChannelID, currentSong.SongID, Math.Round(ts.TotalMilliseconds / 1000, 1));
                            }));
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.Ban();
                            }));
                    }
                };

            //点击下一曲按钮--跳过该曲
            ct_NextImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (fmType == FMType.NormFM)
                    {
                        TimeSpan ts = ct_MediaPlayer.Position;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.Next(currentChannel.ChannelID, currentSong.SongID, Math.Round(ts.TotalMilliseconds / 1000, 1));
                            }));
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.Next();
                            }));
                    }
                };

            //点击播放歌手歌曲按钮--对登录用户，歌手兆赫
            ct_SingerFMImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (fmType == FMType.SingerFM)
                    {
                        fmType = FMType.NormFM;
                        ct_SingerFMImage.Source = new BitmapImage(new Uri("Images/SingerFM.png", UriKind.RelativeOrAbsolute));
                        ct_SingerFMImage.ToolTip = "收听该歌手歌曲";
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetNewPlayList(currentChannel.ChannelID);
                            }));
                    }
                    else
                    {
                        fmType = FMType.SingerFM;
                        ct_SingerFMImage.Source = new BitmapImage(new Uri("Images/DelSingerFM.png", UriKind.RelativeOrAbsolute));
                        ct_SingerFMImage.ToolTip = "取消收听该歌手歌曲";
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetSingerFMNewPlayList(currentSong.SingerName);
                            }));
                    }
                };

            //点击播放专辑歌曲按钮--对登录用户，专辑兆赫
            ct_AlbumFMImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (fmType == FMType.AlbumFM)
                    {
                        fmType = FMType.NormFM;
                        ct_AlbumFMImage.Source = new BitmapImage(new Uri("Images/AlbumFM.png", UriKind.RelativeOrAbsolute));
                        ct_AlbumFMImage.ToolTip = "收听该专辑歌曲";
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetNewPlayList(currentChannel.ChannelID);
                            }));
                    }
                    else
                    {
                        fmType = FMType.SingerFM;
                        ct_AlbumFMImage.Source = new BitmapImage(new Uri("Images/DelAlbumFM.png", UriKind.RelativeOrAbsolute));
                        ct_AlbumFMImage.ToolTip = "取消收听该专辑歌曲";
                        ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetAlbumFMNewPlayList(currentSong.AlbumName);
                            }));
                    }
                };

            //点击专辑封面--暂停播放
            ct_AlbumPicture.MouseLeftButtonDown += (sender, e) =>
                {
                    PlayerPanel.Opacity = 0.5;
                    PauseCanvas.Visibility = Visibility.Visible;
                    visiblePanel = PauseCanvas;
                    ct_LoginImage.IsEnabled = false;
                    ct_MediaPlayer.Pause();
                    timer.Stop();
                };

            //点击继续播放--继续播放
            PauseCanvas.MouseLeftButtonDown += (sender, e) =>
                {
                    //ChangeVisibility(PlayerPanel);
                    PlayerPanel.Opacity = 1.0;
                    PauseCanvas.Visibility = Visibility.Collapsed;
                    visiblePanel = PlayerPanel;
                    ct_LoginImage.IsEnabled = true;
                    ct_MediaPlayer.Play();
                    timer.Start();
                };

            //初始化
            api.GetNewPlayList(currentChannel.ChannelID);
            #endregion

            #region 播放列表相关

            //点击播放列表图标--显示/隐藏播放列表
            ct_PlaylistImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (visiblePanel.Equals(PlayListPanel))
                    {
                        ChangeVisibility(PlayerPanel);
                    }
                    else
                    {
                        ChangeVisibility(PlayListPanel);
                    }
                };

            #endregion

            #region 兆赫列表相关

            ChannelType channelType = ChannelType.HotChannel;
            int currentChPage = 1;
            int totalChPages = 2;
            int chPerPage = 10;
            string keyword = string.Empty;
            

            //绑定兆赫列表事件
            api.channelChangedEvent += channels =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        List<doubanFMAPI.Entities.Channel.ChannelEntity> n_channels = channels.Channels.ToList();
                        ct_channelListBox.ItemsSource = n_channels;
                        totalChPages = channels.ChannelCount;
                        if (channelType == ChannelType.FavChannel)
                            totalChPages = 1;
                        PageChanged(currentChPage, totalChPages);
                    }));
                };

            //点击兆赫图标--显示/隐藏兆赫列表
            ct_ChannelStackPanel.MouseLeftButtonDown += (sender, e) =>
                {
                    if (visiblePanel.Equals(ChannelPanel))
                    {
                        ChangeVisibility(PlayerPanel);
                    }
                    else
                    {
                        ChangeVisibility(ChannelPanel);
                    }
                };

            //点击热门图标按钮--显示热门兆赫
            ct_HotChPanel.MouseLeftButtonDown += (sender, e) =>
                {
                    currentChPage = 1;
                    channelType = ChannelType.HotChannel;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetHotChannels((currentChPage - 1) * 10, chPerPage);
                            }));
                    ct_chKeywordTextBox.Text = string.Empty;
                };

            //点击上升最快图标按钮--显示上升最快兆赫
            ct_UpTrendingChPanel.MouseLeftButtonDown += (sender, e) =>
                {
                    currentChPage = 1;
                    channelType = ChannelType.UpTrendingChannel;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetUpTrendingChannels((currentChPage - 1) * 10, chPerPage);
                            }));
                    ct_chKeywordTextBox.Text = string.Empty;
                };

            //点击收藏图标按钮--显示本地收藏兆赫
            ct_FavChPanel.MouseLeftButtonDown += (sender, e) =>
                {
                    currentChPage = 1;
                    channelType = ChannelType.FavChannel;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                if (currentUser == null || string.IsNullOrEmpty(currentUser.Name))
                                    api.GetFavoriateChannels("");
                                else
                                    api.GetFavoriateChannels(currentUser.Name);
                                totalChPages = 1;
                            }));
                    ct_chKeywordTextBox.Text = string.Empty;
                };

            //点击搜素图标按钮--显示搜索兆赫
            ct_SearchChPanel.MouseLeftButtonDown += (sender, e) =>
                {
                    keyword = ct_chKeywordTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(keyword))
                        return;
                    currentChPage = 1;
                    channelType = ChannelType.SearchChannel;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                api.GetSearchChannels(keyword, (currentChPage - 1) * 10, chPerPage);
                            }));
                };

            //点击上一页图标--对当前兆赫进行翻页
            ct_PrePageImage.MouseLeftButtonDown += (sender, e) =>
                {
                    currentChPage--;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                if (channelType == ChannelType.HotChannel)
                                    api.GetHotChannels((currentChPage - 1) * 10, chPerPage);
                                else if (channelType == ChannelType.UpTrendingChannel)
                                    api.GetUpTrendingChannels((currentChPage - 1) * 10, chPerPage);
                                else if (channelType == ChannelType.FavChannel)
                                    api.GetUpTrendingChannels((currentChPage - 1) * 10, chPerPage);
                                else
                                    api.GetSearchChannels(keyword, (currentChPage - 1) * 10, chPerPage);
                            }));
                };

            //点击下一页图标--对当前兆赫进行翻页
            ct_NextPageImage.MouseLeftButtonDown += (sender, e) =>
            {
                currentChPage++;
                ThreadPool.QueueUserWorkItem(new WaitCallback(
                            obj =>
                            {
                                if (channelType == ChannelType.HotChannel)
                                    api.GetHotChannels((currentChPage - 1) * 10, chPerPage);
                                else if (channelType == ChannelType.UpTrendingChannel)
                                    api.GetUpTrendingChannels((currentChPage - 1) * 10, chPerPage);
                                else if (channelType == ChannelType.FavChannel)
                                    api.GetUpTrendingChannels((currentChPage - 1) * 10, chPerPage);
                                else
                                    api.GetSearchChannels(keyword, (currentChPage - 1) * 10, chPerPage);
                            }));
            };

            //双击兆赫列表--切换当前兆赫
            ct_channelListBox.MouseDoubleClick += (sender, e) =>
                {
                    //ct_OpFavChannelImage.Visibility = Visibility.Visible;
                    List<doubanFMAPI.Entities.Channel.ChannelEntity> channels = (List<doubanFMAPI.Entities.Channel.ChannelEntity>)ct_channelListBox.ItemsSource;
                    currentChannel = channels[ct_channelListBox.SelectedIndex];
                    //currentChannelID = currentChannel.ChannelID;
                    ct_Channel.Content = currentChannel.Name;
                    TimeSpan ts = ct_MediaPlayer.Position;
                    double songPT = Math.Round(ts.TotalMilliseconds / 1000, 1);
                    if(currentSong != null)
                        api.GetNewPlayList(currentChannel.ChannelID, currentSong.SongID, songPT);
                    else
                        api.GetNewPlayList(currentChannel.ChannelID);
                    SetFavChannel();
                };

            //点击对当前兆赫进行收藏和取消收藏操作
            ct_OpFavChannelImage.MouseLeftButtonDown += (sender, e) =>
                {
                    if (ct_OpFavChannelImage.Tag == null || ct_OpFavChannelImage.Tag.ToString() == "UnFav")
                    {
                        ct_OpFavChannelImage.Tag = "Fav";
                        api.AddToFavoriateChannels(currentUser == null ? "" : currentUser.Name, currentChannel);
                        ct_OpFavChannelImage.Source = new BitmapImage(new Uri("Images/Favorited.png", UriKind.Relative));
                    }
                    else
                    {
                        ct_OpFavChannelImage.Tag= "UnFav";
                        api.RemoveFromFavoriateChannels(currentUser == null ? "" : currentUser.Name, currentChannel.ChannelID);
                        ct_OpFavChannelImage.Source = new BitmapImage(new Uri("Images/Favorite.png", UriKind.Relative));
                    }
                };

            #endregion
        }

        /// <summary>
        /// 切换显示界面
        /// </summary>
        /// <param name="visibleCanvas">要显示的面板对象</param>
        private void ChangeVisibility(Canvas visibleCanvas)
        {
            visiblePanel.Visibility = Visibility.Collapsed;
            visibleCanvas.Visibility = Visibility.Visible;
            visiblePanel = visibleCanvas;
        }

        /// <summary>
        /// 异步更新验证码
        /// </summary>
        private void AnycUpdateCaptcha()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                captchaUrl =>
                {
                    string _captchaUrl = api.UpdateCaptcha();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ct_CaptchaImage.Source = new BitmapImage(new Uri(_captchaUrl, UriKind.Absolute));
                    }));
                }));  
        }

        /// <summary>
        /// 设置 标记红心/取消红心
        /// </summary>
        /// <param name="isLike"></param>
        private void SetLike(bool isLike)
        {
            ct_LikeImage.Source = new BitmapImage(new Uri(isLike ? "Images/Like.png" : "Images/UnLike.png", UriKind.RelativeOrAbsolute));
            ct_LikeImage.Tag = isLike ? "1" : "0";
        }

        /// <summary>
        /// 对当前TimeSpan进行排序
        /// </summary>
        /// <param name="lyrics">歌词字典</param>
        /// <returns>排序后的时间点</returns>
        private List<TimeSpan> SortTS(Dictionary<TimeSpan, string> lyrics)
        {
            if (lyrics != null && lyrics.Count > 0)
            {
                TimeSpan[] timeSpans = new TimeSpan[lyrics.Count];
                lyrics.Keys.CopyTo(timeSpans, 0);
                List<TimeSpan> sortedTimeSpan = new List<TimeSpan>(timeSpans);
                sortedTimeSpan.Sort();
                return sortedTimeSpan;
            }
            else
                return null;
        }

        /// <summary>
        /// 显示某一时间点的歌词
        /// </summary>
        /// <param name="lyrics">歌词集合</param>
        /// <param name="currentTime">时间点</param>
        /// <returns>歌词内容</returns>
        private string ShowCurrentLyric(Dictionary<TimeSpan, string> lyrics, List<TimeSpan> sortedTimeSpan, TimeSpan currentTime, ref int fromIndex)
        {
            string currentLyric = string.Empty;
            if (sortedTimeSpan != null && lyrics != null)
            {
                currentLyric = lyrics[sortedTimeSpan[fromIndex]];
                if (fromIndex < sortedTimeSpan.Count - 1)
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format("{0}-{1}-{2}", currentTime, sortedTimeSpan[fromIndex + 1], currentTime.CompareTo(sortedTimeSpan[fromIndex + 1])));
                    if (currentTime.CompareTo(sortedTimeSpan[fromIndex + 1]) >= 0)
                    {
                        currentLyric = lyrics[sortedTimeSpan[fromIndex + 1]];
                        fromIndex++;
                    }
                }
                else
                    currentLyric = string.Empty;
            }
            return currentLyric;
        }

        /// <summary>
        /// 对字符串进行裁剪
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="label">Label控件</param>
        /// <param name="isAlbumName">是否为专辑名称控件</param>
        private void CutString(string str, ref Label label, bool isAlbumName = false)
        {
            if(isAlbumName)
                label.Content = "<" + str + ">";
            else
                label.Content = str;
            if (label.MaxWidth - label.ActualWidth > 1)
                return;
            string tmp = str;
            for (int i = 0; i < str.Length; i++)
            {
                tmp = tmp.Substring(0, tmp.Length - 1);
                label.Content = tmp;
                label.UpdateLayout();
                if (label.MaxWidth - label.ActualWidth > 1)
                {
                    if(isAlbumName)
                        label.Content = "<" + tmp.Substring(0, tmp.Length - 2) + "...>";
                    else
                        label.Content = tmp.Substring(0, tmp.Length - 1) + "...";
                    break;
                }
            }
        }

        /// <summary>
        /// 翻页操作
        /// </summary>
        /// <param name="page">当前页</param>
        /// <param name="totalPages">总页数</param>
        private void PageChanged(int page, int totalPages)
        {
            if (page == 1)
            {
                ct_PrePageImage.IsEnabled = false;
                ct_PrePageImage.Opacity = 0.5;
            }
            else
            {
                ct_PrePageImage.IsEnabled = true;
                ct_PrePageImage.Opacity = 1.0;
            }
            if (page == totalPages)
            {
                ct_NextPageImage.IsEnabled = false;
                ct_NextPageImage.Opacity = 0.5;
            }
            else
            {
                ct_NextPageImage.IsEnabled = true;
                ct_NextPageImage.Opacity = 1.0;
            }
            ct_CurrentPage.Content = page;
        }

        /// <summary>
        /// 设置频道是否已收藏
        /// </summary>
        private void SetFavChannel()
        {
            if (currentUser == null)
            {
                if (api.IsFavorite("", currentChannel.ChannelID))
                {
                    ct_OpFavChannelImage.Source = new BitmapImage(new Uri("Images/Favorited.png", UriKind.Relative));
                    ct_OpFavChannelImage.Tag = "Fav";
                }
                else
                {
                    ct_OpFavChannelImage.Source = new BitmapImage(new Uri("Images/Favorite.png", UriKind.Relative));
                    ct_OpFavChannelImage.Tag = "UnFav";
                }
            }
            else
            {
                if (api.IsFavorite(currentUser.Name, currentChannel.ChannelID))
                {
                    ct_OpFavChannelImage.Source = new BitmapImage(new Uri("Images/Favorited.png", UriKind.Relative));
                    ct_OpFavChannelImage.Tag = "Fav";
                }
                else
                {
                    ct_OpFavChannelImage.Source = new BitmapImage(new Uri("Images/Favorite.png", UriKind.Relative));
                    ct_OpFavChannelImage.Tag = "UnFav";
                }
            }
        }
    }

}
