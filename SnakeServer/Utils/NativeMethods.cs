using System;
using System.Runtime.InteropServices;
using System.Text;

public static class JwtTokenGenerator
{
    // 导入 C++ DLL 中的函数
    [DllImport("SnakeDll.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int CreateJwtToken(
        [MarshalAs(UnmanagedType.LPStr)] string username,
        [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
        int outputSize);
    public static string CreateToken(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("用户名不能为空", nameof(username));
        }

        // 创建足够大的缓冲区来存储 Token
        const int bufferSize = 2048;
        var buffer = new StringBuilder(bufferSize);

        // 调用 C++ DLL 函数
        int result = CreateJwtToken(username, buffer, bufferSize);

        // 检查返回值
        switch (result)
        {
            case 0:
                return buffer.ToString();
            case -1:
                throw new ArgumentException("参数错误");
            case -2:
                throw new InvalidOperationException("缓冲区太小，无法存储 Token");
            case -3:
                throw new InvalidOperationException("Token 生成过程中发生未知错误");
            default:
                throw new InvalidOperationException($"未知的错误代码: {result}");
        }
    }
}