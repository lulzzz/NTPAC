#!/bin/bash

ARCH_ID='ubuntu.16.04-arm'
TARGET_HOST='node1'
TARGET_PORT='6666'
TARGET_USER='marta'
TARGET_DIR="/home/marta/ntpac"
CONFIGURATION="Debug"

if [ $# -eq 0 ]
then
	echo "./buildAndDeploy PROJECT_DIR"
	exit 1
fi

PROJECT_DIR="$1"
PROJECT_NAME=$(basename $1)

set -x
cd "$PROJECT_DIR"
PUBLISH_DIR="$PROJECT_DIR/bin/$CONFIGURATION/netcoreapp2.1/$ARCH_ID/publish/"
#rm -rf "$PUBLISH_DIR"
dotnet publish --runtime "$ARCH_ID" --configuration "$CONFIGURATION" && rsync -avzh -e "ssh -p $TARGET_PORT" "$PUBLISH_DIR" "$TARGET_USER@$TARGET_HOST:$TARGET_DIR/$PROJECT_NAME"
cd -
