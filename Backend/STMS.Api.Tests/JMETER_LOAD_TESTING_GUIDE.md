# 🚀 STMS Load Testing with JMeter - Complete Step-by-Step Guide

## 📋 Table of Contents
1. [Prerequisites](#prerequisites)
2. [JMeter Installation](#jmeter-installation)
3. [Start Your STMS Backend](#start-stms-backend)
4. [Create Test Plan](#create-test-plan)
5. [Test Scenarios](#test-scenarios)
6. [Run Tests](#run-tests)
7. [Analyze Results](#analyze-results)

---

## ✅ Prerequisites

Before starting, you need:
- ✓ Java installed (you already have JavaSE-23)
- ✓ STMS Backend API running
- ✓ SQL Server database running

---

## 📥 Step 1: Install JMeter

### Download JMeter

1. **Go to**: https://jmeter.apache.org/download_jmeter.cgi
2. **Download**: Apache JMeter 5.6.3 (or latest)
   - Windows: `apache-jmeter-5.6.3.zip`
3. **Extract** to: `C:\JMeter` (or any folder you prefer)

### Verify Installation

```cmd
cd C:\JMeter\apache-jmeter-5.6.3\bin
jmeter.bat
```

✅ JMeter GUI should open!

---

## 🚀 Step 2: Start Your STMS Backend

### Terminal 1: Start Backend API

```cmd
cd c:\Users\Cooray\OneDrive\Desktop\stms\Backend\STMS.Api
dotnet run
```

**Wait for**:
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

✅ Your API is running!

### Test Backend is Working

Open browser: http://localhost:5000/swagger

You should see Swagger UI with all your endpoints.

---

## 🧪 Step 3: Create Your First JMeter Test Plan

### 3.1 Open JMeter

```cmd
cd C:\JMeter\apache-jmeter-5.6.3\bin
jmeter.bat
```

### 3.2 Create Test Plan Structure

1. **Right-click** on "Test Plan" → **Rename** → `STMS Load Test`

2. **Add Thread Group** (Users):
   - Right-click "Test Plan" → Add → Threads (Users) → Thread Group
   - Rename to: `STMS API Users`
   - Configure:
     - **Number of Threads (users)**: `10`
     - **Ramp-Up Period (seconds)**: `10`
     - **Loop Count**: `5`
   - Click **Apply**

   > This means: 10 concurrent users, ramping up over 10 seconds, each making 5 requests = 50 total requests

3. **Add HTTP Request Defaults**:
   - Right-click "Thread Group" → Add → Config Element → HTTP Request Defaults
   - Configure:
     - **Protocol**: `http`
     - **Server Name or IP**: `localhost`
     - **Port Number**: `5000`
   - Click **Apply**

4. **Add Listeners** (to see results):
   
   a. **View Results Tree**:
   - Right-click "Thread Group" → Add → Listener → View Results Tree
   
   b. **Summary Report**:
   - Right-click "Thread Group" → Add → Listener → Summary Report
   
   c. **Graph Results**:
   - Right-click "Thread Group" → Add → Listener → Graph Results

---

## 📊 Step 4: Add Your API Tests

### Test 1: GET All Tournaments

1. **Right-click "Thread Group"** → Add → Sampler → HTTP Request
2. **Rename** to: `GET - All Tournaments`
3. **Configure**:
   - **Method**: `GET`
   - **Path**: `/api/tournaments`
4. **Click Apply**

### Test 2: GET Tournament by ID

1. **Right-click "Thread Group"** → Add → Sampler → HTTP Request
2. **Rename** to: `GET - Tournament by ID`
3. **Configure**:
   - **Method**: `GET`
   - **Path**: `/api/tournaments/1`
4. **Click Apply**

### Test 3: GET Leaderboard (Most Important!)

1. **Right-click "Thread Group"** → Add → Sampler → HTTP Request
2. **Rename** to: `GET - Leaderboard`
3. **Configure**:
   - **Method**: `GET`
   - **Path**: `/api/leaderboard/1`
4. **Click Apply**

### Test 4: GET Leaderboard with Gender Filter

1. **Right-click "Thread Group"** → Add → Sampler → HTTP Request
2. **Rename** to: `GET - Leaderboard Male`
3. **Configure**:
   - **Method**: `GET`
   - **Path**: `/api/leaderboard/1?gender=male`
4. **Click Apply**

### Test 5: GET Players by Tournament

1. **Right-click "Thread Group"** → Add → Sampler → HTTP Request
2. **Rename** to: `GET - Players by Tournament`
3. **Configure**:
   - **Method**: `GET`
   - **Path**: `/api/tournaments/1/players`
4. **Click Apply**

---

## 🎯 Step 5: Run Your Tests

### Save Your Test Plan First!

1. **File** → **Save Test Plan As**
2. Save as: `STMS_Load_Test.jmx` in your STMS project folder

### Run the Test

1. Click the **Green Play Button** (▶) at the top
2. Watch the tests run in "View Results Tree"
3. Check "Summary Report" for statistics

### What to Look For:

✅ **All requests should be GREEN** (success)
❌ **RED requests** = errors

---

## 📈 Step 6: Increase Load Gradually

### Test Scenario 1: Light Load (Baseline)
```
Number of Threads: 10
Ramp-Up Period: 10 seconds
Loop Count: 5
Total Requests: 50
```

### Test Scenario 2: Moderate Load
```
Number of Threads: 50
Ramp-Up Period: 20 seconds
Loop Count: 10
Total Requests: 500
```

### Test Scenario 3: Heavy Load
```
Number of Threads: 100
Ramp-Up Period: 30 seconds
Loop Count: 10
Total Requests: 1,000
```

### Test Scenario 4: Stress Test
```
Number of Threads: 200
Ramp-Up Period: 60 seconds
Loop Count: 10
Total Requests: 2,000
```

### How to Change:

1. Select "Thread Group"
2. Update the values
3. Click **Apply**
4. Run again

---

## 📊 Step 7: Analyze Results

### Summary Report Columns:

| Column | What It Means | Good Value |
|--------|---------------|------------|
| **Samples** | Total requests | Should match expected |
| **Average** | Average response time (ms) | < 500ms |
| **Min** | Fastest response (ms) | < 100ms |
| **Max** | Slowest response (ms) | < 2000ms |
| **Std. Dev** | Response time variation | Low = consistent |
| **Error %** | Failed requests % | 0% |
| **Throughput** | Requests/second | Higher = better |

### What to Check:

✅ **Error % = 0%** (all requests succeeded)
✅ **Average < 500ms** (fast responses)
✅ **Throughput > 10/sec** (good performance)

---

## 🎯 Important STMS Endpoints to Test

### Priority 1 (Most Important):
```
GET /api/leaderboard/{tournamentId}
GET /api/leaderboard/{tournamentId}?gender=male
GET /api/leaderboard/{tournamentId}?gender=female
```

### Priority 2 (High Traffic):
```
GET /api/tournaments
GET /api/tournaments/{id}
GET /api/tournaments/{tournamentId}/players
GET /api/universities/{universityId}/players
```

### Priority 3 (Admin Operations):
```
POST /api/tournaments
PUT /api/tournaments/{id}
DELETE /api/tournaments/{id}
POST /api/timings
```

---

## 🔧 Advanced JMeter Features

### Add Assertions (Verify Response)

1. **Right-click on HTTP Request** → Add → Assertions → Response Assertion
2. **Configure**:
   - **Field to Test**: Response Code
   - **Pattern Matching**: Equals
   - **Patterns to Test**: `200`
3. This ensures response is successful

### Add Think Time (More Realistic)

1. **Right-click on Thread Group** → Add → Timer → Constant Timer
2. **Configure**:
   - **Thread Delay (ms)**: `1000` (1 second pause between requests)

### Extract Data (Chain Requests)

1. **Right-click on HTTP Request** → Add → Post Processors → JSON Extractor
2. **Extract tournament ID** from response:
   - **Variable Name**: `tournamentId`
   - **JSON Path**: `$[0].id`
3. **Use in next request**: `/api/tournaments/${tournamentId}`

---

## 📁 Save Your Test Results

### Generate HTML Report

1. After test completes, go to: **Tools** → **Generate HTML Report**
2. Or run from command line:

```cmd
cd C:\JMeter\apache-jmeter-5.6.3\bin

jmeter -n -t "path\to\STMS_Load_Test.jmx" -l results.jtl -e -o reports
```

This creates a detailed HTML report in the `reports` folder.

---

## 🎯 Recommended Testing Strategy

### Day 1: Baseline Testing
1. Test with **10 users, 5 loops**
2. Record baseline metrics
3. Fix any errors

### Day 2: Moderate Load
1. Test with **50 users, 10 loops**
2. Compare to baseline
3. Check response times

### Day 3: Heavy Load
1. Test with **100 users, 10 loops**
2. Monitor system resources (CPU, memory)
3. Identify bottlenecks

### Day 4: Stress Test
1. Test with **200+ users**
2. Find breaking point
3. Document maximum capacity

---

## 🐛 Common Issues & Solutions

### Issue 1: Connection Refused
**Error**: `java.net.ConnectException: Connection refused`
**Solution**: Make sure your STMS API is running (`dotnet run`)

### Issue 2: All Requests Fail (404)
**Error**: `404 Not Found`
**Solution**: Check your endpoint paths in JMeter match your API routes

### Issue 3: Slow Response Times
**Error**: Average > 2000ms
**Solution**: 
- Check database queries
- Add indexes to database
- Reduce concurrent users

### Issue 4: Memory Errors
**Error**: Out of memory
**Solution**: Increase JMeter heap size:
```cmd
# Edit jmeter.bat and add:
set HEAP=-Xms1g -Xmx4g
```

---

## 📊 Sample Test Plan Structure

```
STMS Load Test
├── Thread Group (10 users, 10s ramp-up, 5 loops)
│   ├── HTTP Request Defaults (localhost:5000)
│   ├── GET - All Tournaments
│   ├── GET - Tournament by ID
│   ├── GET - Leaderboard
│   ├── GET - Leaderboard (Male)
│   ├── GET - Leaderboard (Female)
│   ├── GET - Players by Tournament
│   ├── GET - Timings
│   └── Listeners
│       ├── View Results Tree
│       ├── Summary Report
│       ├── Graph Results
│       └── Aggregate Report
```

---

## ✅ Checklist

- [ ] JMeter installed
- [ ] STMS Backend running
- [ ] Test plan created
- [ ] HTTP Request Defaults configured
- [ ] All important endpoints added
- [ ] Listeners added (View Results Tree, Summary Report)
- [ ] Baseline test run (10 users)
- [ ] Results analyzed
- [ ] Test plan saved
- [ ] HTML report generated

---

## 🎯 Quick Start Commands

```cmd
# 1. Start Backend
cd c:\Users\Cooray\OneDrive\Desktop\stms\Backend\STMS.Api
dotnet run

# 2. Open JMeter (in new terminal)
cd C:\JMeter\apache-jmeter-5.6.3\bin
jmeter.bat

# 3. Run test from command line (optional)
jmeter -n -t STMS_Load_Test.jmx -l results.jtl
```

---

## 📚 Next Steps

1. ✅ Install JMeter
2. ✅ Create basic test plan (10 users)
3. ✅ Test all GET endpoints
4. ✅ Gradually increase load
5. ✅ Generate HTML reports
6. ⭐ Optimize slow endpoints
7. ⭐ Add POST/PUT/DELETE tests (if needed)

---

**Good luck with your load testing! 🚀**

Need help? Check JMeter docs: https://jmeter.apache.org/usermanual/
