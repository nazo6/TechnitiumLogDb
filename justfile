default:
  @just --list

build:
  dotnet build
  zip -rj bin/debug.zip ./bin/Debug/net7.0

release:
  dotnet build -c Release
  zip -rj bin/release.zip ./bin/Release/net7.0

start:
  cd docker && docker-compose up -d

stop:
  cd docker && docker-compose down
