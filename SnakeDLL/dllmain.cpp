// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        // DLL 被加载到进程时调用
        // 可以在这里进行初始化工作
        break;
    case DLL_THREAD_ATTACH:
        // 新线程创建时调用
        break;
    case DLL_THREAD_DETACH:
        // 线程退出时调用
        break;
    case DLL_PROCESS_DETACH:
        // DLL 从进程中卸载时调用
        // 可以在这里进行清理工作
        break;
    }
    return TRUE;
}