FROM nginx:alpine AS nginxbase

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["OpenId.BlazorWasm/OpenId.BlazorWasm.csproj", "OpenId.BlazorWasm/"]
COPY ["OpenId.Shared/OpenId.Shared.csproj", "OpenId.Shared/"]

RUN dotnet restore "OpenId.Shared/OpenId.Shared.csproj"
RUN dotnet restore "OpenId.BlazorWasm/OpenId.BlazorWasm.csproj"
COPY . .

WORKDIR "/src/OpenId.BlazorWasm"
RUN dotnet build "OpenId.BlazorWasm.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenId.BlazorWasm.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM  nginxbase AS final
WORKDIR /usr/share/nginx/html
RUN #rm -f /usr/share/nginx/html/appsettings.json

COPY  OpenId.BlazorWasm/nginx.conf /etc/nginx/nginx.conf

ARG BLAZOR_ENVIRONMENT
RUN echo 'Environment8=' ${BLAZOR_ENVIRONMENT}

RUN sed -i "s/blazor-environment/${BLAZOR_ENVIRONMENT}/" /etc/nginx/nginx.conf
COPY --from=publish /app/publish/wwwroot .                   
