const API_URL = 'http://localhost:5002/api/bookings';
const SHOW_ID = 'a389231c-76b6-4e67-93ae-56255cfde6c8'; // Replace with a real Show ID from your DB
const CONCURRENT_USERS = 50;
const SEATS = ['B1', 'B2'];

async function attemptBooking(userId) {
  try {
    const response = await fetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        showId: SHOW_ID,
        showName: 'Load Test Movie',
        showTime: new Date().toISOString(),
        customerEmail: `user${userId}@test.com`,
        seatNumbers: SEATS,
        totalAmount: 200
      })
    });
    return { userId, status: response.status };
  } catch (error) {
    return { userId, status: 'Error' };
  }
}

async function runTest() {
  console.log(`🚀 Starting load test with ${CONCURRENT_USERS} users for seats ${SEATS.join(',')}...`);
  
  const startTime = Date.now();
  const promises = [];
  
  for (let i = 1; i <= CONCURRENT_USERS; i++) {
    promises.push(attemptBooking(i));
  }
  
  const results = await Promise.all(promises);
  const endTime = Date.now();
  
  const successful = results.filter(r => r.status === 202).length;
  const conflicts = results.filter(r => r.status === 409).length;
  const errors = results.filter(r => r.status !== 202 && r.status !== 409).length;
  
  console.log('\n--- Load Test Results ---');
  console.log(`Total Requests: ${CONCURRENT_USERS}`);
  console.log(`✅ Successful (202 Accepted): ${successful}`);
  console.log(`❌ Conflicts (409 Conflict): ${conflicts}`);
  console.log(`⚠️ Other Errors: ${errors}`);
  console.log(`⏱️ Total Time: ${endTime - startTime}ms`);
  
  if (successful === 1) {
    console.log('\n💎 SUCCESS: Redis Distributed Lock worked perfectly! Only 1 user reserved the seats.');
  } else if (successful > 1) {
    console.log('\n🔥 FAILURE: Double booking occurred!');
  } else {
    console.log('\n❓ No bookings succeeded. Is the API running and Show ID correct?');
  }
}

runTest();
