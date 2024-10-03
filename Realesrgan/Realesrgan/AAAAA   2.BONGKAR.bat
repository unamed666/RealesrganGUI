@echo off
setlocal
@color 70
cd /d %~dp0
echo Pilih video input
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


rem Displaying the combined input
cd ffmpeg\bin
echo.
@color 71
echo.
echo SKUUUUUYYYYYYYYYYYYY
echo.
echo.
echo ffmpeg -i "%batchPickerOutput%" -qscale:v 1 -qmin 1 -qmax 1 -vsync 0 ../../tmp_frames/frame%%08d.jpg 
echo.
echo.
echo.
@color 01
ffmpeg -i "%batchPickerOutput%" -qscale:v 1 -qmin 1 -qmax 1 -vsync 0 ../../tmp_frames/frame%%08d.jpg 
@color 02
pause
@color 04






