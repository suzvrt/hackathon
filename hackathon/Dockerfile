FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Configuração do ambiente de build
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Instalação de dependências necessárias para Native AOT
RUN apt-get update && apt-get install -y \
    clang \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Cópia dos arquivos de projeto
COPY hackathon.csproj ./

# Restauração das dependências
RUN dotnet restore

# Cópia do código fonte
COPY . .

# Build e publicação com Native AOT
RUN dotnet publish -c Release \
    -r linux-x64 \
    -p:PublishAot=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=link \
    -p:EnableCompressionInSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o out

# Imagem de runtime otimizada
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy

WORKDIR /app

# Cópia dos arquivos publicados
COPY --from=build /app/out .

# Configuração de variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Exposição da porta
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["./hackathon"]