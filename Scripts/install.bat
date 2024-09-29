@echo off
cd /D "%~dp0"
echo .
echo --------- Cleaning Up Disk ---------
echo .
diskpart /s initdisk.txt
echo .
echo --------- Deploying Windows Image ---------
echo .
echo [*] Apply Image
dism /apply-image /imagefile:G:\nadia-build\build\nadia_win11_ltsc_26100.wim /index:1 /applydir:W:\
echo .
echo [*] Add Drivers
dism /image:W:\ /Add-Driver /Driver:%cd%\drivers /Recurse
echo .
echo --------- Creating Boot Loader ---------
echo .
bcdboot W:\Windows /s S:
echo .
echo --------- Finished ---------
echo Press any key to reboot!
pause
wpeutil reboot