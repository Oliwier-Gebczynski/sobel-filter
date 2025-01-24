#ifndef SOBEL_FILTER_H
#define SOBEL_FILTER_H

#include <string>

extern "C" {
    __declspec(dllexport) void ApplySobelFilter(unsigned char* inputImage, const char* outputPath, int width, int height);
}

#endif // SOBEL_FILTER_H