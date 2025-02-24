# Sobel Filter

Sobel Filter is a performance comparison project that demonstrates the speed differences between two implementations of the Sobel edge detection algorithm. The project consists of dynamic link libraries (DLLs) written in C++ and Assembly, both of which are invoked from a C# application.

## Overview

The Sobel operator is widely used in image processing to detect edges by calculating the gradient of image intensity. This repository provides:
- A main C# application that loads and compares the performance of two Sobel filter implementations.
- A C++ DLL that implements the Sobel filter.
- An Assembly DLL that implements the Sobel filter with a focus on performance optimization.
- Sample assets to test and demonstrate the functionality and speed of both implementations.

## Features

- **Edge Detection:** Utilizes the Sobel operator to highlight edges in images.
- **Performance Comparison:** Showcases and compares the execution speed between the C++ and Assembly implementations.
- **Multi-language Integration:** Combines C#, C++, and Assembly to balance ease of development with low-level performance optimization.
- **Modular Design:** Separated into distinct modules for the main C# application and the supporting DLLs (C++ and Assembly).

## Repository Structure

- **assets/**: Contains sample images and test resources.
- **sobel-filter/**: The main C# project that loads the C++ and Assembly DLLs and performs the performance comparison.
- **sobel-cpp-dll/**: The C++ project that builds a dynamic link library implementing the Sobel filter.
- **sobel-ddl/**: The Assembly project that builds a dynamic link library implementing the Sobel filter.
- **.gitignore**: Specifies files and folders that should not be tracked by Git.
