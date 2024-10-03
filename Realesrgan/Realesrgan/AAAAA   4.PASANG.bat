@echo off
setlocal
@color 70
cd /d %~dp0
echo Choose video input
pause
echo.
rem Batch picker functionality
set "powershellCommand=[System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms') | Out-Null; $fileDialog = New-Object System.Windows.Forms.OpenFileDialog; $fileDialog.ShowDialog() | Out-Null; $fileDialog.FileName | Out-File -Encoding utf8 -FilePath '%temp%\selected_file.txt' -NoNewline"
powershell -Command "%powershellCommand%"

rem Remove Byte Order Mark (BOM) from the file content
for /f "usebackq delims=" %%a in (`powershell -Command "(Get-Content -Path '%temp%\selected_file.txt' -Raw) -replace '^(\xEF\xBB\xBF)?', ''"`) do (
    set "batchPickerOutput=%%a"
    goto :next
)
:next

rem Displaying the output from the batch picker script
echo Video input PATH: %batchPickerOutput%


rem Extracting the extension from the file path
for %%A in ("%batchPickerOutput%") do (set "extension=%%~xA"
set "filename=%%~nA")

echo Extension: %extension%

rem Prompting the user for additional input
cd ffmpeg\bin
echo.
echo.
@color 71
set /p "FPSInput=FPS, Check your video in properties > detail > framerate (onepiece_demo : 23.98 ) : "
echo.
@color 74
echo.
echo Custom Prompt, copy what is in brackets if audio result is delayed or faster ( -vf "setpts=(X/Y)*PTS" )
echo X=original video duration / Y=failed finished video duration
set /p "additionalInput=No need=just Enter : "
rem Displaying the combined input
echo.
echo.
echo SKUUUUUYYYYYYYYYYYYY
echo.
echo.
@color 01
echo ffmpeg -i ../../out_frames/frame%%08d.jpg -i "%batchPickerOutput%" -map 0:v:0 -map 1:a:0 -c:a copy -c:v libx264 -r %FPSInput% -pix_fmt yuv420p "%additionalInput% ../../%filename%-X%additionalInput%-%extension% "
echo.
echo.
echo.
ffmpeg -i ../../out_frames/frame%%08d.jpg -i "%batchPickerOutput%" -map 0:v:0 -map 1:a:0 -c:a copy -c:v libx264 -r %FPSInput% -pix_fmt yuv420p "%additionalInput% ../../../../%filename%-OUTPUT%extension%"
@color 02
pause
@color 04

endlocal




