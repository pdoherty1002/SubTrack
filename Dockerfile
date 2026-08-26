# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the .csproj files first, so Docker can cache the restore step
# separately from the rest of the source. Restore only reruns when a
# project reference or package actually changes, not on every code edit.
COPY src/SubTrack.Api/SubTrack.Api.csproj src/SubTrack.Api/
COPY src/SubTrack.Domain/SubTrack.Domain.csproj src/SubTrack.Domain/
COPY src/SubTrack.Infrastructure/SubTrack.Infrastructure.csproj src/SubTrack.Infrastructure/
RUN dotnet restore src/SubTrack.Api/SubTrack.Api.csproj

# Now copy everything else and build
COPY src/ src/
RUN dotnet publish src/SubTrack.Api/SubTrack.Api.csproj -c Release -o /app/publish --no-restore

# ---- Final stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SubTrack.Api.dll"]