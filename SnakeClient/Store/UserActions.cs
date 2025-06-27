namespace SnakeClient.Store
{

    // 用户登录动作
    public class LoginAction
    {
        public string Username { get; }
        public string Password { get; }
        public Action Success { get; }
        public Action Error { get; }

        public LoginAction(string username, string password, Action success, Action error)
        {
            Username = username;
            Password = password;
            Success = success;
            Error = error;
        }
    }


    // 更新用户信息动作
    public class UpdateUserAction
    {
        public string Id { get; }
        public string Username { get; }
        public string Photo { get; }
        public string Token { get; }
        public bool IsLogin { get; }

        public UpdateUserAction(string id, string username, string photo, string token, bool isLogin)
        {
            Id = id;
            Username = username;
            Photo = photo;
            Token = token;
            IsLogin = isLogin;
        }
    }

    // 响应数据模型
    public class LoginResponse
    {
        public required string Id { get; set; }
        public required string Token { get; set; }
        public required string Photo { get; set; }
    }
}
