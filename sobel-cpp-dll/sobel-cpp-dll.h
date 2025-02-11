#pragma once


extern "C" __declspec(dllexport) void __cdecl CppSobelFunction(
    const unsigned char* inputImage,
    unsigned char* outputImage,
    int width,
    int height
);