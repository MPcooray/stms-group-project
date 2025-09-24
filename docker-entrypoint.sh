#!/bin/sh
set -e

# Frontend runtime config (serves a small JS file under /index.html to read)
: "${VITE_API_BASE_URL:=http://localhost:5001}"

if [ -d /app/wwwroot ]; then
  echo "Writing runtime config to /app/wwwroot/env-config.js"
  cat > /app/wwwroot/env-config.js <<EOF
window.__APP_ENV__ = {
  VITE_API_BASE_URL: "${VITE_API_BASE_URL}"
};
EOF
else
  echo "Warning: /app/wwwroot not found; frontend assets may not be present"
fi

# Start the ASP.NET Core app (PID 1)
exec dotnet STMS.Api.dll
