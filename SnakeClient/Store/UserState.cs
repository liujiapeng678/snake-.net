

using Fluxor;

namespace SnakeClient.Store
{
    [FeatureState]
    public class UserState
    {
        public string Id { get; }
        public string Username { get; }
        public string Photo { get; }
        public string Token { get; }
        public bool IsLogin { get; }

        // 默认构造函数 - 初始状态
        public UserState()
        {
            Id = "";
            Username = "";
            Photo = "";
            Token = "";
            IsLogin = false;
        }

        // 用于更新状态的构造函数
        public UserState(string id, string username, string photo, string token, bool isLogin)
        {
            Id = id;
            Username = username;
            Photo = photo;
            Token = token;
            IsLogin = isLogin;
        }
    }
}