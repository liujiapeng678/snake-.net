// JwtTokenDll.h - JWT Token ���� DLL ��ͷ�ļ�
#pragma once

#include "framework.h"

// ������붨��
#define JWT_SUCCESS 0
#define JWT_ERROR_INVALID_PARAMS -1
#define JWT_ERROR_BUFFER_TOO_SMALL -2
#define JWT_ERROR_GENERATION_FAILED -3
#define JWT_ERROR_UNKNOWN -4

// ������������
extern "C" {
    // ���� JWT Token
    // ����:
    //   username: �û��� (����)
    //   output: ��������� (���)
    //   outputSize: �����������С (����)
    // ����ֵ:
    //   JWT_SUCCESS: �ɹ�
    //   JWT_ERROR_*: ���ִ�����
    __declspec(dllexport) int CreateJwtToken(const char* username, char* output, int outputSize);

    // ��ȡ��������
    // ����:
    //   errorCode: ������
    // ����ֵ:
    //   ���������ַ���
    __declspec(dllexport) const char* GetErrorDescription(int errorCode);
}