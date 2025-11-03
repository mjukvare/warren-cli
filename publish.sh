#!/bin/zsh

 BUILD_DATE=$(date -u +%Y-%m-%d)
 GIT_HASH=$(git rev-parse --short HEAD)
 VERSION="$BUILD_DATE-$GIT_HASH"
 
 echo "Building version ${VERSION}"
 
 dotnet publish -o pub -c Release -r win-x64 /p:Version=$VERSION /p:InformationalVersion=$VERSION
 dotnet publish -o pub -c Release -r osx-arm64 /p:Version=$VERSION /p:InformationalVersion=$VERSION