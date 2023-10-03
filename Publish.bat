RD /S /Q "bin"

RD /S /Q "CustomRadioStation\obj"

pause

dotnet publish CustomRadioStation\CustomRadioStation.csproj -c Release

pause