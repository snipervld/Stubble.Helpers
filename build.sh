#!/usr/bin/env sh
set -e

rm -rf artefacts

dotnet test -c Release
dotnet pack -c Release -o artefacts
