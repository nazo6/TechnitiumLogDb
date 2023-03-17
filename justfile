default:
  @just --list

build:
  dotnet build

copy:
  cp -r ./bin/Debug/net7.0/* ./docker/dns/

start: build copy
  cd docker && docker-compose up -d

stop:
  cd docker && docker-compose down


update: build copy
  cd docker && docker-compose restart dns
