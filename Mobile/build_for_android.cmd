@echo off
set /p AndroidSigningPassword=Password for keystore? 
dotnet publish -f net7.0-android -c Release || goto :done
echo.
echo Signed APK can be found in .\bin\Release\net7.0-android\publish
:done
pause