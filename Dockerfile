FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["QuizWebApp.csproj", ""]
RUN dotnet restore "./QuizWebApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "QuizWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuizWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

# install System.Drawing native dependencies
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuizWebApp.dll"]