using System;


public class GlobalUtil
{
    #region RoleApTime
    public static int DefaultServerId = 1;//默认的服务器id
    public const int ROLE_AP_MAX = 120;//体力最大值
    public const int ROLE_AP_TIME = 5 * 60;//体力回复时间
    #endregion
    #region 交易行时间数据
    public static long TradeBuyRefreshTime = 30 * 60;//购买自动刷新时间（秒）
    public static long TradeSellRefreshTime = 30 * 60;//出售自动刷新时间（秒）
    public static long TradeSelledRefreshTime = 30 * 60;//已出售自动刷新时间（秒）
    public static int TradeBuyHandReTime = 2 * 60;//购买界面手动刷新时间（秒）
    public static int TradeSellHandTime = 30;//出售手动刷新时间（秒）
    public static int TradeSelledHandTime = 30;//已出售手动刷新时间（秒）
    public static int TradeSellUpTime = 72 * 60 * 60;//商品出售上架时间（秒）
    public static int TradeBuyMaxPerPage = 20;//购买界面每页最大信息条数（秒）
    public static int TradeBuyMaxPage = 10;//每次请求数据最大页数（秒）
    #endregion
    #region 武道会数据
    public static int BudoRecoverTime = 60 * 60;//武道会挑战次数恢复时间
    #endregion
    #region 任务系统数据
    public static int RewardTaskRefreshTime = 30 * 60;//悬赏任务刷新时间
    #endregion
    #region 排行榜系统数据
    public static int RankRefreshTime = 30 * 60;//排行榜数据刷新间隔时间
    #endregion
    #region 好友
    public static string AddSuccesFriend = "";//一键添加成功的好友ID
    /// <summary>
    /// 显示推荐好友开关 0,3为不需要，1为需要显示推送图标，2为已进入推荐好友界面
    /// </summary>
    public static int RecomFriend = 0;
    public const int InviteRefTime = 30;
    /// <summary>
    /// 好友数据刷新时间
    /// </summary>
    public const int FriendRefTime = 120;
    /// <summary>
    /// 切磋记录刷新时间
    /// </summary>
    public const int RecordRefTime = 120;
    #endregion
    #region 盟会
    /// <summary>
    /// 盟会创建需消耗（元宝）
    /// </summary>
    public const int GuildCreateNeed = 250;
    /// <summary>
    /// 盟会敌对数量上限
    /// </summary>
    public const int GuildEnemyMax = 5;
    /// <summary>
    /// 盟会副会长数量上限
    /// </summary>
    public const int GuildViceLeader = 3;
    /// <summary>
    /// 盟会申请解散时间（7天）
    /// </summary>
    public const int Guilddissolvetime = 604800;
    /// <summary>
    /// 申请盟会数量上限
    /// </summary>
    public const int GuildAppMaxNum = 4;
    /// <summary>
    /// 盟会申请状态时间（7天）
    /// </summary>
    public const int Guildapplytime = 604800;
    #endregion
    #region 英雄成长最大值
    public const float AtkUpMax = 82.8f;
    public const float HpUpmax = 517.5f;
    public const float PhyDefUpMax = 55.2f;
    public const float MagDefUpMax = 55.2f;
    #endregion
    #region Dreamland
    public const int DL_OPEN_TIME = 120; //开启CD时间
    public const int DL_CLOSE_TIME = 600; //关闭CD时间
    public const int DL_COOLING_TIME = 300;// 冷却CD时间
    #endregion

    #region Scene
    public const string MAIN_SCENE = "main";
    public const string MAP_SCENE = "worldmap";
    public const string CHAPTER_SCENE = "chapter";
    public const string GAME_SCENE = "game";
    #endregion

