#include "pch.h"
#include "sobel-cpp-dll.h"
#include <cmath>
#include <algorithm>

static const int Gx[3][3] = {
    { 1,  0, -1 },
    { 2, 0, -2 },
    { 1,  0, -1 }
};

static const int Gy[3][3] = {
    { 1,  2,  1 },
    { 0,   0,  0 },
    { -1, -2, -1 }
};

extern "C" __declspec(dllexport) void __cdecl CppSobelFunction(const unsigned char* inputImage, unsigned char* outputImage, int width, int height) {
    for (int x = 0; x < width; x++) {
        outputImage[x] = 0; 
        outputImage[(height - 1) * width + x] = 0; 
    }
    for (int y = 0; y < height; y++) {
        outputImage[y * width] = 0;
        outputImage[y * width + (width - 1)] = 0;
    }

    for (int y = 1; y < height - 1; y++) {
        for (int x = 1; x < width - 1; x++) {
            int sumX = 0;
            int sumY = 0;
            
            for (int j = -1; j <= 1; j++) {
                for (int i = -1; i <= 1; i++) {
                    int pixel = inputImage[(y + j) * width + (x + i)];
                    sumX += pixel * Gx[j + 1][i + 1];
                    sumY += pixel * Gy[j + 1][i + 1];
                }
            }
           
            int magnitude = abs(sumX) + abs(sumY);
            if (magnitude > 255)
                magnitude = 255;
            outputImage[y * width + x] = static_cast<unsigned char>(magnitude);
        }
    }

}


