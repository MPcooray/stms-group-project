# 🎯 JMeter Load Testing - Visual Step-by-Step

## 📥 STEP 1: Get JMeter ✅ ALREADY DONE!

```
┌─────────────────────────────────────────┐
│ ✓ JMeter Downloaded!                   │
│ ✓ Location: C:\Users\Cooray\Downloads\ │
│      apache-jmeter-5.6.3\              │
│      apache-jmeter-5.6.3\bin\          │
│ ✓ Ready to use!                        │
└─────────────────────────────────────────┘
```

---

## 🚀 STEP 2: Start Backend (1 minute)

```cmd
cd c:\Users\Cooray\OneDrive\Desktop\stms\Backend\STMS.Api
dotnet run --no-launch-profile --urls http://localhost:5000
```

Wait for this:
```
✓ Now listening on: http://localhost:5000
✓ Now listening on: https://localhost:5001
```

Test it: Open http://localhost:5000/swagger ✅

---

## 🎨 STEP 3: Open JMeter (1 minute)

```cmd
cd C:\Users\Cooray\Downloads\apache-jmeter-5.6.3\apache-jmeter-5.6.3\bin
jmeter.bat
```

You'll see this window:
```
┌────────────────────────────────────────┐
│  Apache JMeter                    _ □ X│
├────────────────────────────────────────┤
│  File Edit Run Options Help            │
├────────────────────────────────────────┤
│  ▶ ⏸ ⏹                                 │
├────────────────────────────────────────┤
│  Test Plan                             │
│    → WorkBench                         │
└────────────────────────────────────────┘
```

---

## 🔧 STEP 4: Build Test Plan (5 minutes)

### 4.1 Add Thread Group

```
Right-click "Test Plan"
  └─> Add
      └─> Threads (Users)
          └─> Thread Group ✓
```

**Configure Thread Group:**
```
┌──────────────────────────────┐
│ Name: STMS Users             │
│ Number of Threads: 10        │
│ Ramp-Up Period: 10           │
│ Loop Count: 5                │
└──────────────────────────────┘
```

### 4.2 Add HTTP Defaults

```
Right-click "Thread Group"
  └─> Add
      └─> Config Element
          └─> HTTP Request Defaults ✓
```

**Configure HTTP Defaults:**
```
┌──────────────────────────────┐
│ Protocol: http               │
│ Server Name: localhost       │
│ Port Number: 5000            │
└──────────────────────────────┘
```

Note: If you don't use the script and the console shows a different port (e.g., `Now listening on: http://localhost:5287`), put that port here instead of 5000.

### 4.3 Add HTTP Request

```
Right-click "Thread Group"
  └─> Add
      └─> Sampler
          └─> HTTP Request ✓
```

**Configure HTTP Request:**
```
┌──────────────────────────────┐
│ Name: GET Tournaments        │
│ Method: GET                  │
│ Path: /api/tournaments       │
└──────────────────────────────┘
```

### 4.4 Add Listeners

```
Right-click "Thread Group"
  └─> Add
      └─> Listener
          ├─> View Results Tree ✓
          ├─> Summary Report ✓
          └─> Graph Results ✓
```

---

## 🎬 STEP 5: Run Test! (30 seconds)

### Your Test Plan Should Look Like:

```
STMS Load Test
└─ Thread Group
   ├─ HTTP Request Defaults
   ├─ GET Tournaments
   ├─ View Results Tree
   ├─ Summary Report
   └─ Graph Results
```

### Click the Play Button ▶

Watch it run! 🎉

---

## 📊 STEP 6: View Results

### View Results Tree:
```
┌────────────────────────────────────┐
│ ✓ GET Tournaments [200]           │
│   Response code: 200               │
│   Response time: 127 ms            │
│   Response data:                   │
│   [{"id":1,"name":"Tournament"}]   │
└────────────────────────────────────┘
```

### Summary Report:
```
┌─────────────────────────────────────────────┐
│ Label        │ Samples │ Avg │ Error%      │
├─────────────────────────────────────────────┤
│ Tournaments  │   50    │ 150 │   0.0%  ✓  │
└─────────────────────────────────────────────┘
```

---

## 🎯 Add More Tests (Copy-Paste!)

### Test 1: Leaderboard
```
Name: GET Leaderboard
Method: GET
Path: /api/leaderboard/1
```

### Test 2: Leaderboard with Filter
```
Name: GET Leaderboard Male
Method: GET
Path: /api/leaderboard/1?gender=male
```

