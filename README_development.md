# 🏊 Swimming Tournament Management System (STMS) 

## 📌 Project Overview 
The **STMS** platform streamlines inter-university swimming tournament management with admin tools and public visibility for results and leaderboards.   
This branch focuses on **continuous development**, integration testing, and pre-deployment validation. 

Developed as part of **SE3022 – Case Study Project (Year 3, Semester 1)** at **SLIIT**. 

---

## 👥 Team Members 
- **M.P. Cooray** – IT23194830   
- **P.W.K.W. Rupasinghe** – IT23283312   
- **N.D. Gamage** – IT23161788   
- **W.M. Chamudini** – IT23292154   

--- 

## ⚙️ Tech Stack 
- **Frontend:** React.js   
- **Backend:** ASP.NET Web API + ADO.NET   
- **Database:** MySQL   
- **Version Control:** GitHub   
- **Deployment:** Azure App Service / Docker   
- **Testing Tools:** Selenium, JMeter, Unit Test  
- **CI:** GitHub Actions (`ci.yml`) 

--- 

## 🚀 System Features 
- Secure Admin login & authentication   
- Tournament creation & management   
- University & Player registration   
- Event timing & automatic point allocation   
- Real-time leaderboard (by event, player, university)   
- Public portal for results (no login required)   
- Export leaderboard as PDF   

--- 

## 🚀 Development Features 
- Event and leaderboard modules under active iteration   
- CI verification for backend and frontend   
- Manual merge control before production deployment   

--- 
## 📂 Project Structure 
``` 
stms-group-project/ 
├── frontend/ # React.js code 
├── backend/ # ASP.NET Web API code 
├── database/ # SQL scripts for schema & seed data 
├── tests/ # Unit & integration tests 
├── docs/ # SRS, diagrams, reports  
├── .github/ # CI/CD workflows (GitHub Actions) 
└── README.md # Project documentation 

``` 
--- 

## 🛠️ Setup Instructions 

### 1. Clone Repository 
```bash
git clone https://github.com/<org-or-username>/stms-group-project.git 
cd stms-group-project 
```

### 2. Backend Setup 
```bash
cd Backend/STMS.Api 
dotnet restore 
dotnet run 
```

### 3. Frontend Setup 
```bash
cd Frontend 
npm install 
npm run dev 
```

---

## 🔄 Git Workflow (Development Branch – CI Focus) 
This branch is for **active development and validation**. 

- Code is built and tested via GitHub Actions (`ci.yml`) 
- CI verifies: 
  - Backend builds and tests 
  - Frontend compiles successfully 
  - Unit tests run without errors   

### CI Checks 
| Check | Tool | Description | 
|--------|------|-------------| 
| Backend Build | .NET CLI | Ensures API builds correctly | 
| Backend Tests | xUnit | Validates API logic | 
| Frontend Build | Vite | Ensures no build errors | 
| Frontend Tests | Jest/Vitest | Checks React components | 

---

## 🔁 Branch Integration 
- Developers push to `development` 
- After successful CI checks, merge into `main` 
- Switch default branch to `main` for deployment 
 
---

## 📦 CI Workflow Summary 
| Purpose | Workflow | Trigger | Output | 
|----------|-----------|----------|---------| 
| Development CI | `ci.yml` | Push or PR to `development` | Verifies build + tests | 

---

## 🧪 Manual Testing 
```bash
# Backend 
dotnet test 

# Frontend 
npm test 
```

---

## 📜 License 
This project is developed for academic purposes under the SE3022 – Case Study Project module. 
