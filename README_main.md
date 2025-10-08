# 🏊 Swimming Tournament Management System (STMS)

## 📌 Project Overview
The **Swimming Tournament Management System (STMS)** is a web-based solution for managing inter-university swimming tournaments.  
Administrators can manage tournaments, events, players and universities, while the public can view real-time results and leaderboards.

This project is developed as part of **SE3022 – Case Study Project (Year 3, Semester 1)** at **SLIIT**.

---

## 👥 Team Members
- **M.P. Cooray** – IT23194830  
- **P.W.K.W. Rupasinghe** – IT23283312  
- **N.D. Gamage** – IT23161788  
- **W.M. Chamudini** – IT23292154  

---

## ⚙️ Tech Stack
- **Frontend:** React.js (Vite)
- **Backend:** ASP.NET Core Web API + ADO.NET
- **Database:** Azure SQL
- **Containerization:** Docker + GitHub Container Registry (GHCR)
- **Deployment:** Azure Web App (Containerized)
- **Version Control:** GitHub
- **Testing Tools:** Selenium, JMeter, xUnit

---

## 🚀 Features
- Secure Admin login & authentication  
- Tournament creation & management  
- University & Player registration  
- Event timing & automatic point allocation  
- Real-time leaderboard (by event, player, university)  
- Public portal for results (no login required)  
- Export leaderboard as PDF  
- CI/CD integration with GitHub Actions and Azure  

---

## 📂 Project Structure
```
stms-group-project/
├── frontend/      # React.js code
├── backend/       # ASP.NET Core API
├── database/      # SQL schema and seed data
├── tests/         # Unit & integration tests
├── docs/          # Reports, diagrams, and SRS
├── .github/       # CI/CD workflow files
└── README.md
```

---

## ⚙️ Local Development

### Backend (ASP.NET Core)
- Default URL: http://localhost:5287 (see `Backend/STMS.Api/Properties/launchSettings.json`)
- Run:
  ```bash
  cd Backend/STMS.Api
  dotnet run
  ```

### Frontend (React + Vite)
- Dev server: http://localhost:3000  
- Setup:
  ```bash
  cd Frontend
  cp .env.example .env.local
  npm install
  npm run dev
  ```

### CORS/Proxy
- Backend allows CORS for `localhost:3000` and `localhost:5173`.
- Vite proxies `/api` and `/auth` to the backend automatically.

---

## 🔄 Git Workflow (Main Branch – CI/CD Focus)
This branch is used **for production deployment**.

- The `main` branch is **containerized and deployed** to **Azure Web App**.
- Two GitHub Actions workflows handle deployment:
  - **`build-container.yml`** -> Builds Docker image and pushes to GHCR  
  - **`deploy-to-azure.yml`** -> Deploys the latest GHCR image to Azure  

### Deployment Process
1. Merge the latest tested code from `development` into `main`
2. CI/CD automatically:
   - Builds container  
   - Pushes to GHCR  
   - Deploys to Azure Web App  

> ⚠️ Switch the default branch to **main** before deployment.

---

## 🧪 Testing
- **Backend:** `dotnet test`
- **Frontend:** `npm test`

---

## 📦 CI/CD Overview
| Purpose | Workflow | Trigger | Output |
|----------|-----------|----------|---------|
| Container Build | `build-container.yml` | Push to `main` | Docker image to GHCR |
| Azure Deployment | `deploy-to-azure.yml` | Push to `main` | Deployed container on Azure |

---

📜 License

This project is developed for academic purposes under the SE3022 – Case Study Project module.


---

👉 This README gives you:
- Clear **overview** (good for grading).  
- **Team member section** (credit distribution).  
- **Setup instructions** (for examiners to run easily).  
- **Workflow guide** (showing Agile + GitHub + JIRA).  
- CI/CD mention (aligns with assignment goals:contentReference[oaicite:0]{index=0}).  

---