### Test 3: Players
```
Name: GET Players
Method: GET
Path: /api/tournaments/1/players
```

### Test 4: Universities
```
Name: GET Universities
Method: GET
Path: /api/tournaments/1/universities
```

---

## 📈 Increase Load Gradually

### Start Small:
```
Threads: 10
Ramp-Up: 10 seconds
Loops: 5
Total: 50 requests
```

### Then Increase:
```
Threads: 50   → 100   → 200
Ramp-Up: 20s  → 30s   → 60s
Loops: 10     → 10    → 10
Total: 500    → 1,000 → 2,000
```

---

## ✅ Success Criteria

Good Test Results:
```
✓ Error % = 0%
✓ Average < 500ms
✓ Max < 2000ms
✓ Throughput > 10 req/sec
```

Warning Signs:
```
⚠ Error % > 5%
⚠ Average > 1000ms
⚠ Max > 5000ms
⚠ Throughput < 5 req/sec
```

---

## 💾 Save Your Work

### Save Test Plan:
```
File → Save Test Plan As
Name: STMS_Load_Test.jmx
Location: Backend\STMS.Api.Tests\
```

### Export Results:
```
Tools → Generate HTML Report
```

---

## 🐛 Troubleshooting

### ❌ Connection Refused
```
Problem: Can't connect to localhost:5000
Solution: 
  1. Check backend is running (dotnet run)
  2. Check http://localhost:5000/swagger works
```

### ❌ 404 Not Found
```
Problem: All requests return 404
Solution:
  1. Check path starts with /api/
  2. Example: /api/tournaments NOT /tournaments
```

### ❌ Red Errors in Results
```
Problem: Requests failing
Solution:
  1. Check View Results Tree for error details
  2. Test endpoint in browser first
  3. Check database is running
```

---

## 🎓 JMeter Structure Explained

```
Test Plan (Your project)
  │
  └─ Thread Group (Users)
      │
      ├─ Config Elements (Settings)
      │   └─ HTTP Request Defaults
      │
      ├─ Samplers (Actions)
      │   ├─ HTTP Request 1
      │   ├─ HTTP Request 2
      │   └─ HTTP Request 3
      │
      └─ Listeners (Results)
          ├─ View Results Tree
          ├─ Summary Report
          └─ Graph Results
```

---

## 📱 Test Different Scenarios

### Scenario 1: Normal Usage (Office Hours)
```
10-50 users
Steady traffic
All endpoints equally
```

### Scenario 2: Event Day (Tournament Day)
```
100-200 users
Heavy leaderboard requests
Focus on GET /api/leaderboard
```

### Scenario 3: Admin Operations
```
5-10 users
POST/PUT/DELETE requests
Create tournaments, add players
```

---

## 🚀 Quick Commands Summary

```cmd
# Start Backend
cd c:\Users\Cooray\OneDrive\Desktop\stms\Backend\STMS.Api
dotnet run

# Open JMeter GUI
cd C:\Users\Cooray\Downloads\apache-jmeter-5.6.3\apache-jmeter-5.6.3\bin
jmeter.bat

# Run Test (No GUI - Faster!)
jmeter -n -t test.jmx -l results.jtl

# Generate Report
jmeter -n -t test.jmx -l results.jtl -e -o reports
```

---

## 🎯 Your First Test Checklist

- [ ] JMeter downloaded & extracted
- [ ] Backend running (dotnet run)
- [ ] JMeter opened (jmeter.bat)
- [ ] Test Plan created
- [ ] Thread Group added (10 users, 10s, 5 loops)
- [ ] HTTP Defaults set (localhost:5000)
- [ ] HTTP Request added (/api/tournaments)
- [ ] Listeners added (View Results Tree, Summary)
- [ ] Test saved (STMS_Load_Test.jmx)
- [ ] Test run successfully ▶
- [ ] Results analyzed ✓

---

## 📚 Files Created for You

1. **JMETER_LOAD_TESTING_GUIDE.md** ← Full detailed guide
2. **JMETER_QUICK_START.md** ← Quick reference
3. **JMETER_VISUAL_GUIDE.md** ← This file!

---

## 🎉 You're Ready!

```
1. Start Backend     ✓
2. Open JMeter       ✓
3. Create Test Plan  ✓
4. Add Requests      ✓
5. Run & Analyze     ✓
6. Celebrate! 🎊     ✓
```

**Good luck with your load testing! 🚀**
