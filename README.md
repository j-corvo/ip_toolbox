# Barcode Detector

This repository contains a toolbox for barcode detection using a custom-made optimized version of the OpenCV library. All the filters and image transformations for the barcode scanning process were implemented from scratch. The goal of this repository is to try to give an optimized version of the already implemented methods found in the OpenCV library. 

![image](https://github.com/j-corvo/ip_toolbox/assets/52609366/1c66246f-e7e5-4d09-9ed8-b9131508c285)


## Implementation and Methods

The methods implemented in this solution are the following and can be divided into categories:
 
- Color:
    - Negative
    - Gray
    - Isolate (Red, Green, Blue)
- Transforms:
    - Translation
    - Rotation
    - Bilinear Rotation
    - Zoom (click on a point)
    - Auto zoom
    - Bilinear Scaling
- Filters:
    - Uniform Mean
    - Non-Uniform Mean
    - Sobel
    - Differentiation
    - Roberts
- Visualization:
    - Histogram
    - Equalization Histogram
    - Histogram Filtering (horizontal mean, horizontal match, vertical mean, vertical match)
- Projections:
    - Vertical
    - Horizontal
- Morphology:
    - Dilation
    - Erosion
    - Open (1x, 5x, 10x, 20x)
    - Close (1x, 2x, 5x, 10x)
    - Vertical Line Preservation (isolate vertical lines)
    - Horizontal Line Preservation (isolates horizontal lines)
    - Otsu Binarization
    - BlackHat Transform
    - WhiteHat Transform
      
For barcode identification, some more methods were implemented such as:
- Angle Finder -> Identify the rotation angle of the barcode
- Isolate Numbers -> Segmentation of the numbers in the barcode
- IdentifyCB -> Identify the position of the Barcode and draw a square on it

## Performance 

In this toolbox, some methods could be improved, decreasing the processing time compared to the OpenCV library.

![SS_image](https://github.com/j-corvo/ip_toolbox/assets/52609366/a9feb9c7-9449-4796-947c-ce8e6e1f5672)

The times are depicted in the following table:

| Method   | ip_toolbox | OpenCV | Time difference (secs) |
| --- | --- | --- | --- |
| Roberts | 11.210 | 18.245 | -7.035 |      
| Differentiation | 11.321 | 16.036 | -4.715 |
| Otsu | 8.002 | 4.000 | +2.002 |
| Binarization | 3.000 | 6.002 | -3.002 |
| Sobel (3x3) | 28.735 | 8.206 | +20.529 |
| Bilinear Scaling | 29.651 | 1.096| +28.555 |

In conclusion, everybody knows that OpenCV is a highly optimized library with hundreds of contributors, thus, higher time results were expected from an implementation from scratch. However, it was satisfactory to see that, in some methods, the ip_toolbox had better results than OpenCV. 
It is always a challenge to defy the odds and try new approaches!

## How to use

1) Clone the repository to your computer
2) Open Visual Studio (the version doesn't matter, even though it is a project from 2019, it runs in the most recent version of VS)
3) Run the application and a window like this will open:

![image](https://github.com/j-corvo/ip_toolbox/assets/52609366/5b5ca711-fa01-40de-950c-aa93bf15f04b)
