@echo off
setlocal
@color 70
cd /d %~dp0

@color 71
rem Prompting the user for additional input
set /p "additionalInput=Scale size (2/3/4): "

rem Displaying the combined input
echo.
echo.
echo SKUUUUUYYYYYYYYYYYYY
echo.
echo.
echo realesrgan-ncnn-vulkan.exe -i tmp_frames -o out_frames -n realesr-animevideov3 -s %additionalInput% -f jpg 
echo.
echo.
echo.
@color 01
realesrgan-ncnn-vulkan.exe -i tmp_frames -o out_frames -n realesr-animevideov3 -s %additionalInput% -f jpg 
pause
@color 02
pause
@color 04




