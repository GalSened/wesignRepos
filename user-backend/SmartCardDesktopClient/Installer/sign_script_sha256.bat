cd C:\Program Files (x86)\Windows Kits\10\App Certification Kit
signtool sign /fd SHA256 /a /f \\fs01\Production\WeSign\V3\Installer\SmartCardDesktopClient\codsign.pfx /p 1234 \\fs01\Production\WeSign\V3\Installer\SmartCardDesktopClient\SmartCardDesktopClientSetup.exe
pause