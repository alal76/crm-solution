/**
 * UI Test: Create Microsoft Corporation Account with 5 Top Executives
 * Adds comprehensive publicly available data about Microsoft and its leadership
 */

import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://localhost';
const API_URL = process.env.API_URL || (BASE_URL === 'http://localhost' ? 'http://localhost:5000' : BASE_URL);

// Microsoft Corporation comprehensive data (publicly available)
const microsoftAccount = {
  category: 1, // Organization
  company: 'Microsoft Corporation',
  industry: 'Technology',
  customerType: 0,
  status: 'Active',
  phone: '+1 (425) 882-8080',
  email: 'info@microsoft.com',
  website: 'https://www.microsoft.com',
  address: 'One Microsoft Way',
  city: 'Redmond',
  state: 'WA',
  zipCode: '98052',
  country: 'USA',
  notes: `Microsoft Corporation - Global Technology Leader

Founded: April 4, 1975
Founders: Bill Gates and Paul Allen
Headquarters: Redmond, Washington, USA

Business Overview:
Microsoft is one of the world's largest technology companies, known for developing, licensing, and supporting a wide range of software products, services, and devices. The company operates through three main segments:

1. Productivity and Business Processes
   - Microsoft 365 (formerly Office 365)
   - LinkedIn
   - Dynamics 365

2. Intelligent Cloud
   - Azure (cloud computing platform)
   - SQL Server
   - GitHub
   - Enterprise Services

3. More Personal Computing
   - Windows Operating Systems
   - Xbox gaming platform
   - Surface devices
   - Bing search engine

Key Financials (FY 2024):
- Annual Revenue: $245.1 billion
- Operating Income: $109.4 billion
- Net Income: $88.1 billion
- Market Cap: ~$3.3 trillion (as of 2024)
- Employees: ~221,000 worldwide

Stock Information:
- NASDAQ: MSFT
- S&P 500 constituent
- Dow Jones Industrial Average constituent

Recent Major Acquisitions:
- Activision Blizzard (2023) - $68.7 billion
- Nuance Communications (2021) - $19.7 billion
- GitHub (2018) - $7.5 billion
- LinkedIn (2016) - $26.2 billion

Key Products and Services:
- Microsoft Azure
- Microsoft 365
- Windows 11
- Microsoft Teams
- GitHub Copilot
- Xbox Game Pass
- LinkedIn
- Dynamics 365

Sustainability & ESG:
- Carbon negative by 2030 goal
- Zero waste by 2030 goal
- Water positive by 2030 goal
- $1 billion Climate Innovation Fund

Industry Recognition:
- Fortune 500: Ranked #14 (2024)
- Most Valuable Companies globally
- Top employer in technology sector`
};

