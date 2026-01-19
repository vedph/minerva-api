# Stage 1: Base (Runtime)
# We don't specify --platform so it pulls the version matching the TARGET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: Build (SDK)
# We use --platform=$BUILDPLATFORM to ensure the SDK runs at native speed 
# on the machine doing the work (e.g., your Intel GitHub Action or Mac)
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["Minerva.Api/Minerva.Api.csproj", "Minerva.Api/"]

# Restore specifically for the target architecture
RUN dotnet restore "Minerva.Api/Minerva.Api.csproj" -a $TARGETARCH

COPY . .
# Publish specifically for the target architecture
RUN dotnet publish "Minerva.Api/Minerva.Api.csproj" -c Release -o /app/publish -a $TARGETARCH --self-contained false

# Stage 4: Final (The actual image)
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Minerva.Api.dll"]
