default:
  @just --list

build:
  dotnet build

start:
  cd docker && docker-compose up -d

stop:
  cd docker && docker-compose down

copy:
  cp -r ./bin/Debug/net7.0/* ./docker/dns/

restart:
  cd docker && docker-compose restart dns

update: build restart
