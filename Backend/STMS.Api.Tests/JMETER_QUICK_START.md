# JMeter Quick Reference - STMS Load Testing

## 🚀 Quick Start (5 Minutes)

### Step 1: ✅ JMeter Already Downloaded!
```
✓ Location: C:\Users\Cooray\Downloads\apache-jmeter-5.6.3\
✓ Ready to use!
```

### Step 2: Start Your Backend
```cmd
cd c:\stms\Backend\STMS.Api
dotnet run --no-launch-profile --urls http://localhost:5000
```
Wait for: `Now listening on: http://localhost:5000`

**Easier:** Double-click `START_BACKEND.bat` (it starts the API fixed on port 5000)

### Step 3: Open JMeter
```cmd
cd C:\Users\Cooray\Downloads\apache-jmeter-5.6.3\apache-jmeter-5.6.3\bin
jmeter.bat
```

**OR use the shortcut:** Double-click `START_JMETER.bat`

### Step 4: Create Test
1. Right-click "Test Plan" → Add → Threads → Thread Group
2. Right-click "Thread Group" → Add → Config → HTTP Request Defaults
   - Server: `localhost`
   - Port: `5000` (if you used START_BACKEND.bat)
   - Note: If you run without the script and see a different port in console (e.g., 5287), use that port here.
3. Right-click "Thread Group" → Add → Sampler → HTTP Request
   - Method: `GET`
   - Path: `/api/tournaments`
4. Right-click "Thread Group" → Add → Listener → View Results Tree
5. Click ▶ (Play button)

Done! You're load testing! 🎉

---

## 📋 Test Configuration Cheat Sheet

### Thread Group Settings

| Scenario | Threads | Ramp-Up | Loops | Total Requests |
|----------|---------|---------|-------|----------------|
| **Smoke Test** | 1 | 1s | 1 | 1 |
| **Light Load** | 10 | 10s | 5 | 50 |
| **Moderate** | 50 | 20s | 10 | 500 |
| **Heavy Load** | 100 | 30s | 10 | 1,000 |
| **Stress Test** | 200 | 60s | 10 | 2,000 |

---

## 🎯 STMS Endpoints to Test

### Copy-paste these paths into HTTP Requests:

```
# Tournaments
GET /api/tournaments
GET /api/tournaments/1
POST /api/tournaments
PUT /api/tournaments/1
DELETE /api/tournaments/1

# Leaderboard (MOST IMPORTANT!)
GET /api/leaderboard/1
GET /api/leaderboard/1?gender=male
GET /api/leaderboard/1?gender=female

# Players
GET /api/tournaments/1/players
GET /api/universities/1/players
GET /api/players/1
POST /api/universities/1/players
PUT /api/players/1
DELETE /api/players/1

# Universities
GET /api/tournaments/1/universities
GET /api/universities/1
POST /api/tournaments/1/universities
PUT /api/universities/1
DELETE /api/universities/1

# Events
GET /api/tournaments/1/events
GET /api/events/1
POST /api/tournaments/1/events
PUT /api/events/1
DELETE /api/events/1

# Timings
GET /api/timings/1/1
POST /api/timings
```

---

## 📊 Reading Results

### Summary Report - What to Look For:

| Metric | Good | Warning | Bad |
|--------|------|---------|-----|
| **Error %** | 0% | 1-5% | >5% |
| **Average (ms)** | <500 | 500-2000 | >2000 |
| **Throughput (req/s)** | >10 | 5-10 | <5 |
| **Min (ms)** | <100 | 100-500 | >500 |
| **Max (ms)** | <2000 | 2000-5000 | >5000 |

---

## ⚡ JMeter GUI Navigation

### Add Elements (Right-click):
```
Test Plan
  → Add → Threads → Thread Group
    → Add → Config Element → HTTP Request Defaults
    → Add → Sampler → HTTP Request
    → Add → Listener → View Results Tree
    → Add → Listener → Summary Report
    → Add → Timer → Constant Timer
    → Add → Assertions → Response Assertion
```

### Keyboard Shortcuts:
- `Ctrl + S` - Save
- `Ctrl + R` - Run test
- `Ctrl + .` - Stop test
- `Ctrl + E` - Clear results

---

## 🐛 Quick Fixes

### Problem: Connection Refused
```cmd
# Solution: Start your backend!
cd Backend\STMS.Api
dotnet run
```

### Problem: 404 Not Found
```
Check: HTTP Request path matches your API routes
Example: /api/tournaments NOT /tournaments
```

### Problem: Slow Performance
```
1. Check: Number of users (reduce if needed)
2. Check: Database queries (add indexes)
3. Check: Ramp-up period (increase gradually)
```

---

## 💾 Save & Run

### Save Test Plan:
```
File → Save Test Plan As → STMS_Load_Test.jmx
```

### Run from GUI:
```
Click ▶ button
```

### Run from Command Line:
```cmd
cd C:\JMeter\apache-jmeter-5.6.3\bin
jmeter -n -t "path\to\STMS_Load_Test.jmx" -l results.jtl
```

### Generate HTML Report:
```cmd
jmeter -n -t test.jmx -l results.jtl -e -o reports
```

---

## 📈 Progressive Testing Strategy

```
Day 1: Start small
- 1 user, 1 loop (smoke test)
- Fix any errors

Day 2: Light load
- 10 users, 5 loops
- Record baseline

Day 3: Increase gradually
- 50 users, 10 loops
- Monitor performance

Day 4: Stress test
- 100+ users
- Find limits
```

---

## ✅ Pre-Test Checklist

- [ ] Backend running (`dotnet run`)
- [ ] Swagger accessible (http://localhost:5000/swagger)
- [ ] Database connected
- [ ] JMeter opened
- [ ] Test plan created
- [ ] HTTP defaults set (localhost:5000)
- [ ] Listeners added

---

## 📞 Need Help?

**Full Guide**: `JMETER_LOAD_TESTING_GUIDE.md`
**JMeter Docs**: https://jmeter.apache.org/usermanual/

---

**Ready? Start Backend → Open JMeter → Add Requests → Run Test! 🚀**