// Top 5 Microsoft Executives (publicly available information)
const microsoftExecutives = [
  {
    firstName: 'Satya',
    lastName: 'Nadella',
    email: 'satya.nadella@microsoft.com',
    phone: '+1 (425) 882-8080',
    mobile: '+1 (425) 882-8081',
    title: 'Chairman and Chief Executive Officer (CEO)',
    department: 'Executive Leadership',
    status: 'Active',
    preferredContactMethod: 'Email',
    linkedIn: 'https://www.linkedin.com/in/satyanadella',
    twitter: '@sataborat',
    notes: `Satya Narayana Nadella - Chairman and CEO of Microsoft

Professional Background:
- CEO since February 4, 2014
- Chairman since June 2021
- Previously: Executive VP of Cloud and Enterprise Group

Education:
- Bachelor's in Electrical Engineering - Mangalore University, India
- MS in Computer Science - University of Wisconsin-Milwaukee
- MBA - University of Chicago Booth School of Business

Career at Microsoft:
- Joined Microsoft in 1992
- Led transformation of Azure cloud platform
- Drove "Mobile-first, Cloud-first" strategy
- Transformed company culture to growth mindset

Key Achievements as CEO:
- Market cap growth from $300B to $3T+
- Azure became #2 cloud provider globally
- Microsoft Teams launched and grew to 300M+ users
- Successful acquisitions: LinkedIn, GitHub, Activision Blizzard
- AI integration with OpenAI partnership

Board Memberships:
- Starbucks Corporation Board (2017-2024)
- Fred Hutchinson Cancer Research Center Trustee

Personal:
- Born: August 19, 1967 (Hyderabad, India)
- Author: "Hit Refresh" (2017)
- Passionate about cricket
- Advocate for accessibility and inclusion

Recognition:
- Fortune Businessperson of the Year (2019)
- CNBC's CEO of the Decade
- Financial Times Person of the Year (2014)`
  },
  {
    firstName: 'Brad',
    lastName: 'Smith',
    email: 'brad.smith@microsoft.com',
    phone: '+1 (425) 882-8082',
    mobile: '+1 (425) 882-8083',
    title: 'Vice Chair and President',
    department: 'Executive Leadership',
    status: 'Active',
    preferredContactMethod: 'Email',
    linkedIn: 'https://www.linkedin.com/in/intlbradsmith',
    twitter: '@BradSmi',
    notes: `Brad Smith - Vice Chair and President of Microsoft

Professional Background:
- Vice Chair and President since September 2021
- Previously: President and Chief Legal Officer (2015-2021)
- General Counsel (2002-2015)

Education:
- B.A. International Relations and Economics - Princeton University
- J.D. - Columbia Law School

Key Responsibilities:
- Leads Microsoft's work on critical issues
- Environmental sustainability
- Accessibility
- Privacy and cybersecurity
- Digital safety
- Responsible AI
- Philanthropic investments

Prior Experience:
- Covington & Burling law firm (1986-1993)
- Multiple leadership roles at Microsoft since 1993

Key Initiatives Led:
- Microsoft's carbon negative commitment
- Defending Democracy Program
- AI for Good initiatives
- TechSpark rural technology initiative

Publications:
- Co-author: "Tools and Weapons: The Promise and the Peril of the Digital Age" (2019)
- Co-author: "Tools and Weapons Update" (2021)

Board Memberships:
- Netflix Board of Directors
- Princeton University Trustee

Personal:
- Born: 1959 (Wausau, Wisconsin)
- Known for bipartisan approach to policy
- Advocate for digital skills education

Recognition:
- Regularly appears before Congress and parliaments
- Named to TIME 100 list
- Champion of responsible tech policy`
  },
  {
    firstName: 'Amy',
    lastName: 'Hood',
    email: 'amy.hood@microsoft.com',
    phone: '+1 (425) 882-8084',
    mobile: '+1 (425) 882-8085',
    title: 'Executive Vice President and Chief Financial Officer (CFO)',
    department: 'Finance',
    status: 'Active',
    preferredContactMethod: 'Email',
    linkedIn: 'https://www.linkedin.com/in/amy-hood-microsoft',
    twitter: '',
    notes: `Amy Hood - Executive Vice President and CFO of Microsoft

Professional Background:
- CFO since May 2013
- First female CFO in Microsoft history
- Previously: CFO of Microsoft Business Division

Education:
- B.A. Economics - Duke University
- MBA - Harvard Business School

Career at Microsoft:
- Joined Microsoft in 2002
- Various finance leadership roles
- Business Division CFO (2010-2013)

Key Responsibilities:
- Global finance organization leadership
- Financial strategy
- Investor relations
- Business development
- IT and Corporate Strategy

Financial Achievements as CFO:
- Grew annual revenue from $78B to $245B+
- Successful integration of major acquisitions
- Strong margin expansion
- Disciplined capital allocation

Prior Experience:
- Goldman Sachs - Investment Banking Division
- Focused on technology and media companies

Leadership Style:
- Known for analytical rigor
- Data-driven decision making
- Long-term strategic focus
- Clear financial communication

Board Memberships:
- 3M Company Board of Directors

Personal:
- Based in Seattle area
- Known for direct communication style
- Advocate for women in finance and technology

Recognition:
- Forbes Most Powerful Women list
- Fortune Most Powerful Women in Business
- Institutional Investor All-America Executive Team`
  },
  {
    firstName: 'Judson',
    lastName: 'Althoff',
    email: 'judson.althoff@microsoft.com',
    phone: '+1 (425) 882-8086',
    mobile: '+1 (425) 882-8087',
    title: 'Executive Vice President and Chief Commercial Officer',
    department: 'Commercial Sales',
    status: 'Active',
    preferredContactMethod: 'Email',
    linkedIn: 'https://www.linkedin.com/in/judsonalthoff',
    twitter: '@judsonalthoff',
    notes: `Judson Althoff - Executive Vice President and Chief Commercial Officer

Professional Background:
- Chief Commercial Officer since 2021
- EVP Worldwide Commercial Business (2016-2021)
- Joined Microsoft in 2013

Education:
- B.S. Engineering - Pennsylvania State University

Key Responsibilities:
- Leads global commercial business operations
- Enterprise customer relationships
- Partner ecosystem
- Industry solutions
- Customer success

Career at Microsoft:
- Transformed commercial go-to-market approach
- Built industry-focused sales model
- Drove cloud sales acceleration
- Led digital transformation of sales organization

Prior Experience:
- Oracle Corporation - SVP North America Technology Sales
- Various technology sales leadership roles
- Strong enterprise sales background

Key Achievements:
- Commercial cloud revenue growth leadership
- Enterprise customer digital transformation
- Partner ecosystem expansion
- Customer success program development

Industry Focus Areas:
- Financial Services
- Healthcare
- Manufacturing
- Retail
- Government
- Education

Leadership Approach:
- Customer-obsessed culture
- Partner-first mindset
- Industry expertise development
- Digital selling innovation

Personal:
- Passionate about helping customers digitally transform
- Known for energy and customer focus
- Based in Pacific Northwest

Recognition:
- Recognized for commercial transformation success
- Top sales leadership in technology industry`
  },
  {
    firstName: 'Kathleen',
    lastName: 'Hogan',
    email: 'kathleen.hogan@microsoft.com',
    phone: '+1 (425) 882-8088',
    mobile: '+1 (425) 882-8089',
    title: 'Executive Vice President and Chief Human Resources Officer (CHRO)',
    department: 'Human Resources',
    status: 'Active',
    preferredContactMethod: 'Email',
    linkedIn: 'https://www.linkedin.com/in/kathleen-hogan-microsoft',
    twitter: '',
    notes: `Kathleen Hogan - EVP and Chief Human Resources Officer (Chief People Officer)

Professional Background:
- Chief Human Resources Officer since November 2014
- EVP Human Resources and Chief People Officer
- Joined Microsoft in 2003

Education:
- B.S. Applied Mathematics - Harvard University
- MBA - Stanford Graduate School of Business

Key Responsibilities:
- Global HR strategy and operations
- Talent acquisition and development
- Diversity and inclusion
- Culture transformation
- Employee experience
- Compensation and benefits

Career at Microsoft:
- Various leadership roles in Services, IT, and HR
- Previously: CVP of Microsoft Services
- Instrumental in cultural transformation under Satya Nadella

Key Achievements:
- Led Microsoft's culture transformation
- Implemented growth mindset culture
- Expanded diversity and inclusion programs
- Modernized employee experience
- Talent attraction and retention during tech talent war

Prior Experience:
- McKinsey & Company - Management Consultant
- Focused on organizational transformation

Culture Initiatives Led:
- Growth Mindset framework implementation
- "Model, Coach, Care" leadership principles
- Inclusive hiring practices
- Mental health and wellbeing programs
- Hybrid work transformation

Board Memberships:
- Gap Inc. Board of Directors

Personal:
- Strong advocate for workplace culture
- Passionate about learning and development
- Known for empathetic leadership style

Recognition:
- HR Executive of the Year
- Top HR leaders in technology
- Champion of workplace culture transformation`
  }
];

