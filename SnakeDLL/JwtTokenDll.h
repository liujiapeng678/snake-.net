// JwtTokenDll.h - JWT Token 生成 DLL 的头文件
#pragma once

#include "framework.h"

// 错误代码定义
#define JWT_SUCCESS 0
#define JWT_ERROR_INVALID_PARAMS -1
#define JWT_ERROR_BUFFER_TOO_SMALL -2
#define JWT_ERROR_GENERATION_FAILED -3
#define JWT_ERROR_UNKNOWN -4

// 导出函数声明
extern "C" {
    // 创建 JWT Token
    // 参数:
    //   username: 用户名 (输入)
    //   output: 输出缓冲区 (输出)
    //   outputSize: 输出缓冲区大小 (输入)
    // 返回值:
    //   JWT_SUCCESS: 成功
    //   JWT_ERROR_*: 各种错误码
    __declspec(dllexport) int CreateJwtToken(const char* username, char* output, int outputSize);

    // 获取错误描述
    // 参数:
    //   errorCode: 错误码
    // 返回值:
    //   错误描述字符串
    __declspec(dllexport) const char* GetErrorDescription(int errorCode);
}