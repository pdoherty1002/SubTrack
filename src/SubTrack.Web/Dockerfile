FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/SubTrack.Web/SubTrack.Web.csproj src/SubTrack.Web/
RUN dotnet restore src/SubTrack.Web/SubTrack.Web.csproj
COPY src/ src/
RUN dotnet publish src/SubTrack.Web/SubTrack.Web.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/SubTrack.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80