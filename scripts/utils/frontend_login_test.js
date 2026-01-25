const { chromium } = require('playwright');
(async () => {
  const browser = await chromium.launch({ args: ['--no-sandbox'] });
  const page = await browser.newPage();
  try {
    await page.goto('http://host.docker.internal:3000', { waitUntil: 'networkidle' });
    // Try common selectors
    await page.fill('input[type="email"]', 'abhi.lal@gmail.com').catch(()=>{});
    await page.fill('input[type="password"]', 'Admin@123').catch(()=>{});
    // Wait for the login POST response while attempting submit
    const [response] = await Promise.all([
      page.waitForResponse(r => r.url().includes('/api/auth/login') && r.request().method() === 'POST', { timeout: 15000 }),
      page.click('button[type="submit"]').catch(async ()=>{
        // fallback button text
        await page.click('button:has-text("Sign In")').catch(()=>{});
      })
    ]);
    console.log('LOGIN_STATUS', response.status());
    const body = await response.text();
    console.log('LOGIN_BODY', body);
    const ls = await page.evaluate(()=> Object.fromEntries(Object.entries(localStorage)) );
    console.log('LOCAL_STORAGE', JSON.stringify(ls));
    const cookies = await page.context().cookies();
    console.log('COOKIES', JSON.stringify(cookies));
  } catch (err) {
    console.error('ERROR', err && err.message ? err.message : err);
    process.exitCode = 2;
  } finally {
    await browser.close();
  }
})();
