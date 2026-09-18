# Bearing
A 3D and 2D game engine

## Building
Command to build for windows: dotnet publish -c Release -r win-x64 --self-contained true

Command to build for linux: dotnet publish -c Release -r linux-x64 --self-contained true

final files are found in the '/bin/Release/net8.0/[platform]/publish' folder
where [platform] is either 'win-x64' or 'linux-x64' depending on which build command was ran