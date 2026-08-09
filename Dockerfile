# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY Packlead.slnx ./
COPY Packlead.Api/Packlead.Api.csproj Packlead.Api/
COPY Packlead.Application/Packlead.Application.csproj Packlead.Application/
COPY Packlead.Domain/Packlead.Domain.csproj Packlead.Domain/
COPY Packlead.Infrastructure/Packlead.Infrastructure.csproj Packlead.Infrastructure/
RUN dotnet restore Packlead.Api/Packlead.Api.csproj

COPY Packlead.Api/ Packlead.Api/
COPY Packlead.Application/ Packlead.Application/
COPY Packlead.Domain/ Packlead.Domain/
COPY Packlead.Infrastructure/ Packlead.Infrastructure/

RUN dotnet publish Packlead.Api/Packlead.Api.csproj -c Release -o /app/publish --no-restore \
    --self-contained false \
    /p:PublishTrimmed=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_gcServer=0
EXPOSE 8080

RUN addgroup -S packlead && adduser -S packlead -G packlead
USER packlead

COPY --from=build --chown=packlead:packlead /app/publish .

ENTRYPOINT ["dotnet", "Packlead.Api.dll"]
