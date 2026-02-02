const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  // Monitor all requests
  page.on('request', request => {
    const url = request.url();
    if (url.includes('api') || url.includes('auth')) {
      console.log('REQUEST:', request.method(), url);
    }
  });
  
  page.on('response', response => {
    const url = response.url();
    if (url.includes('api') || url.includes('auth')) {
      console.log('RESPONSE:', response.status(), url);
    }
  });
  
  page.on('requestfailed', request => {
    console.log('FAILED:', request.url(), request.failure()?.errorText);
  });
  
  try {
    console.log('Loading login page...');
    await page.goto('http://20.245.40.183/login', { waitUntil: 'networkidle', timeout: 30000 });
    console.log('Page loaded');
    
    // Wait a bit for React to render
    await page.waitForTimeout(2000);
    
    // Try multiple selectors for email field
    const emailSelector = 'input[name="email"], input[type="email"], input[id*="email"], input[placeholder*="mail"]';
    const passwordSelector = 'input[name="password"], input[type="password"], input[id*="password"]';
    
    // Wait for email input
    await page.waitForSelector(emailSelector, { timeout: 10000 });
    console.log('Email field found');
    
    // Fill the form
    await page.fill(emailSelector, 'admin@crm.local');
    await page.fill(passwordSelector, 'Admin@123');
    
    console.log('Filled form, clicking submit...');
    
    // Click submit
    await page.click('button[type="submit"]');
    
    // Wait for network activity
    await page.waitForTimeout(5000);
    
    // Check current URL
    const currentUrl = await page.url();
    console.log('Final URL:', currentUrl);
    
    if (currentUrl.includes('/dashboard') || !currentUrl.includes('/login')) {
      console.log('SUCCESS: Login appears to have worked!');
    } else {
      console.log('Login may have failed - still on login page');
    }
    
    console.log('Done');
  } catch (e) {
    console.log('Error:', e.message);
    // Take a screenshot for debugging
    await page.screenshot({ path: 'login-error.png' });
    console.log('Screenshot saved to login-error.png');
  }
  
  await browser.close();
})();
