#!/bin/bash
set -e
rm -r bin
rm -r obj
dotnet publish -f net8.0-ios -c Release -p:CodesignProvision="CARMEN (App Store)"
IPA_FILE="`ls -1 ./bin/Release/net8.0-ios/ios-arm64/publish/Carmen.Mobile*.ipa | tail -n 1`"
echo Press ENTER to submit to Apple AppStore: $IPA_FILE
read
xcrun altool --upload-app -f $IPA_FILE -t ios --username davidlang42@gmail.com --apple-id davidlang42@gmail.com # this will prompt for an AppleID app specific password
# could use Transporter instead of xcrun: https://apps.apple.com/us/app/transporter/id1450874784?mt=12