// Helper to get auth token
async function getAuthToken(request: any): Promise<string> {
  const loginResponse = await request.post(`${API_URL}/api/auth/login`, {
    data: {
      email: 'abhi.lal@gmail.com',
      password: 'Admin@123'
    }
  });
  
  if (!loginResponse.ok()) {
    const errorText = await loginResponse.text();
    console.log(`Login failed: ${loginResponse.status()} - ${errorText}`);
    throw new Error(`Login failed: ${loginResponse.status()}`);
  }
  
  const loginData = await loginResponse.json();
  return loginData.accessToken || loginData.token;
}

test.describe.serial('Create Microsoft Corporation with 5 Top Executives', () => {
  let authToken: string;
  let microsoftCustomerId: number;
  const executiveContactIds: number[] = [];

  test('Login and verify authentication', async ({ request }) => {
    authToken = await getAuthToken(request);
    expect(authToken).toBeTruthy();
    console.log('✅ Successfully authenticated');
  });

  test('Create Microsoft Corporation Account', async ({ request }) => {
    authToken = await getAuthToken(request);
    
    console.log('\n🏢 Creating Microsoft Corporation account...');
    
    const customerResponse = await request.post(`${API_URL}/api/customers`, {
      headers: { 'Authorization': `Bearer ${authToken}` },
      data: microsoftAccount
    });

    if (!customerResponse.ok()) {
      const errText = await customerResponse.text();
      console.log(`❌ Customer creation failed: ${customerResponse.status()}`);
      console.log(`   Error: ${errText}`);
      throw new Error(`Failed to create Microsoft account: ${errText}`);
    }
    
    const customerData = await customerResponse.json();
    microsoftCustomerId = customerData.id;
    
    console.log(`✅ Created Microsoft Corporation`);
    console.log(`   Customer ID: ${microsoftCustomerId}`);
    console.log(`   Company: ${customerData.company}`);
    console.log(`   Industry: ${customerData.industry}`);
    
    expect(customerData.company).toBe('Microsoft Corporation');
  });

  test('Create 5 Microsoft Executives as Contacts', async ({ request }) => {
    authToken = await getAuthToken(request);
    
    // First get the Microsoft customer ID
    const customersResponse = await request.get(`${API_URL}/api/customers?pageSize=100`, {
      headers: { 'Authorization': `Bearer ${authToken}` }
    });
    const customersData = await customersResponse.json();
    const items = customersData.items || customersData.data || customersData;
    const microsoft = items.find((c: any) => c.company === 'Microsoft Corporation');
    
    if (!microsoft) {
      throw new Error('Microsoft Corporation not found. Run previous test first.');
    }
    microsoftCustomerId = microsoft.id;
    
    console.log(`\n👥 Creating ${microsoftExecutives.length} Microsoft executives...`);
    console.log(`   Linking to Microsoft (ID: ${microsoftCustomerId})\n`);

    for (let i = 0; i < microsoftExecutives.length; i++) {
      const exec = microsoftExecutives[i];
      
      console.log(`\n👤 Creating executive ${i + 1}/${microsoftExecutives.length}: ${exec.firstName} ${exec.lastName}`);
      console.log(`   Title: ${exec.title}`);
      
      // Create the contact
      const contactResponse = await request.post(`${API_URL}/api/contacts`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: exec.firstName,
          lastName: exec.lastName,
          email: exec.email,
          phone: exec.phone,
          mobile: exec.mobile,
          title: exec.title,
          department: exec.department,
          status: exec.status,
          preferredContactMethod: exec.preferredContactMethod,
          notes: exec.notes
        }
      });

      if (!contactResponse.ok()) {
        const errText = await contactResponse.text();
        console.log(`   ❌ Contact creation failed: ${contactResponse.status()}`);
        console.log(`   Error: ${errText}`);
        throw new Error(`Failed to create contact ${exec.firstName} ${exec.lastName}: ${errText}`);
      }
      
      const contactData = await contactResponse.json();
      executiveContactIds.push(contactData.id);
      console.log(`   ✅ Created contact ID: ${contactData.id}`);
      
      // Link contact to Microsoft
      const isPrimary = i === 0; // Satya Nadella is primary
      const isDecisionMaker = i < 3; // Top 3 are decision makers
      
      const linkResponse = await request.post(`${API_URL}/api/customers/${microsoftCustomerId}/contacts`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          contactId: contactData.id,
          isPrimaryContact: isPrimary,
          isDecisionMaker: isDecisionMaker,
          positionAtCustomer: exec.title,
          notes: `${exec.firstName} ${exec.lastName} - ${exec.department}`
        }
      });

      if (!linkResponse.ok()) {
        const errorText = await linkResponse.text();
        console.log(`   ⚠️ Failed to link contact: ${linkResponse.status()} - ${errorText}`);
      } else {
        console.log(`   ✅ Linked to Microsoft as ${isPrimary ? 'Primary Contact' : 'Contact'}`);
        if (isDecisionMaker) {
          console.log(`   ✅ Marked as Decision Maker`);
        }
      }
      
      expect(contactResponse.ok()).toBeTruthy();
    }
    
    console.log(`\n✅ Created all ${microsoftExecutives.length} executives successfully`);
  });

  test('Verify Microsoft account and contacts', async ({ request }) => {
    authToken = await getAuthToken(request);
    
    // Find Microsoft
    const customersResponse = await request.get(`${API_URL}/api/customers?pageSize=100`, {
      headers: { 'Authorization': `Bearer ${authToken}` }
    });
    const customersData = await customersResponse.json();
    const items = customersData.items || customersData.data || customersData;
    const microsoft = items.find((c: any) => c.company === 'Microsoft Corporation');
    
    expect(microsoft).toBeTruthy();
    console.log(`\n📊 Verification Summary:`);
    console.log(`   ✅ Microsoft Corporation found (ID: ${microsoft.id})`);
    console.log(`   📍 Location: ${microsoft.city}, ${microsoft.state}`);
    console.log(`   🌐 Website: ${microsoft.website}`);
    console.log(`   🏭 Industry: ${microsoft.industry}`);
    
    // Get linked contacts
    const contactsResponse = await request.get(`${API_URL}/api/customers/${microsoft.id}/contacts`, {
      headers: { 'Authorization': `Bearer ${authToken}` }
    });
    
    if (contactsResponse.ok()) {
      const contactsData = await contactsResponse.json();
      const contactItems = contactsData.items || contactsData.data || contactsData || [];
      
      console.log(`\n   👥 Linked Contacts: ${Array.isArray(contactItems) ? contactItems.length : 0}`);
      
      if (Array.isArray(contactItems)) {
        for (const contact of contactItems) {
          const name = contact.contactName || `${contact.firstName} ${contact.lastName}`;
          const title = contact.positionAtCustomer || contact.title || 'N/A';
          const primary = contact.isPrimaryContact ? '⭐ Primary' : '';
          const decision = contact.isDecisionMaker ? '🎯 Decision Maker' : '';
          console.log(`      - ${name}: ${title} ${primary} ${decision}`);
        }
      }
    }
    
    expect(microsoft.company).toBe('Microsoft Corporation');
  });
});
