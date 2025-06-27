using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace SnakeClient.Store
{
    public class UserEffects
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public UserEffects(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        [EffectMethod]
        public async Task HandleLoginAction(LoginAction action, IDispatcher dispatcher)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5081/api/user/login",
                new
                {
                    username = action.Username,
                    password = action.Password,
                    photo = ""
                });

                // 解析响应
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    string id = result.Id;
                    string token = result.Token; // 提取 Token
                    string photo = result.Photo; // 提取 Photo

                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "token", token);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "id", id);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "photo", photo);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "username", action.Username);

                    dispatcher.Dispatch(new UpdateUserAction(id, action.Username, photo, token, true));

                    action.Success?.Invoke();
                }
                else
                {
                    action.Error?.Invoke();
                }

            }
            catch
            {
                action.Error?.Invoke();
            }
        }
    }
}
