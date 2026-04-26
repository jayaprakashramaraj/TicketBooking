const MODE = (process.env.MODE || 'docker').toLowerCase();
const CONCURRENT_USERS = Number(process.env.CONCURRENT_USERS || 100);
const SEATS = (process.env.SEATS || '2B,2C')
  .split(',')
  .map(seat => seat.trim())
  .filter(Boolean);
const CLEANUP = process.env.CLEANUP !== 'false';

const endpoints = MODE === 'docker'
  ? {
      catalog: process.env.CATALOG_API_URL || 'http://localhost:5002',
      booking: process.env.BOOKING_API_URL || 'http://localhost:5002',
      paymentSimulator: process.env.PAYMENT_SIMULATOR_URL || 'http://localhost:5002'
    }
  : {
      catalog: process.env.CATALOG_API_URL || 'http://localhost:7001',
      booking: process.env.BOOKING_API_URL || 'http://localhost:7002',
      paymentSimulator: process.env.PAYMENT_SIMULATOR_URL || 'http://localhost:7005'
    };

async function fetchJson(url, options) {
  let response;
  try {
    response = await fetch(url, options);
  } catch (error) {
    throw new Error(`Request failed for ${url}: ${error.message}`);
  }

  const text = await response.text();
  const body = text ? JSON.parse(text) : null;
  return { response, body };
}

async function resolveShow() {
  if (process.env.SHOW_ID) {
    const showName = process.env.SHOW_NAME || 'Load Test Movie';
    const showTime = process.env.SHOW_TIME || new Date().toISOString();
    const price = Number(process.env.SHOW_PRICE || 100);

    return {
      id: process.env.SHOW_ID,
      movieName: showName,
      startTime: showTime,
      price
    };
  }

  const { response, body } = await fetchJson(`${endpoints.catalog}/api/shows/search`);
  if (!response.ok) {
    throw new Error(`Could not load shows from Catalog API. HTTP ${response.status}`);
  }

  if (!Array.isArray(body) || body.length === 0) {
    throw new Error('Catalog API returned no shows. Start Catalog API and seed data first.');
  }

  return body[0];
}

async function attemptBooking(userId, show) {
  try {
    const response = await fetch(`${endpoints.booking}/api/bookings`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        showId: show.id,
        showName: show.movieName,
        showTime: show.startTime,
        customerEmail: `load-user-${Date.now()}-${userId}@test.local`,
        seatNumbers: SEATS,
        totalAmount: Number(show.price || 100) * SEATS.length
      })
    });

    const text = await response.text();
    let body = null;
    try {
      body = text ? JSON.parse(text) : null;
    } catch {
      body = text;
    }

    return {
      userId,
      status: response.status,
      bookingId: body?.bookingId,
      body
    };
  } catch (error) {
    return {
      userId,
      status: 'Error',
      error: error.message
    };
  }
}

async function cleanupAcceptedBookings(results) {
  if (!CLEANUP) return;

  const bookingIds = results
    .filter(result => result.status === 202 && result.bookingId)
    .map(result => result.bookingId);

  if (bookingIds.length === 0) return;

  console.log(`\nCleaning up ${bookingIds.length} accepted booking(s) through the payment simulator...`);

  await Promise.all(bookingIds.map(async bookingId => {
    try {
      await waitForBookingMaterialized(bookingId);

      const response = await fetch(`${endpoints.paymentSimulator}/api/payment/complete`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ bookingId, status: 'Cancel' })
      });

      console.log(`  ${bookingId}: cleanup HTTP ${response.status}`);
    } catch (error) {
      console.log(`  ${bookingId}: cleanup failed - ${error.message}`);
    }
  }));
}

async function waitForBookingMaterialized(bookingId) {
  const deadline = Date.now() + 15000;

  while (Date.now() < deadline) {
    try {
      const { response, body } = await fetchJson(`${endpoints.booking}/api/bookings/${bookingId}/status`);
      if (response.ok && body?.status && body.status !== 'Processing') {
        return;
      }
    } catch {
      // Keep polling; the cleanup path should not fail because one status call raced startup/persistence.
    }

    await new Promise(resolve => setTimeout(resolve, 500));
  }
}

function printResults(results, elapsedMs, show) {
  const successful = results.filter(result => result.status === 202).length;
  const conflicts = results.filter(result => result.status === 409).length;
  const errors = results.filter(result => result.status !== 202 && result.status !== 409).length;

  console.log('\n--- Load Test Results ---');
  console.log(`Mode: ${MODE}`);
  console.log(`Show: ${show.movieName} (${show.id})`);
  console.log(`Seats: ${SEATS.join(', ')}`);
  console.log(`Total Requests: ${CONCURRENT_USERS}`);
  console.log(`Successful reservations (202): ${successful}`);
  console.log(`Conflicts (409): ${conflicts}`);
  console.log(`Other errors: ${errors}`);
  console.log(`Total Time: ${elapsedMs}ms`);

  if (errors > 0) {
    console.log('\nUnexpected responses:');
    results
      .filter(result => result.status !== 202 && result.status !== 409)
      .slice(0, 10)
      .forEach(result => {
        console.log(`  user ${result.userId}: ${result.status} ${JSON.stringify(result.body || result.error || '')}`);
      });
  }

  if (successful === 1) {
    console.log('\nPASS: only one concurrent request reserved the selected seats.');
  } else if (successful > 1) {
    console.log('\nFAIL: more than one request reserved the same seats.');
  } else {
    console.log('\nNo request reserved the seats. Check API health, selected seats, and existing booked seats.');
  }
}

async function runTest() {
  if (SEATS.length === 0) {
    throw new Error('Provide at least one seat through SEATS, for example SEATS=2B,2C.');
  }

  const show = await resolveShow();
  console.log(`Starting load test with ${CONCURRENT_USERS} users against ${MODE} APIs...`);

  const startTime = Date.now();
  const results = await Promise.all(
    Array.from({ length: CONCURRENT_USERS }, (_, index) => attemptBooking(index + 1, show))
  );
  const elapsedMs = Date.now() - startTime;

  printResults(results, elapsedMs, show);
  await cleanupAcceptedBookings(results);
}

runTest().catch(error => {
  console.error(`Load test failed: ${error.message}`);
  process.exitCode = 1;
});
