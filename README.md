doubanFMAPI
===========

豆瓣电台API和demo

### 豆瓣FM API
> 获取API对象(单例模式)
> > API api = API.GetInstance()

> 登录相关
> > 获取/刷新登录页验证码
> > > string captchaString = api.UpdateCaptcha()
> 
> > 用户登录
> > > api.Login(string userName, string password, string captcha, bool remember = true)
>  
> > 判断用户是否已登录 //返回值说明：-1：网络连接出错 0：未登录 1：已登录
> > > int loginState = api.HasLogined()
> 
> > 获取登录用户信息
> > > api.GetLoginedUserInfo()
> 
> > 注销登录
> > > api.Logout()
> 
> > 登录完成事件
> > > api.loginCompletedEvent += loginInfo => {}
> 
> 歌曲相关
> > 获取新的播放列表 //适用于新打开软件或者切换兆赫时
> > > api.GetNewPlayList(int channelID, string songID = "", double SongPT = 0.0)
> 
> > 为歌曲标注红心
> > > api.Like(int channelID, string songID, double songPT)
> 
> > 取消已经标注的红心
> > > api.UnLike(int channelID, string songID, double songPT)
> 
> > 不再播放当前歌曲
> > > api.Ban(int channelID, string songID, double songPT)
> 
> > 跳过当前歌曲
> > > api.Next(int channelID, string songID, double songPT)
> 
> > 歌曲自然结束
> > > api.NaturalEnd(int channelID, string songID, double songPT)
> 
> > 播放列表更改事件
> > > api.playListChangedEvent += newPlaylist => {}
> 
> > 播放列表加载失败事件
> > > api.playListLoadFailedEvent += () => {}
> 
> > 歌曲切换事件
> > > api.currentSongChangedEvent += newSong => {}
> 
> > 播放列表为空事件
> > > api.playListEmptyedEvent += () => {}
> 
> 兆赫相关
> > 获取初始兆赫
> > > ChannelEntity channels = api.GetInitChannel(bool logined)
> 
> > 获取热门兆赫
> > > api.GetHotChannels(int start, int limit)
> 
> > 获取上升最快兆赫列表
> > > api.GetUpTrendingChannels(int start, int limit)
> 
> > 搜索兆赫
> > > api.GetSearchChannels(string keyword, int start, int limit)
> 
> > 获取用户收藏的兆赫列表
> > > api.GetFavoriateChannels(string userName)
> 
> > 判断某兆赫是否在收藏列表中
> > > bool isFav = api.IsFavorite(string userName, int channelID)
> 
> > 添加兆赫到收藏列表
> > > api.AddToFavoriateChannels(string userName, ChannelEntity channel)
> 
> > 从收藏列表中移除兆赫
> > > api.RemoveFromFavoriateChannels(string userName, int channelID)
> 
> > 兆赫变更事件
> > > api.channelChangedEvent += channelInfo {}
> 
> > 兆赫加载失败事件
> > > api.channelLoadFailedEvent += () {}

    更多API操作见源码API.cs
    目前豆瓣FM新增在线收藏列表，而此API中仍是本地存储，因此部分内容请先自行修改！
    demo bug反馈justplus AT ahu.edu.cn
