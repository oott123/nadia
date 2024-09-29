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
echo [*] Bypass NRO
reg load HKLM\SOFT W:\Windows\System32\config\SOFTWARE
reg load HKU\DEFU W:\Users\Default\NTUSER.DAT
reg add HKLM\SOFT\Microsoft\Windows\CurrentVersion\OOBE /v BypassNRO /t REG_DWORD /d 1 /f
reg add HKU\DEFU\Software\Microsoft\Windows\CurrentVersion\CloudExperienceHost\Intent\PersonalDataExport /f /v PDEShown /t REG_DWORD /d 2
reg unload HKLM\SOFT
reg unload HKU\DEFU
echo .
echo --------- Creating Boot Loader ---------
echo .
bcdboot W:\Windows /s S:
echo .
echo --------- Finished ---------
echo Press any key to reboot!
pause
wpeutil reboot