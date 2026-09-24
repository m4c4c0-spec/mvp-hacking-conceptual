#!/usr/bin/env bash
# Compila las capas puras de EthicalLab, corre los tests EditMode con NUnit y
# revisa la sintaxis de todo Assets/**/*.cs. Sin abrir Unity.
#
# Requiere el SDK de .NET 8. Si no está en PATH, se instala en ~/.dotnet:
#   curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0
set -euo pipefail
cd "$(dirname "$0")/.."

if ! command -v dotnet >/dev/null 2>&1; then
  if [ -x "$HOME/.dotnet/dotnet" ]; then
    export PATH="$HOME/.dotnet:$PATH"
    export DOTNET_ROOT="$HOME/.dotnet"
  else
    echo "Falta dotnet. Instala .NET 8 SDK: curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0" >&2
    exit 2
  fi
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

echo "== 1/3 capas puras (Shared, Domain, Application) → netstandard2.1 / C# 9"
dotnet build Tools/EthicalLab.Pure/EthicalLab.Pure.csproj -c Release -v quiet -nologo

echo "== 2/3 tests EditMode (NUnit)"
dotnet test Tools/EthicalLab.PureTests/EthicalLab.PureTests.csproj -c Release -v quiet -nologo

echo "== 3/3 sintaxis de Assets/**/*.cs (Roslyn)"
dotnet run --project Tools/EthicalLab.SyntaxCheck/EthicalLab.SyntaxCheck.csproj -c Release -v quiet -- "$PWD"
