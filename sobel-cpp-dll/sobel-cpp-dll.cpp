#include "sobel-cpp-dll.h"
#include "pch.h"
#include <fstream>
#include <cmath>
#include <string>
#include <algorithm>

// Definicje macierzy Sobela
const int SOBEL_KERNEL_X[3][3] = { {-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1} };
const int SOBEL_KERNEL_Y[3][3] = { {-1, -2, -1}, {0, 0, 0}, {1, 2, 1} };

extern "C" {
    __declspec(dllexport) void ApplySobelFilter(unsigned char* inputImage, const char* outputPath, int width, int height) {
        unsigned char* outputImage = new unsigned char[width * height];

        // Ustawienie wszystkich pikseli na 0 w obrazie wyjœciowym
        for (int i = 0; i < width * height; i++) {
            outputImage[i] = 0;
        }

        // Przejœcie przez ka¿dy piksel obrazu, z wyj¹tkiem brzegów
        for (int y = 1; y < height - 1; y++) {
            for (int x = 1; x < width - 1; x++) {
                int sumX = 0, sumY = 0;

                // Aplikacja konwolucji z macierzami Sobela
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        int pixelIndex = (y + j) * width + (x + i);
                        int pixelValue = static_cast<int>(inputImage[pixelIndex]);
                        sumX += pixelValue * SOBEL_KERNEL_X[i + 1][j + 1];
                        sumY += pixelValue * SOBEL_KERNEL_Y[i + 1][j + 1];
                    }
                }

                // Obliczenie gradientu
                int gradientMagnitude = static_cast<int>(std::sqrt(sumX * sumX + sumY * sumY));
                // Ograniczenie wartoœci do zakresu 0-255
                gradientMagnitude = std::clamp(gradientMagnitude, 0, 255);

                // Przypisanie wartoœci do wyjœciowego obrazu
                int outputIndex = y * width + x;
                outputImage[outputIndex] = static_cast<unsigned char>(gradientMagnitude);
            }
        }

        // Zapisanie wyniku do pliku
        std::ofstream outputFile(outputPath, std::ios::binary);
        if (outputFile.is_open()) {
            // Zapisanie nag³ówka pliku BMP (bardzo uproszczone)
            // Plik BMP jest bardziej skomplikowany, ale tutaj u¿ywamy uproszczonej wersji dla celów demonstracyjnych
            unsigned char bmpHeader[54] = { 0 };
            bmpHeader[0] = 'B';
            bmpHeader[1] = 'M';
            int fileSize = 54 + width * height;
            memcpy(&bmpHeader[2], &fileSize, 4);
            bmpHeader[10] = 54;
            bmpHeader[14] = 40;
            memcpy(&bmpHeader[18], &width, 4);
            memcpy(&bmpHeader[22], &height, 4);
            bmpHeader[26] = 1;
            bmpHeader[28] = 8;

            outputFile.write((char*)bmpHeader, 54);
            outputFile.write((char*)outputImage, width * height);
            outputFile.close();
        }

        delete[] outputImage;
    }
}