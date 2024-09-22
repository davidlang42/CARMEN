#!/bin/bash
set -e
dotnet publish -f net8.0-maccatalyst -c Release
PKG_FILE="`ls -1 ./bin/Release/net8.0-maccatalyst/publish/Carmen.Mobile*.pkg | tail -n 1`"
echo Press ENTER to submit to Apple: $PKG_FILE
read
xcrun notarytool submit $PKG_FILE --wait --apple-id davidlang42@gmail.com --team-id 45435R99CA # this will prompt for an AppleID app specific password
xcrun stapler staple $PKG_FILE
xcrun stapler validate $PKG_FILE
