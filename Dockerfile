FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o bin
EXPOSE 80
ENTRYPOINT ["dotnet", "bin/glytics.dll"]