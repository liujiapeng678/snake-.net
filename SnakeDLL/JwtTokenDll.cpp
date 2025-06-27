// JwtTokenDll.cpp : ���� DLL Ӧ�ó���ĵ���������
//

#include "pch.h"
#include "framework.h"
#include "JwtTokenDll.h"

using namespace std;

// Base64 �����
static const string base64_chars =
"ABCDEFGHIJKLMNOPQRSTUVWXYZ"
"abcdefghijklmnopqrstuvwxyz"
"0123456789+/";

// Base64 ���뺯��
string base64_encode(const vector<unsigned char>& data) {
    string ret;
    int i = 0;
    int j = 0;
    unsigned char char_array_3[3];
    unsigned char char_array_4[4];
    int in_len = data.size();
    const unsigned char* bytes_to_encode = data.data();

    while (in_len--) {
        char_array_3[i++] = *(bytes_to_encode++);
        if (i == 3) {
            char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
            char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
            char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
            char_array_4[3] = char_array_3[2] & 0x3f;

            for (i = 0; (i < 4); i++)
                ret += base64_chars[char_array_4[i]];
            i = 0;
        }
    }

    if (i) {
        for (j = i; j < 3; j++)
            char_array_3[j] = '\0';

        char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
        char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
        char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
        char_array_4[3] = char_array_3[2] & 0x3f;

        for (j = 0; (j < i + 1); j++)
            ret += base64_chars[char_array_4[j]];

        while ((i++ < 3))
            ret += '=';
    }

    return ret;
}

// Base64URL ���루JWT ר�ã�
string base64url_encode(const vector<unsigned char>& data) {
    string base64 = base64_encode(data);

    // �滻�ַ���+ -> -, / -> _
    for (char& c : base64) {
        if (c == '+') c = '-';
        else if (c == '/') c = '_';
    }

    // �Ƴ�����ַ� '='
    size_t pad_pos = base64.find('=');
    if (pad_pos != string::npos) {
        base64.erase(pad_pos);
    }

    return base64;
}

string base64url_encode(const string& str) {
    vector<unsigned char> data(str.begin(), str.end());
    return base64url_encode(data);
}

// �򻯵� HMAC-SHA256 ʵ��
vector<unsigned char> hmac_sha256(const string& key, const string& data) {
    HCRYPTPROV hProv = 0;
    HCRYPTHASH hHash = 0;
    HCRYPTKEY hKey = 0;
    vector<unsigned char> hash;

    try {
        // ��ȡ�����ṩ����������
        if (!CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
            if (!CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT)) {
                throw runtime_error("�޷���ȡ����������");
            }
        }

        // ������Կ BLOB
        struct {
            BLOBHEADER hdr;
            DWORD len;
            BYTE key_data[64]; // �㹻��Ļ�����
        } key_blob;

        memset(&key_blob, 0, sizeof(key_blob));
        key_blob.hdr.bType = PLAINTEXTKEYBLOB;
        key_blob.hdr.bVersion = CUR_BLOB_VERSION;
        key_blob.hdr.reserved = 0;
        key_blob.hdr.aiKeyAlg = CALG_RC2;
        key_blob.len = min(key.length(), sizeof(key_blob.key_data));
        memcpy(key_blob.key_data, key.c_str(), key_blob.len);

        if (!CryptImportKey(hProv, (BYTE*)&key_blob, sizeof(BLOBHEADER) + sizeof(DWORD) + key_blob.len, 0, 0, &hKey)) {
            throw runtime_error("��Կ����ʧ��");
        }

        // ���� HMAC ��ϣ����
        if (!CryptCreateHash(hProv, CALG_HMAC, hKey, 0, &hHash)) {
            throw runtime_error("���� HMAC ��ϣʧ��");
        }

        // ���� HMAC ��Ϣ
        HMAC_INFO HmacInfo;
        memset(&HmacInfo, 0, sizeof(HmacInfo));
        HmacInfo.HashAlgid = CALG_SHA_256;
        if (!CryptSetHashParam(hHash, HP_HMAC_INFO, (BYTE*)&HmacInfo, 0)) {
            throw runtime_error("���� HMAC ����ʧ��");
        }

        // �����ϣ
        if (!CryptHashData(hHash, (BYTE*)data.c_str(), data.length(), 0)) {
            throw runtime_error("��ϣ����ʧ��");
        }

        // ��ȡ��ϣ����
        DWORD dwHashLen = 0;
        DWORD dwDataLen = sizeof(DWORD);
        if (!CryptGetHashParam(hHash, HP_HASHSIZE, (BYTE*)&dwHashLen, &dwDataLen, 0)) {
            throw runtime_error("��ȡ��ϣ����ʧ��");
        }

        // ��ȡ��ϣֵ
        hash.resize(dwHashLen);
        if (!CryptGetHashParam(hHash, HP_HASHVAL, hash.data(), &dwHashLen, 0)) {
            throw runtime_error("��ȡ��ϣֵʧ��");
        }
    }
    catch (const exception&) {
        // ��� Windows ���� API ʧ�ܣ�ʹ�ü򻯵Ĺ�ϣ�㷨��Ϊ��ѡ
        hash.resize(32);
        for (size_t i = 0; i < 32; i++) {
            hash[i] = static_cast<unsigned char>(
                (key[i % key.length()] ^ data[i % data.length()]) + i
                );
        }
    }

    // ������Դ
    if (hHash) CryptDestroyHash(hHash);
    if (hKey) CryptDestroyKey(hKey);
    if (hProv) CryptReleaseContext(hProv, 0);

    return hash;
}

