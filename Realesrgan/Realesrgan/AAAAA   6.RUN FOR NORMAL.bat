@echo off
setlocal
@color 70
cd /d %~dp0

echo Choose Image
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
@color 71
rem Displaying the output from the batch picker script
echo Image PATH: %batchPickerOutput%

rem Extracting the extension from the file path
for %%A in ("%batchPickerOutput%") do (set "extension=%%~xA"
set "filename=%%~nA")

echo Extension: %extension%
rem Prompting the user for additional input
echo.
echo Scale size (You cant change it lol): 4
set "additionalInput=4"

rem Displaying the combined input
echo.
echo.
echo SKUUUUUYYYYYYYYYYYYY
echo.
echo.
echo realesrgan-ncnn-vulkan.exe -i  "%batchPickerOutput%" -n realesrgan-x4plus -o "%filename%-X%additionalInput%N-%extension%" -s %additionalInput%
echo.
echo.
echo.
@color 01
realesrgan-ncnn-vulkan.exe -i  "%batchPickerOutput%" -n realesrgan-x4plus -o "%filename%-X%additionalInput%N-%extension%" -s %additionalInput%
@color 02
pause
@color 04
endlocal




