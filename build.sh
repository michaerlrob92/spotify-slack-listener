#!/bin/bash
export DOTNET_ROOT=/opt/buildhome/.dotnet
./dotnet-install.sh --version 3.1.200
dotnet publish "SpotifySlackListener/SpotifySlackListener.csproj" -c Release -o "published" --runtime "linux-x64" --framework netcoreapp3.1