@echo off
for /f "delims=" %%A in ('cd') do (
     set foldername=%%~nxA
    )

start %SystemRoot%\explorer.exe %AppData%\Godot\app_userdata\%foldername%
exit