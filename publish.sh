#!/usr/bin/env sh

[ -z $1 ] && echo "app key missing" && exit 1
[ ! -d "./artefacts/*.nupkg" ] && echo "no packages to publish" && exit 1

dotnet nuget push ./artefacts/*.nupkg -k $1 -s https://api.nuget.org/v3/index.json
