@echo off
cd /D "%~dp0"
echo This tool will wipe your first disk and install
pause
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
echo Enable EMS
bcdedit /store S:\EFI\Microsoft\Boot\BCD /ems {default} on
bcdedit /store S:\EFI\Microsoft\Boot\BCD /emssettings EMSPORT:1 EMSBAUDRATE:9600
echo .
echo --------- Finished ---------
echo Press any key to reboot
pause
wpeutil reboot