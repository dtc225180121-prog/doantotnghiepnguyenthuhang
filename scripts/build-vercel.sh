#!/bin/sh
set -eu

rm -rf dist
mkdir -p dist
cp -R wwwroot/. dist/

if [ -n "${API_ORIGIN:-}" ]; then
  escaped_api_origin=$(printf '%s\n' "$API_ORIGIN" | sed 's/[\/&]/\\&/g')
  sed -i.bak "s/const DEPLOYED_API_ORIGIN = \"[^\"]*\";/const DEPLOYED_API_ORIGIN = \"$escaped_api_origin\";/" dist/assets/js/core/config.js
  rm -f dist/assets/js/core/config.js.bak
fi
