#!/bin/sh
set -e

API_BASE=${VITE_API_BASE_URL:-${API_BASE:-}}
if [ -n "$API_BASE" ]; then
  echo "Writing runtime config with API base: $API_BASE"
  cat > /usr/share/nginx/html/config.js <<EOF
window.__API_BASE__ = "$API_BASE";
EOF
else
  echo "No runtime API base provided; skipping config.js creation"
fi

nginx -g 'daemon off;'
