default:
  @just --list

build:
  dotnet build
  zip -rj output.zip ./bin/Debug/net7.0

start:
  cd docker && docker-compose up -d

stop:
  cd docker && docker-compose down
