FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "bin/glytics.dll"]