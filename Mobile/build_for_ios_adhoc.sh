#!/bin/bash
set -e
rm -r bin
rm -r obj
dotnet publish -f net7.0-ios -c Release -p:CodesignProvision="CARMEN (Ad-Hoc)"
echo BUILT FOR AD-HOC DEPLOYMENT
echo Distribute using Apple Configurator: https://apps.apple.com/app/id1037126344
echo User guide: https://support.apple.com/en-au/guide/apple-configurator-mac/welcome/mac
