using Fluxor;
using SnakeClient.Services;
using System.Net.WebSockets;
using SnakeClient.GameClass;

namespace SnakeClient.Store
{
    [FeatureState]
    public class PkState
    {
        public string Status { get; set; } = "开始匹配";
        public IWebSocketService? Socket { get; set; } = null;
        public string Me { get; set; } = "";
        public string OpponentName { get; set; } = "我的对手";
        public string OpponentPhoto { get; set; } = "https://cdn.acwing.com/media/article/image/2022/08/09/1_1db2488f17-anonymous.png";
        public int[,]? Map { get; set; }
        public int AId { get; set; } = 0;
        public int ASx { get; set; } = 0;
        public int ASy { get; set; } = 0;
        public int BId { get; set; } = 0;
        public int BSx { get; set; } = 0;
        public int BSy { get; set; } = 0;
        public GameMap? Game { get; set; } = null;
        public string Loser { get; set; } = "none";
        public int SelectedBot { get; set; }
        public int SelectedBotRating { get; set; }
        public bool GoToPlayground { get; set; } = false;

        // 默认构造函数 - 初始状态
        public PkState()
        {
            Status = "开始匹配";
            Socket = null;
            Me = "";
            OpponentName = "我的对手";
            OpponentPhoto = "https://cdn.acwing.com/media/article/image/2022/08/09/1_1db2488f17-anonymous.png";
            Map = null;
            AId = 0;
            ASx = 0;
            ASy = 0;
            BId = 0;
            BSx = 0;
            BSy = 0;
            Game = null;
            Loser = "none";
            SelectedBot = -1;
            SelectedBotRating = 3500;
            GoToPlayground = false;
        }

        // 用于更新状态的构造函数
        public PkState(
            string status,
            IWebSocketService? socket,
            string me,
            string opponentName,
            string opponentPhoto,
            int[,]? map,
            int aId,
            int aSx,
            int aSy,
            int bId,
            int bSx,
            int bSy,
            GameMap game,
            string loser,
            int selectedBot,
            int selectedBotRating,
            bool goToPlayground
            )
        {
            Status = status;
            Socket = socket;
            Me = me;
            OpponentName = opponentName;
            OpponentPhoto = opponentPhoto;
            Map = map;
            AId = aId;
            ASx = aSx;
            ASy = aSy;
            BId = bId;
            BSx = bSx;
            BSy = bSy;
            Game = game;
            Loser = loser;
            SelectedBot = selectedBot;
            SelectedBotRating = selectedBotRating;
            GoToPlayground = goToPlayground;
        }
    }
}