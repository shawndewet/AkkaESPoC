#!/usr/bin/env bash
# Builds docker images
parent_path=$( cd "$(dirname "${BASH_SOURCE[0]}")" ; pwd -P )
cd "$parent_path"

version="0.1.0"
hostImageName="AkkaESPoC.host"
webImageName="AkkaESPoC.web"

if [ -z $1 ]; then
	echo "No tag for [${hostImageName}] specified. Defaulting to [${version}]"
	echo "No tag for [${webImageName}] specified. Defaulting to [${version}]"
else
	version="$1"
	echo "Building [${hostImageName}] with tag [${version}]"
	echo "Building [${webImageName}] with tag [${version}]"
fi

dotnet publish ./AkkaESPoC.Host/AkkaESPoC.Host.csproj -c Release -p:Version=${version}

docker build ./AkkaESPoC.Host/. -t "${imageName}:${version}"

dotnet publish ./AkkaESPoC.WebApp/AkkaESPoC.WebApp.csproj -c Release -p:Version=${version}

docker build ./AkkaESPoC.WebApp/. -t "${imageName}:${version}"