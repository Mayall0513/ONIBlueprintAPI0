FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /BlueprintRepository

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c release -o /BlueprintRepository --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /BlueprintRepository
COPY --from=build-env /BlueprintRepository .
ENTRYPOINT ["dotnet", "BlueprintRepository.dll"]