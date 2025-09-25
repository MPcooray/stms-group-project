# Multi-stage Dockerfile that builds backend and frontend and produces a single runtime image

######### Build backend #########
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-backend
WORKDIR /src

# Copy backend sources and publish
COPY Backend/ ./Backend/
WORKDIR /src/Backend/STMS.Api

RUN dotnet restore "STMS.Api.csproj"
RUN dotnet publish "STMS.Api.csproj" -c Release -o /app/publish --no-restore

######### Build frontend #########
FROM node:20-alpine AS build-frontend
WORKDIR /app

# Allow build-time API base for the frontend
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# Copy frontend sources
COPY Frontend/package.json Frontend/package-lock.json* ./
COPY Frontend/pnpm-lock.yaml ./
COPY Frontend/ ./

# Fail early if entrypoint missing in frontend context (helps CI diagnostics)
RUN [ -f ./docker-entrypoint.sh ] || (echo "Frontend: docker-entrypoint.sh missing in build context" && false)

RUN npm ci --legacy-peer-deps || npm install
# Create a production env file so Vite will embed the API base at build time.
# Use the build-time value (set via --build-arg) so CI can choose the correct path.
RUN echo "VITE_API_BASE_URL=${VITE_API_BASE_URL}" > .env.production

RUN npm run build

######### Runtime image #########
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy backend publish
COPY --from=build-backend /app/publish .

# Copy frontend static output into ASP.NET Core's wwwroot so the backend serves it
# The frontend Dockerfile used /app/build as the output; adjust if your build outputs to /app/dist
COPY --from=build-frontend /app/dist ./wwwroot

# Entrypoint writes runtime config for the frontend and then starts dotnet
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
