# 🏊 Swimming Tournament Management System (STMS)

## 📌 Project Overview
The **Swimming Tournament Management System (STMS)** is a web-based application that enables administrators to manage inter-university swimming tournaments while allowing the public to view real-time results and leaderboards.  

This project is developed as part of **SE3022 – Case Study Project (Year 3, Semester 1)** at **SLIIT**.

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

---

## 🚀 Features
- Secure Admin login & authentication  
- Tournament creation & management  
- University & Player registration  
- Event timing & automatic point allocation  
- Real-time leaderboard (by event, player, university)  
- Public portal for results (no login required)  
- Export leaderboard as PDF  

---

## 📂 Project Structure
stms-group-project/
│── frontend/ # React.js code
│── backend/ # ASP.NET Web API code
│── database/ # SQL scripts for schema & seed data
│── tests/ # Unit & integration tests
│── docs/ # SRS, diagrams, reports
│── .github/ # CI/CD workflows (GitHub Actions)
│── README.md # Project documentation


---

## 🛠️ Setup Instructions

### 1. Clone Repository
```bash
git clone https://github.com/<org-or-username>/stms-group-project.git
cd stms-group-project

2. Backend Setup (ASP.NET + MySQL)

Configure appsettings.json with MySQL credentials.

Run database migration scripts inside /database/.

Start backend:

dotnet run

3. Frontend Setup (React.js)
cd frontend
npm install
npm start

4. Run Tests
dotnet test         # Backend tests
npm test            # Frontend tests

🔄 Git Workflow (with JIRA Integration)

Create a new branch from main using the JIRA issue key:

git checkout -b feature/STMS-12-login-ui


Commit with issue key:

git commit -m "STMS-12 Implement login API"


Push branch:

git push origin feature/STMS-12-login-ui


Open a Pull Request (PR) → link it to the JIRA story.

📦 CI/CD Pipeline

Automated builds & tests via GitHub Actions (.github/workflows/ci.yml)

Deployment to Azure App Service

Dockerized containers for portability

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
