#!/bin/bash

rm -r "bin"

rm -r "CustomRadioStation/obj"

/home/test/.dotnet/dotnet publish CustomRadioStation/CustomRadioStation.csproj -c ReleaseLinux