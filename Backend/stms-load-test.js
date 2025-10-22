import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Load test configuration
export const options = {
  stages: [
    { duration: '30s', target: 10 },   // Warm up: 10 users
    { duration: '1m', target: 30 },    // Ramp up: 30 users
    { duration: '2m', target: 50 },    // Normal load: 50 users
    { duration: '1m', target: 100 },   // Peak load: 100 users
    { duration: '30s', target: 0 },    // Cool down
  ],
  thresholds: {
    'http_req_duration': ['p(95)<1000'],      // 95% of requests under 1s
    'http_req_duration{name:leaderboard}': ['p(95)<2000'], // Leaderboard under 2s
    'http_req_failed': ['rate<0.05'],         // Less than 5% errors
    'errors': ['rate<0.1'],                   // Less than 10% custom errors
  },
};

// Configuration - UPDATE THESE!
const BASE_URL = 'https://localhost:5001/api';  // Your API URL
const TOURNAMENT_ID = 1;  // Use an existing tournament ID

export default function () {
  // Group 1: Tournament Operations (Light queries)
  group('Tournament Operations', function () {
    let res = http.get(`${BASE_URL}/tournaments`, {
      tags: { name: 'tournaments' },
    });
    
    check(res, {
      'tournaments: status is 200': (r) => r.status === 200,
      'tournaments: response time < 500ms': (r) => r.timings.duration < 500,
      'tournaments: has data': (r) => r.body.length > 0,
    }) || errorRate.add(1);
    
    sleep(1);
  });
  
  // Group 2: Leaderboard Operations (Heavy queries - Most important!)
  group('Leaderboard Operations', function () {
    // Test leaderboard without filter
    let res1 = http.get(`${BASE_URL}/leaderboard/${TOURNAMENT_ID}`, {
      tags: { name: 'leaderboard' },
    });
    
    check(res1, {
      'leaderboard: status is 200': (r) => r.status === 200,
      'leaderboard: response time < 2s': (r) => r.timings.duration < 2000,
      'leaderboard: has players': (r) => {
        try {
          const data = JSON.parse(r.body);
          return data.players && data.players.length > 0;
        } catch {
          return false;
        }
      },
    }) || errorRate.add(1);
    
    sleep(2);
    
    // Test leaderboard with gender filter (Male)
    let res2 = http.get(`${BASE_URL}/leaderboard/${TOURNAMENT_ID}?gender=male`, {
      tags: { name: 'leaderboard_filtered' },
    });
    
    check(res2, {
      'leaderboard (male): status is 200': (r) => r.status === 200,
      'leaderboard (male): response time < 2s': (r) => r.timings.duration < 2000,
    }) || errorRate.add(1);
    
    sleep(1);
    
    // Test leaderboard with gender filter (Female)
    let res3 = http.get(`${BASE_URL}/leaderboard/${TOURNAMENT_ID}?gender=female`, {
      tags: { name: 'leaderboard_filtered' },
    });
    
    check(res3, {
      'leaderboard (female): status is 200': (r) => r.status === 200,
    }) || errorRate.add(1);
    
    sleep(1);
  });
  
  // Group 3: Player Operations
  group('Player Operations', function () {
    let res = http.get(`${BASE_URL}/tournaments/${TOURNAMENT_ID}/players`, {
      tags: { name: 'players' },
    });
    
    check(res, {
      'players: status is 200': (r) => r.status === 200,
      'players: response time < 1s': (r) => r.timings.duration < 1000,
    }) || errorRate.add(1);
    
    sleep(1);
  });
  
  // Group 4: University Operations
  group('University Operations', function () {
    let res = http.get(`${BASE_URL}/tournaments/${TOURNAMENT_ID}/universities`, {
      tags: { name: 'universities' },
    });
    
    check(res, {
      'universities: status is 200': (r) => r.status === 200,
      'universities: response time < 500ms': (r) => r.timings.duration < 500,
    }) || errorRate.add(1);
    
    sleep(1);
  });
  
  // Group 5: Event Operations
  group('Event Operations', function () {
    let res = http.get(`${BASE_URL}/tournaments/${TOURNAMENT_ID}/events`, {
      tags: { name: 'events' },
    });
    
    check(res, {
      'events: status is 200': (r) => r.status === 200,
    }) || errorRate.add(1);
    
    sleep(1);
  });
}

// Setup function (runs once at the beginning)
export function setup() {
  console.log('🚀 Starting STMS Load Test...');
  console.log(`📍 Base URL: ${BASE_URL}`);
  console.log(`🎯 Tournament ID: ${TOURNAMENT_ID}`);
  
  // Verify API is reachable
  let res = http.get(`${BASE_URL}/tournaments`);
  if (res.status !== 200) {
    throw new Error(`❌ API not reachable! Status: ${res.status}`);
  }
  
  console.log('✅ API is reachable, starting load test...\n');
}

// Teardown function (runs once at the end)
export function teardown(data) {
  console.log('\n🏁 Load test completed!');
}
