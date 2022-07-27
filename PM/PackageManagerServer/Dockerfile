FROM mcr.microsoft.com/dotnet/aspnet:6.0

LABEL author="jimmybarnes"

RUN mkdir -p /usr/local/PackageManagerServer
RUN mkdir -p /bin
WORKDIR /bin

ADD bin/Publish .

CMD ["dotnet", "PackageManagerServer.dll"]