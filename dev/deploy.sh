#!/usr/bin/env bash

set -e

usage() {
  echo "Usage: $0 server_name deployment_name" >&2
  exit 1
}

server_name="$1"
shift

deployment_name="$1"
shift

if [ -z "$server_name" ] || [ -z "$deployment_name" ]; then
  usage
fi

build_dir="builds/$BITBUCKET_REPO_SLUG/$BITBUCKET_BUILD_NUMBER"

set -x

rsync -ravlL --delete publish/ "$server_name:$deployment_name/report-generator"

ssh "$server_name" -- nixops deploy -d "$deployment_name"