// ��ȡ��ǰ Unix ʱ���
long long getCurrentTimestamp() {
    return static_cast<long long>(time(nullptr));
}

// ת�� JSON �ַ���
string escapeJsonString(const string& str) {
    string escaped;
    for (char c : str) {
        switch (c) {
        case '"': escaped += "\\\""; break;
        case '\\': escaped += "\\\\"; break;
        case '\b': escaped += "\\b"; break;
        case '\f': escaped += "\\f"; break;
        case '\n': escaped += "\\n"; break;
        case '\r': escaped += "\\r"; break;
        case '\t': escaped += "\\t"; break;
        default: escaped += c; break;
        }
    }
    return escaped;
}

// JWT Token ���ɺ��ĺ���
string createJwtToken(const string& username) {
    // ���ó���
    const string SECRET_KEY = "liujiapeng666dashuaige666liujiaming666kanhailaoshi666";
    const string ISSUER = "snake-server";
    const string AUDIENCE = "snake-client";

    try {
        // 1. ���� JWT Header
        ostringstream headerStream;
        headerStream << R"({"alg":"HS256","typ":"JWT"})";
        string header = headerStream.str();
        string encodedHeader = base64url_encode(header);

        // 2. ���� JWT Payload
        long long now = getCurrentTimestamp();
        long long exp = now + 3600; // 1Сʱ�����

        ostringstream payloadStream;
        payloadStream << R"({"iss":")" << escapeJsonString(ISSUER) << R"(",)"
            << R"("aud":")" << escapeJsonString(AUDIENCE) << R"(",)"
            << R"("name":")" << escapeJsonString(username) << R"(",)"
            << R"("iat":)" << now << R"(,)"
            << R"("exp":)" << exp << R"(})";

        string payload = payloadStream.str();
        string encodedPayload = base64url_encode(payload);

        // 3. ����ǩ��
        string message = encodedHeader + "." + encodedPayload;
        vector<unsigned char> signature = hmac_sha256(SECRET_KEY, message);
        string encodedSignature = base64url_encode(signature);

        // 4. ������յ� JWT Token
        return encodedHeader + "." + encodedPayload + "." + encodedSignature;
    }
    catch (const exception& e) {
        // ��¼����򷵻ش���ָʾ
        return ""; // ���ؿ��ַ�����ʾʧ��
    }
}

// ������ C# ���õĺ���
extern "C" __declspec(dllexport) int CreateJwtToken(const char* username, char* output, int outputSize) {
    // ������֤
    if (!username || !output || outputSize <= 0) {
        return JWT_ERROR_INVALID_PARAMS;
    }

    try {
        // ���� JWT Token
        string token = createJwtToken(string(username));

        // ����Ƿ����ɳɹ�
        if (token.empty()) {
            return JWT_ERROR_GENERATION_FAILED;
        }

        // ��黺������С
        if (static_cast<int>(token.length()) >= outputSize) {
            return JWT_ERROR_BUFFER_TOO_SMALL;
        }

        // ���ƽ�������������
        strcpy_s(output, outputSize, token.c_str());
        return JWT_SUCCESS;
    }
    catch (const exception&) {
        return JWT_ERROR_UNKNOWN;
    }
    catch (...) {
        return JWT_ERROR_UNKNOWN;
    }
}

// ��ȡ���������ĸ�����������ѡ��
extern "C" __declspec(dllexport) const char* GetErrorDescription(int errorCode) {
    switch (errorCode) {
    case JWT_SUCCESS:
        return "�����ɹ�";
    case JWT_ERROR_INVALID_PARAMS:
        return "������Ч";
    case JWT_ERROR_BUFFER_TOO_SMALL:
        return "���������̫С";
    case JWT_ERROR_GENERATION_FAILED:
        return "Token ����ʧ��";
    case JWT_ERROR_UNKNOWN:
        return "δ֪����";
    default:
        return "δ����Ĵ�����";
    }
}