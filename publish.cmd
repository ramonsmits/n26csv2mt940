@ECHO OFF
dotnet publish n26csv2mt940 --configuration Release --framework net8.0  --self-contained  --verbosity quiet --runtime win-x86   --output ./build/win-x86
dotnet publish n26csv2mt940 --configuration Release --framework net8.0  --self-contained  --verbosity quiet --runtime win-arm64 --output ./build/win-arm64
dotnet publish n26csv2mt940 --configuration Release --framework net8.0  --self-contained  --verbosity quiet --runtime osx-x64   --output ./build/osx-x64
dotnet publish n26csv2mt940 --configuration Release --framework net8.0  --self-contained  --verbosity quiet --runtime osx-arm64 --output ./build/osx-arm64
dotnet publish n26csv2mt940 --configuration Release --framework net8.0  --self-contained  --verbosity quiet --runtime linux-x64 --output ./build/linux-x64
dotnet publish n26csv2mt940 --configuration Release --framework net8.0  --self-contained  --verbosity quiet --runtime linux-arm --output ./build/linux-arm