    #region Notify
    /// <summary>
    /// 服务器返回码消息
    /// </summary>
    public const string SERVERRET = "ServerRet";
    /// <summary>
    /// 逻辑层操作错误提示
    /// </summary>
    public const string LUOJIERROR = "luojierror";
    /// <summary>
    /// 逻辑层操作成功提示
    /// </summary>
    public const string LUOJISUCCESS = "luojisuccess";
    /// <summary>
    /// 删除3D模型
    /// </summary>
    public const string DESTORYMODEL = "DestroyInstance";
    /// <summary>
    /// 阵容心法组件删除
    /// </summary>
    public const string HERODHARMADELE = "HerodharmaDele";
    /// <summary>
    /// 阵容心法互换
    /// </summary>
    public const string HERODHARMASWAP = "herodharmaswap";
    /// <summary>
    /// CDkey输入错误
    /// </summary>
    public const string CDKEY_INPUT_ERROR = "CDkeyInputError";
    /// <summary>
    /// 元宝不足
    /// </summary>
    public const string GOLD_NOT_ENOUGH = "GoldNotEnough";
    /// <summary>
    /// 金币不足
    /// </summary>
    public const string COIN_NOT_ENOUGH = "CoinNotEnough";
    /// <summary>
    /// 装备装换不匹配
    /// </summary>
    public const string CONVERT_ERROR = "ConvertError";
    /// <summary>
    /// 选择宝石
    /// </summary>
    public const string CHOOSE_DIAMOND = "ChooseDiamond";
    /// <summary>
    /// 摘除宝石
    /// </summary>
    public const string REMOVE_DIAMOND = "RemoveDiamond";
    /// <summary>
    /// 选择装备
    /// </summary>
    public const string CHOOSE_EQUIP = "ChooseEquip";
    /// <summary>
    /// 确定装备传承
    /// </summary>
    public const string SURE_EQUIP_INHERIT = "SureEquipInherit";
    /// <summary>
    /// 确定装备传承弹窗
    /// </summary>
    public const string SURE_EQUIP_INHERIT_POP = "SureEquipInheritPop";
    /// <summary>
    /// 打开选择宝石界面通知
    /// </summary>
    public const string OPEN_CHOOSE_DIAMONT = "OpenChooseDiamont";
    /// <summary>
    /// 列表界面点击英雄下阵或心法下槽状态
    /// </summary>
    public const string DELEHERODHARMASTATE = "deleherodharmastate";
    /// <summary>
    /// 列表界面点击英雄上阵或心法上槽状态
    /// </summary>
    public const string ADDHERODHARMASTATE = "addherodharmastate";
    /// <summary>
    /// 选择使用的初级经验丹变化
    /// </summary>
    public const string CHANGEPRIMARYEXPDAN = "changeprimaryexpdan";
    /// <summary>
    /// 选择使用的中级经验丹变化
    /// </summary>
    public const string CHANGEMIDDLEEXPDAN = "changemiddleexpdan";
    /// <summary>
    /// 选择使用的高级经验丹变化
    /// </summary>
    public const string CHANGEHIGHEXPDAN = "changehighexpdan";
    /// <summary>
    /// 强化时强化石不够
    /// </summary>
    public const string GOOD_NO_ENOUGH = "GoodNoEnough";
    /// <summary>
    /// 合成结果消息回调
    /// </summary>
    public const string COMPOSE_DIAMOND_RESULT = "ComposeDiamondResult";
    /// <summary>
    /// Role信息更新
    /// </summary>
    public const string ROLESVRDATAUPDATE = "RoleSvrDataUpdate";
    /// <summary>
    /// 英雄数据更新
    /// </summary>
    public const string PLAYERSVRDATA_UPDATE = "PlayerSvrDataUpdate";
    /// <summary>
    /// 阵容战力变化
    /// </summary>
    public const string TEAM_POWER_CHANGE = "Team_Power_Change";
    /// <summary>
    /// 装备数据更新
    /// </summary>
    public const string EQUIPSCRDATE_IPDATE = "EquipSvrDataUpdate";

    public const string ROLE_UPGRADE = "Role_Upgrade";
    /// <summary>
    /// 物品信息更新
    /// </summary>
    public const string GOODSSVRDATAUPDATE = "GoodsSvrDataUpdate";
    /// <summary>
    /// 宝箱信息更新
    /// </summary>
    public const string TREASUREDATAUPDATE = "TreasureDataUpdate";
    /// <summary>
    /// 阵容信息更新
    /// </summary>
    public const string TEAMSVRDATAUPDATE = "TeamSvrDataUpdate";
    /// <summary>
    /// 数据库数据删除
    /// </summary>
    public const string ON_DELETE_REFRSH = "On_Delete_Refrsh";
    ///
    public const string On_ADD_REFRESH = "On_Add_Refrsh";
    /// <summary>
    /// 播放window动画,打开popupwindow的boxCollider2D,防止点击其他
    /// </summary>
    public const string PLAY_WINDOW_ANIMATION = "Play_Window_Animation";
    /// <summary>
    /// 结束window动画,关闭popupwindow的boxCollider2D
    /// </summary>
    public const string END_PLAY_WINDOW_ANIMATION = "End_Play_Window_Animation";

    public const string CHECK_EQUIP_ACTION = "Check_Equip_Action";

    public const string TASK_INFO_UPDATE = "Task_Info_Update";
    /// <summary>
    /// 本地时间更新
    /// </summary>
    public const string LOCAL_TIME_CHANGE = "Local_Time_Change";

    public const string ACTIVE_REWARD_UPDATE = "Active_Reward_Update";
    /// <summary>
    /// 个人的盟会信息更新
    /// </summary>
    public const string MY_GUILD_UPDATE = "My_Guild_Update";
    /// <summary>
    /// 个人的盟会信息添加
    /// </summary>
    public const string MY_GUILD_ADD = "My_Guild_Add";
    /// <summary>
    /// 个人的盟会职位更新
    /// </summary>
    public const string MY_GUILD_POSCHANGE = "My_Guild_Poschange";
    /// <summary>
    /// 个人盟会科技等级变化
    /// </summary>
    public const string MY_GUILDTEC_UPDATE = "My_GuildTec_Update";
    #endregion

    #region Static
    public static DateTime Mall_Refresh_Time;
    #endregion

}

