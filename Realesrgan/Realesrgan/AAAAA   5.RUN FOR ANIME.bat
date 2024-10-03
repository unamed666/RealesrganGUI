@echo off
setlocal
@color 70
cd /d %~dp0

echo Choose image
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
echo Image PATH: %batchPickerOutput%

rem Extracting the extension from the file path
for %%A in ("%batchPickerOutput%") do (set "extension=%%~xA"
set "filename=%%~nA")

echo Extension: %extension%
@color 71
rem Prompting the user for additional input
echo.
echo.
set /p "additionalInput=Scale Size (2/3/4): "





rem Displaying the combined input
echo.
echo.
echo SKUUUUUYYYYYYYYYYYYY
echo.
echo.
echo realesrgan-ncnn-vulkan.exe -i  "%batchPickerOutput%" -n realesr-animevideov3 -s %additionalInput% -o "%filename%-X%additionalInput%A-%extension%" 
echo.
echo.
echo.
@color 09
realesrgan-ncnn-vulkan.exe -i  "%batchPickerOutput%" -n realesr-animevideov3 -s %additionalInput%  -o "%filename%-X%additionalInput%A-%extension%"
@color 0A
pause
@color 0C
endlocal




