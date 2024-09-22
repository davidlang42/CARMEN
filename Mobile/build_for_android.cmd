@echo off
set /p AndroidSigningPassword=Password for keystore? 
dotnet publish -f net8.0-android -c Release || goto :done
echo.
echo Signed APK and AAB can be found in .\bin\Release\net8.0-android\publish
:done
pause