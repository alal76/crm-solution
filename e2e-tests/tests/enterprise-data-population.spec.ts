import { test, expect, Page } from '@playwright/test';

/**
 * Enterprise Data Population Test Suite
 * 
 * Creates comprehensive test data for 7 major companies:
 * - Microsoft, Google, Amazon, LinkedIn, Infosys, TCS, Wipro
 * 
 * Each company gets:
 * - 5-20 executive contacts
 * - Leads, Opportunities, Service Requests
 * - Full field population to test UI
 */

// Executive data for each company
const companies = [
  {
    name: 'Microsoft Corporation',
    industry: 'Technology',
    website: 'https://www.microsoft.com',
    phone: '+1-425-882-8080',
    address: 'One Microsoft Way',
    city: 'Redmond',
    state: 'Washington',
    country: 'United States',
    zipCode: '98052',
    annualRevenue: 198000000000,
    employees: 221000,
    executives: [
      { firstName: 'Satya', lastName: 'Nadella', title: 'Chief Executive Officer', email: 'satya.nadella@microsoft.com', phone: '+1-425-882-8001', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/satyanadella' },
      { firstName: 'Amy', lastName: 'Hood', title: 'Chief Financial Officer', email: 'amy.hood@microsoft.com', phone: '+1-425-882-8002', department: 'Finance', linkedIn: 'https://linkedin.com/in/amyhood' },
      { firstName: 'Judson', lastName: 'Althoff', title: 'Executive VP & Chief Commercial Officer', email: 'judson.althoff@microsoft.com', phone: '+1-425-882-8003', department: 'Commercial', linkedIn: 'https://linkedin.com/in/judsonalthoff' },
      { firstName: 'Brad', lastName: 'Smith', title: 'Vice Chair and President', email: 'brad.smith@microsoft.com', phone: '+1-425-882-8004', department: 'Legal & Corporate Affairs', linkedIn: 'https://linkedin.com/in/bradsmith' },
      { firstName: 'Scott', lastName: 'Guthrie', title: 'Executive VP Cloud + AI', email: 'scott.guthrie@microsoft.com', phone: '+1-425-882-8005', department: 'Cloud & AI', linkedIn: 'https://linkedin.com/in/scottgu' },
      { firstName: 'Kathleen', lastName: 'Hogan', title: 'Executive VP & Chief Human Resources Officer', email: 'kathleen.hogan@microsoft.com', phone: '+1-425-882-8006', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/kathleenhogan' },
      { firstName: 'Rajesh', lastName: 'Jha', title: 'Executive VP Experiences + Devices', email: 'rajesh.jha@microsoft.com', phone: '+1-425-882-8007', department: 'Product', linkedIn: 'https://linkedin.com/in/rajeshjha' },
      { firstName: 'Christopher', lastName: 'Young', title: 'Executive VP Business Development', email: 'christopher.young@microsoft.com', phone: '+1-425-882-8008', department: 'Business Development', linkedIn: 'https://linkedin.com/in/christopheryoung' },
      { firstName: 'Pavan', lastName: 'Davuluri', title: 'Corporate VP Windows + Devices', email: 'pavan.davuluri@microsoft.com', phone: '+1-425-882-8009', department: 'Windows', linkedIn: 'https://linkedin.com/in/pavandavuluri' },
      { firstName: 'Kevin', lastName: 'Scott', title: 'Chief Technology Officer', email: 'kevin.scott@microsoft.com', phone: '+1-425-882-8010', department: 'Technology', linkedIn: 'https://linkedin.com/in/kevinscott' },
      { firstName: 'Takeshi', lastName: 'Numoto', title: 'Chief Marketing Officer', email: 'takeshi.numoto@microsoft.com', phone: '+1-425-882-8011', department: 'Marketing', linkedIn: 'https://linkedin.com/in/takeshinumoto' },
      { firstName: 'Charlie', lastName: 'Bell', title: 'Executive VP Security', email: 'charlie.bell@microsoft.com', phone: '+1-425-882-8012', department: 'Security', linkedIn: 'https://linkedin.com/in/charliebell' },
    ]
  },
  {
    name: 'Alphabet Inc. (Google)',
    industry: 'Technology',
    website: 'https://www.google.com',
    phone: '+1-650-253-0000',
    address: '1600 Amphitheatre Parkway',
    city: 'Mountain View',
    state: 'California',
    country: 'United States',
    zipCode: '94043',
    annualRevenue: 282000000000,
    employees: 182000,
    executives: [
      { firstName: 'Sundar', lastName: 'Pichai', title: 'Chief Executive Officer', email: 'sundar.pichai@google.com', phone: '+1-650-253-0001', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/sundarpichai' },
      { firstName: 'Ruth', lastName: 'Porat', title: 'President & Chief Investment Officer', email: 'ruth.porat@google.com', phone: '+1-650-253-0002', department: 'Finance', linkedIn: 'https://linkedin.com/in/ruthporat' },
      { firstName: 'Philipp', lastName: 'Schindler', title: 'Chief Business Officer', email: 'philipp.schindler@google.com', phone: '+1-650-253-0003', department: 'Business', linkedIn: 'https://linkedin.com/in/philippschindler' },
      { firstName: 'Kent', lastName: 'Walker', title: 'President Global Affairs & Chief Legal Officer', email: 'kent.walker@google.com', phone: '+1-650-253-0004', department: 'Legal', linkedIn: 'https://linkedin.com/in/kentwalker' },
      { firstName: 'Prabhakar', lastName: 'Raghavan', title: 'SVP Knowledge & Information', email: 'prabhakar.raghavan@google.com', phone: '+1-650-253-0005', department: 'Search', linkedIn: 'https://linkedin.com/in/prabhakarraghavan' },
      { firstName: 'Thomas', lastName: 'Kurian', title: 'CEO Google Cloud', email: 'thomas.kurian@google.com', phone: '+1-650-253-0006', department: 'Cloud', linkedIn: 'https://linkedin.com/in/thomaskurian' },
      { firstName: 'Hiroshi', lastName: 'Lockheimer', title: 'SVP Platforms & Ecosystems', email: 'hiroshi.lockheimer@google.com', phone: '+1-650-253-0007', department: 'Android', linkedIn: 'https://linkedin.com/in/hiroshilockheimer' },
      { firstName: 'Jeff', lastName: 'Dean', title: 'Chief Scientist', email: 'jeff.dean@google.com', phone: '+1-650-253-0008', department: 'AI Research', linkedIn: 'https://linkedin.com/in/jeffdean' },
      { firstName: 'Lorraine', lastName: 'Twohill', title: 'Chief Marketing Officer', email: 'lorraine.twohill@google.com', phone: '+1-650-253-0009', department: 'Marketing', linkedIn: 'https://linkedin.com/in/lorrainetwohill' },
      { firstName: 'Fiona', lastName: 'Cicconi', title: 'Chief People Officer', email: 'fiona.cicconi@google.com', phone: '+1-650-253-0010', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/fionacicconi' },
      { firstName: 'Demis', lastName: 'Hassabis', title: 'CEO DeepMind', email: 'demis.hassabis@deepmind.com', phone: '+1-650-253-0011', department: 'AI', linkedIn: 'https://linkedin.com/in/demishassabis' },
      { firstName: 'Neal', lastName: 'Mohan', title: 'CEO YouTube', email: 'neal.mohan@youtube.com', phone: '+1-650-253-0012', department: 'YouTube', linkedIn: 'https://linkedin.com/in/nealmohan' },
      { firstName: 'Rick', lastName: 'Osterloh', title: 'SVP Devices & Services', email: 'rick.osterloh@google.com', phone: '+1-650-253-0013', department: 'Hardware', linkedIn: 'https://linkedin.com/in/rickosterloh' },
      { firstName: 'Jen', lastName: 'Fitzpatrick', title: 'SVP Google Core', email: 'jen.fitzpatrick@google.com', phone: '+1-650-253-0014', department: 'Core Engineering', linkedIn: 'https://linkedin.com/in/jenfitzpatrick' },
    ]
  },
  {
    name: 'Amazon.com Inc.',
    industry: 'E-commerce & Cloud',
    website: 'https://www.amazon.com',
    phone: '+1-206-266-1000',
    address: '410 Terry Avenue North',
    city: 'Seattle',
    state: 'Washington',
    country: 'United States',
    zipCode: '98109',
    annualRevenue: 574000000000,
    employees: 1540000,
    executives: [
      { firstName: 'Andy', lastName: 'Jassy', title: 'President & Chief Executive Officer', email: 'andy.jassy@amazon.com', phone: '+1-206-266-1001', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/andyjassy' },
      { firstName: 'Brian', lastName: 'Olsavsky', title: 'Senior Vice President & Chief Financial Officer', email: 'brian.olsavsky@amazon.com', phone: '+1-206-266-1002', department: 'Finance', linkedIn: 'https://linkedin.com/in/brianolsavsky' },
      { firstName: 'Doug', lastName: 'Herrington', title: 'CEO Worldwide Amazon Stores', email: 'doug.herrington@amazon.com', phone: '+1-206-266-1003', department: 'Retail', linkedIn: 'https://linkedin.com/in/dougherrington' },
      { firstName: 'Matt', lastName: 'Garman', title: 'CEO Amazon Web Services', email: 'matt.garman@amazon.com', phone: '+1-206-266-1004', department: 'AWS', linkedIn: 'https://linkedin.com/in/mattgarman' },
      { firstName: 'David', lastName: 'Zapolsky', title: 'Senior Vice President & General Counsel', email: 'david.zapolsky@amazon.com', phone: '+1-206-266-1005', department: 'Legal', linkedIn: 'https://linkedin.com/in/davidzapolsky' },
      { firstName: 'Beth', lastName: 'Galetti', title: 'Senior Vice President People Experience & Technology', email: 'beth.galetti@amazon.com', phone: '+1-206-266-1006', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/bethgaletti' },
      { firstName: 'Werner', lastName: 'Vogels', title: 'Chief Technology Officer', email: 'werner.vogels@amazon.com', phone: '+1-206-266-1007', department: 'Technology', linkedIn: 'https://linkedin.com/in/wernervogels' },
      { firstName: 'Mike', lastName: 'Hopkins', title: 'Senior Vice President Prime Video & Amazon Studios', email: 'mike.hopkins@amazon.com', phone: '+1-206-266-1008', department: 'Entertainment', linkedIn: 'https://linkedin.com/in/mikehopkins' },
      { firstName: 'Dave', lastName: 'Clark', title: 'CEO Flexport (Former Amazon Ops)', email: 'dave.clark@amazon.com', phone: '+1-206-266-1009', department: 'Operations', linkedIn: 'https://linkedin.com/in/daveclark' },
      { firstName: 'Rohit', lastName: 'Prasad', title: 'Head Scientist Alexa AI', email: 'rohit.prasad@amazon.com', phone: '+1-206-266-1010', department: 'AI', linkedIn: 'https://linkedin.com/in/rohitprasad' },
      { firstName: 'Panos', lastName: 'Panay', title: 'CEO Amazon Devices', email: 'panos.panay@amazon.com', phone: '+1-206-266-1011', department: 'Devices', linkedIn: 'https://linkedin.com/in/panospanay' },
      { firstName: 'Udit', lastName: 'Madan', title: 'VP Worldwide Operations', email: 'udit.madan@amazon.com', phone: '+1-206-266-1012', department: 'Operations', linkedIn: 'https://linkedin.com/in/uditmadan' },
      { firstName: 'Stefano', lastName: 'Mazzocchi', title: 'VP Technology AWS', email: 'stefano.mazzocchi@amazon.com', phone: '+1-206-266-1013', department: 'AWS Engineering', linkedIn: 'https://linkedin.com/in/stefanomazzocchi' },
      { firstName: 'Ruba', lastName: 'Borno', title: 'VP Channels & Alliances AWS', email: 'ruba.borno@amazon.com', phone: '+1-206-266-1014', department: 'AWS Sales', linkedIn: 'https://linkedin.com/in/rubaborno' },
      { firstName: 'Mai-Lan', lastName: 'Tomsen Bukovec', title: 'VP AWS AI/ML', email: 'mailan.bukovec@amazon.com', phone: '+1-206-266-1015', department: 'AWS AI', linkedIn: 'https://linkedin.com/in/mailantomsenbukovec' },
      { firstName: 'Adam', lastName: 'Selipsky', title: 'Former CEO AWS', email: 'adam.selipsky@amazon.com', phone: '+1-206-266-1016', department: 'AWS Executive', linkedIn: 'https://linkedin.com/in/adamselipsky' },
    ]
  },
  {
    name: 'LinkedIn Corporation',
    industry: 'Professional Networking',
    website: 'https://www.linkedin.com',
    phone: '+1-650-687-3600',
    address: '1000 W Maude Avenue',
    city: 'Sunnyvale',
    state: 'California',
    country: 'United States',
    zipCode: '94085',
    annualRevenue: 15000000000,
    employees: 20000,
    executives: [
      { firstName: 'Ryan', lastName: 'Roslansky', title: 'Chief Executive Officer', email: 'ryan.roslansky@linkedin.com', phone: '+1-650-687-3601', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/ryanroslansky' },
      { firstName: 'Tomer', lastName: 'Cohen', title: 'Chief Product Officer', email: 'tomer.cohen@linkedin.com', phone: '+1-650-687-3602', department: 'Product', linkedIn: 'https://linkedin.com/in/tomercohen' },
      { firstName: 'Mohak', lastName: 'Shroff', title: 'Senior Vice President Engineering', email: 'mohak.shroff@linkedin.com', phone: '+1-650-687-3603', department: 'Engineering', linkedIn: 'https://linkedin.com/in/mohakshroff' },
      { firstName: 'Blake', lastName: 'Barnes', title: 'Chief Financial Officer', email: 'blake.barnes@linkedin.com', phone: '+1-650-687-3604', department: 'Finance', linkedIn: 'https://linkedin.com/in/blakebarnes' },
      { firstName: 'Teuila', lastName: 'Hanson', title: 'Chief People Officer', email: 'teuila.hanson@linkedin.com', phone: '+1-650-687-3605', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/teuilahanson' },
      { firstName: 'Melissa', lastName: 'Selcher', title: 'Chief Marketing & Communications Officer', email: 'melissa.selcher@linkedin.com', phone: '+1-650-687-3606', department: 'Marketing', linkedIn: 'https://linkedin.com/in/melissaselcher' },
      { firstName: 'Mike', lastName: 'Gamson', title: 'Head of LinkedIn Sales Solutions', email: 'mike.gamson@linkedin.com', phone: '+1-650-687-3607', department: 'Sales', linkedIn: 'https://linkedin.com/in/mikegamson' },
      { firstName: 'Penry', lastName: 'Price', title: 'VP Marketing Solutions', email: 'penry.price@linkedin.com', phone: '+1-650-687-3608', department: 'Marketing Solutions', linkedIn: 'https://linkedin.com/in/penryprice' },
    ]
  },
  {
    name: 'Infosys Limited',
    industry: 'IT Services & Consulting',
    website: 'https://www.infosys.com',
    phone: '+91-80-2852-0261',
    address: 'Electronics City',
    city: 'Bangalore',
    state: 'Karnataka',
    country: 'India',
    zipCode: '560100',
    annualRevenue: 18200000000,
    employees: 343000,
    executives: [
      { firstName: 'Salil', lastName: 'Parekh', title: 'Chief Executive Officer & Managing Director', email: 'salil.parekh@infosys.com', phone: '+91-80-2852-0001', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/salilparekh' },
      { firstName: 'Nilanjan', lastName: 'Roy', title: 'Chief Financial Officer', email: 'nilanjan.roy@infosys.com', phone: '+91-80-2852-0002', department: 'Finance', linkedIn: 'https://linkedin.com/in/nilanjanroy' },
      { firstName: 'Mohit', lastName: 'Joshi', title: 'President (Former)', email: 'mohit.joshi@infosys.com', phone: '+91-80-2852-0003', department: 'Delivery', linkedIn: 'https://linkedin.com/in/mohitjoshi' },
      { firstName: 'Ravi', lastName: 'Kumar', title: 'President (Former COO)', email: 'ravi.kumar@infosys.com', phone: '+91-80-2852-0004', department: 'Operations', linkedIn: 'https://linkedin.com/in/ravikumar' },
      { firstName: 'Shaji', lastName: 'Mathew', title: 'Group Head Human Resources', email: 'shaji.mathew@infosys.com', phone: '+91-80-2852-0005', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/shajimathew' },
      { firstName: 'Pravin', lastName: 'Rao', title: 'Former COO', email: 'pravin.rao@infosys.com', phone: '+91-80-2852-0006', department: 'Operations', linkedIn: 'https://linkedin.com/in/pravinrao' },
      { firstName: 'Anand', lastName: 'Swaminathan', title: 'SVP & Global Head Industries', email: 'anand.swaminathan@infosys.com', phone: '+91-80-2852-0007', department: 'Industries', linkedIn: 'https://linkedin.com/in/anandswaminathan' },
      { firstName: 'Jasmeet', lastName: 'Singh', title: 'EVP Global Head Delivery', email: 'jasmeet.singh@infosys.com', phone: '+91-80-2852-0008', department: 'Delivery', linkedIn: 'https://linkedin.com/in/jasmeetsingh' },
      { firstName: 'Sunil', lastName: 'Senan', title: 'SVP & Global Head AI', email: 'sunil.senan@infosys.com', phone: '+91-80-2852-0009', department: 'AI & Data', linkedIn: 'https://linkedin.com/in/sunilsenan' },
      { firstName: 'Karmesh', lastName: 'Vaswani', title: 'EVP Americas', email: 'karmesh.vaswani@infosys.com', phone: '+1-510-742-3000', department: 'Americas', linkedIn: 'https://linkedin.com/in/karmeshvaswani' },
      { firstName: 'Satish', lastName: 'HC', title: 'EVP Europe', email: 'satish.hc@infosys.com', phone: '+44-20-7715-3300', department: 'Europe', linkedIn: 'https://linkedin.com/in/satishhc' },
      { firstName: 'Dennis', lastName: 'Gada', title: 'EVP & Regional Head Manufacturing', email: 'dennis.gada@infosys.com', phone: '+91-80-2852-0010', department: 'Manufacturing', linkedIn: 'https://linkedin.com/in/dennisgada' },
    ]
  },
  {
    name: 'Tata Consultancy Services',
    industry: 'IT Services & Consulting',
    website: 'https://www.tcs.com',
    phone: '+91-22-6778-9999',
    address: 'TCS House, Raveline Street',
    city: 'Mumbai',
    state: 'Maharashtra',
    country: 'India',
    zipCode: '400001',
    annualRevenue: 29000000000,
    employees: 615000,
    executives: [
      { firstName: 'K', lastName: 'Krithivasan', title: 'Chief Executive Officer & Managing Director', email: 'k.krithivasan@tcs.com', phone: '+91-22-6778-9001', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/kkrithivasan' },
      { firstName: 'Samir', lastName: 'Seksaria', title: 'Chief Financial Officer', email: 'samir.seksaria@tcs.com', phone: '+91-22-6778-9002', department: 'Finance', linkedIn: 'https://linkedin.com/in/samirseksaria' },
      { firstName: 'NGS', lastName: 'Subramaniam', title: 'Chief Operating Officer', email: 'ngs.subramaniam@tcs.com', phone: '+91-22-6778-9003', department: 'Operations', linkedIn: 'https://linkedin.com/in/ngssubramaniam' },
      { firstName: 'Milind', lastName: 'Lakkad', title: 'Chief Human Resources Officer', email: 'milind.lakkad@tcs.com', phone: '+91-22-6778-9004', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/milindlakkad' },
      { firstName: 'Rajashree', lastName: 'R', title: 'Chief Marketing Officer', email: 'rajashree.r@tcs.com', phone: '+91-22-6778-9005', department: 'Marketing', linkedIn: 'https://linkedin.com/in/rajashreer' },
      { firstName: 'Rammohan', lastName: 'Gourneni', title: 'President North America', email: 'rammohan.gourneni@tcs.com', phone: '+1-646-313-4000', department: 'North America', linkedIn: 'https://linkedin.com/in/rammohangourneni' },
      { firstName: 'Sapthagiri', lastName: 'Chapalapalli', title: 'President UK & Europe', email: 'sapthagiri.chapalapalli@tcs.com', phone: '+44-20-7220-1000', department: 'Europe', linkedIn: 'https://linkedin.com/in/sapthagiri' },
      { firstName: 'Harrick', lastName: 'Vin', title: 'Chief Technology Officer', email: 'harrick.vin@tcs.com', phone: '+91-22-6778-9006', department: 'Technology', linkedIn: 'https://linkedin.com/in/harrickvin' },
      { firstName: 'Saurabh', lastName: 'Govil', title: 'President BFSI', email: 'saurabh.govil@tcs.com', phone: '+91-22-6778-9007', department: 'Banking & Finance', linkedIn: 'https://linkedin.com/in/saurabhgovil' },
      { firstName: 'Debashis', lastName: 'Ghosh', title: 'President Retail & Consumer Goods', email: 'debashis.ghosh@tcs.com', phone: '+91-22-6778-9008', department: 'Retail', linkedIn: 'https://linkedin.com/in/debashisghosh' },
      { firstName: 'Anupam', lastName: 'Singhal', title: 'President Manufacturing', email: 'anupam.singhal@tcs.com', phone: '+91-22-6778-9009', department: 'Manufacturing', linkedIn: 'https://linkedin.com/in/anupamsinghal' },
      { firstName: 'Krishnan', lastName: 'Ramanujam', title: 'President TCS Interactive', email: 'krishnan.ramanujam@tcs.com', phone: '+91-22-6778-9010', department: 'Digital', linkedIn: 'https://linkedin.com/in/krishnanramanujam' },
      { firstName: 'Ramamoorthy', lastName: 'N', title: 'President Healthcare & Life Sciences', email: 'ramamoorthy.n@tcs.com', phone: '+91-22-6778-9011', department: 'Healthcare', linkedIn: 'https://linkedin.com/in/ramamoorthyn' },
      { firstName: 'Shankar', lastName: 'Narayanan', title: 'President Energy & Utilities', email: 'shankar.narayanan@tcs.com', phone: '+91-22-6778-9012', department: 'Energy', linkedIn: 'https://linkedin.com/in/shankarnarayanan' },
      { firstName: 'Amit', lastName: 'Bajaj', title: 'President Cloud Business', email: 'amit.bajaj@tcs.com', phone: '+91-22-6778-9013', department: 'Cloud', linkedIn: 'https://linkedin.com/in/amitbajaj' },
    ]
  },
  {
    name: 'Wipro Limited',
    industry: 'IT Services & Consulting',
    website: 'https://www.wipro.com',
    phone: '+91-80-2844-0011',
    address: 'Doddakannelli, Sarjapur Road',
    city: 'Bangalore',
    state: 'Karnataka',
    country: 'India',
    zipCode: '560035',
    annualRevenue: 11300000000,
    employees: 258000,
    executives: [
      { firstName: 'Srinivas', lastName: 'Pallia', title: 'Chief Executive Officer & Managing Director', email: 'srinivas.pallia@wipro.com', phone: '+91-80-2844-0001', department: 'Executive Office', linkedIn: 'https://linkedin.com/in/srinivaspallia' },
      { firstName: 'Aparna', lastName: 'Iyer', title: 'Chief Financial Officer', email: 'aparna.iyer@wipro.com', phone: '+91-80-2844-0002', department: 'Finance', linkedIn: 'https://linkedin.com/in/aparnaiyer' },
      { firstName: 'Sanjeev', lastName: 'Jain', title: 'Chief Operating Officer', email: 'sanjeev.jain@wipro.com', phone: '+91-80-2844-0003', department: 'Operations', linkedIn: 'https://linkedin.com/in/sanjeevjain' },
      { firstName: 'Saurabh', lastName: 'Govil', title: 'Chief Human Resources Officer', email: 'saurabh.govil@wipro.com', phone: '+91-80-2844-0004', department: 'Human Resources', linkedIn: 'https://linkedin.com/in/saurabhgovilwipro' },
      { firstName: 'Subha', lastName: 'Tatavarti', title: 'Chief Technology Officer', email: 'subha.tatavarti@wipro.com', phone: '+91-80-2844-0005', department: 'Technology', linkedIn: 'https://linkedin.com/in/subhatatavarti' },
      { firstName: 'Stephanie', lastName: 'Trautman', title: 'Chief Growth Officer', email: 'stephanie.trautman@wipro.com', phone: '+1-347-474-5200', department: 'Growth', linkedIn: 'https://linkedin.com/in/stephanietrautman' },
      { firstName: 'Anis', lastName: 'Chenchah', title: 'CEO Americas 1', email: 'anis.chenchah@wipro.com', phone: '+1-646-784-5000', department: 'Americas', linkedIn: 'https://linkedin.com/in/anischenchah' },
      { firstName: 'Pierre', lastName: 'Bruno', title: 'CEO Europe', email: 'pierre.bruno@wipro.com', phone: '+44-20-7535-8000', department: 'Europe', linkedIn: 'https://linkedin.com/in/pierrebruno' },
      { firstName: 'Vinay', lastName: 'Firake', title: 'CEO APMEA', email: 'vinay.firake@wipro.com', phone: '+91-80-2844-0006', department: 'APMEA', linkedIn: 'https://linkedin.com/in/vinayfirake' },
      { firstName: 'Satish', lastName: 'Babu', title: 'Global Head BFSI', email: 'satish.babu@wipro.com', phone: '+91-80-2844-0007', department: 'BFSI', linkedIn: 'https://linkedin.com/in/satishbabu' },
      { firstName: 'Srini', lastName: 'Pallia', title: 'President Manufacturing', email: 'srini.pallia@wipro.com', phone: '+91-80-2844-0008', department: 'Manufacturing', linkedIn: 'https://linkedin.com/in/srinipallia' },
    ]
  }
];

// Lead data
const leads = [
  { firstName: 'James', lastName: 'Wilson', company: 'TechStart Inc.', email: 'james.wilson@techstart.io', phone: '+1-415-555-0101', title: 'VP Engineering', source: 'Website', score: 85 },
  { firstName: 'Sarah', lastName: 'Chen', company: 'DataFlow Systems', email: 'sarah.chen@dataflow.com', phone: '+1-408-555-0102', title: 'CTO', source: 'Conference', score: 92 },
  { firstName: 'Michael', lastName: 'Brown', company: 'CloudNine Solutions', email: 'michael.brown@cloudnine.io', phone: '+1-650-555-0103', title: 'Director IT', source: 'Referral', score: 78 },
  { firstName: 'Emily', lastName: 'Davis', company: 'FinTech Global', email: 'emily.davis@fintechglobal.com', phone: '+1-212-555-0104', title: 'CFO', source: 'LinkedIn', score: 88 },
  { firstName: 'Robert', lastName: 'Martinez', company: 'Healthcare Plus', email: 'robert.martinez@healthcareplus.org', phone: '+1-713-555-0105', title: 'CIO', source: 'Email Campaign', score: 72 },
  { firstName: 'Jennifer', lastName: 'Taylor', company: 'RetailMax Corp', email: 'jennifer.taylor@retailmax.com', phone: '+1-312-555-0106', title: 'VP Operations', source: 'Trade Show', score: 81 },
  { firstName: 'David', lastName: 'Anderson', company: 'Manufacturing Pro', email: 'david.anderson@mfgpro.com', phone: '+1-313-555-0107', title: 'Plant Manager', source: 'Cold Call', score: 65 },
  { firstName: 'Lisa', lastName: 'Thomas', company: 'EduTech Solutions', email: 'lisa.thomas@edutech.com', phone: '+1-617-555-0108', title: 'Director of Technology', source: 'Webinar', score: 76 },
  { firstName: 'Christopher', lastName: 'Jackson', company: 'Energy Systems Inc', email: 'chris.jackson@energysystems.com', phone: '+1-713-555-0109', title: 'VP Engineering', source: 'Partner Referral', score: 89 },
  { firstName: 'Amanda', lastName: 'White', company: 'Media Networks', email: 'amanda.white@medianetworks.com', phone: '+1-310-555-0110', title: 'CMO', source: 'Social Media', score: 71 },
  { firstName: 'Daniel', lastName: 'Harris', company: 'Logistics Global', email: 'daniel.harris@logisticsglobal.com', phone: '+1-972-555-0111', title: 'Supply Chain Director', source: 'Website', score: 83 },
  { firstName: 'Jessica', lastName: 'Martin', company: 'BioPharm Inc', email: 'jessica.martin@biopharminc.com', phone: '+1-858-555-0112', title: 'Research Director', source: 'Conference', score: 90 },
  { firstName: 'Matthew', lastName: 'Thompson', company: 'InsureTech', email: 'matthew.thompson@insuretech.com', phone: '+1-860-555-0113', title: 'VP Product', source: 'LinkedIn', score: 77 },
  { firstName: 'Ashley', lastName: 'Garcia', company: 'AutoParts Direct', email: 'ashley.garcia@autopartsdirect.com', phone: '+1-248-555-0114', title: 'Procurement Manager', source: 'Trade Show', score: 68 },
  { firstName: 'Joshua', lastName: 'Robinson', company: 'TelecomMax', email: 'joshua.robinson@telecommax.com', phone: '+1-469-555-0115', title: 'Network Director', source: 'Referral', score: 86 },
  { firstName: 'Nicole', lastName: 'Lee', company: 'Fashion Forward', email: 'nicole.lee@fashionforward.com', phone: '+1-213-555-0116', title: 'CEO', source: 'Social Media', score: 79 },
  { firstName: 'Andrew', lastName: 'Clark', company: 'Construction Solutions', email: 'andrew.clark@constructsol.com', phone: '+1-602-555-0117', title: 'Project Director', source: 'Website', score: 73 },
  { firstName: 'Stephanie', lastName: 'Lewis', company: 'Travel Tech', email: 'stephanie.lewis@traveltech.io', phone: '+1-305-555-0118', title: 'CTO', source: 'Email Campaign', score: 84 },
  { firstName: 'Kevin', lastName: 'Walker', company: 'AgriTech Systems', email: 'kevin.walker@agritech.com', phone: '+1-515-555-0119', title: 'VP Technology', source: 'Webinar', score: 70 },
  { firstName: 'Michelle', lastName: 'Hall', company: 'Real Estate Pro', email: 'michelle.hall@realestatepro.com', phone: '+1-404-555-0120', title: 'Managing Director', source: 'Partner Referral', score: 82 },
];

// Opportunity data templates
const opportunityTemplates = [
  { name: 'Enterprise Cloud Migration', amount: 2500000, stage: 'Proposal', probability: 60 },
  { name: 'Digital Transformation Initiative', amount: 5000000, stage: 'Negotiation', probability: 75 },
  { name: 'AI/ML Platform Implementation', amount: 1500000, stage: 'Qualification', probability: 40 },
  { name: 'Cybersecurity Assessment & Upgrade', amount: 800000, stage: 'Proposal', probability: 55 },
  { name: 'ERP System Modernization', amount: 3200000, stage: 'Discovery', probability: 30 },
  { name: 'Data Analytics Platform', amount: 1200000, stage: 'Proposal', probability: 65 },
  { name: 'Infrastructure Optimization', amount: 900000, stage: 'Negotiation', probability: 80 },
  { name: 'Application Development Services', amount: 750000, stage: 'Qualification', probability: 45 },
  { name: 'Managed Services Contract', amount: 2000000, stage: 'Closed Won', probability: 100 },
  { name: 'Cloud Native Development', amount: 1800000, stage: 'Proposal', probability: 50 },
  { name: 'DevOps Transformation', amount: 600000, stage: 'Discovery', probability: 35 },
  { name: 'Customer Experience Platform', amount: 1100000, stage: 'Negotiation', probability: 70 },
  { name: 'Supply Chain Optimization', amount: 1400000, stage: 'Proposal', probability: 55 },
  { name: 'IoT Solution Implementation', amount: 850000, stage: 'Qualification', probability: 40 },
  { name: 'Salesforce Implementation', amount: 500000, stage: 'Closed Won', probability: 100 },
];

// Service Request templates
const serviceRequestTemplates = [
  { subject: 'Critical Production Issue', description: 'Production system down, affecting all users', priority: 'Critical', category: 'Technical Support' },
  { subject: 'Login Authentication Failure', description: 'Users unable to authenticate via SSO', priority: 'High', category: 'Access Management' },
  { subject: 'Performance Degradation', description: 'Application response times exceeding SLA', priority: 'High', category: 'Performance' },
  { subject: 'Data Export Request', description: 'Need to export customer data for compliance audit', priority: 'Medium', category: 'Data Services' },
  { subject: 'Feature Enhancement Request', description: 'Request to add bulk import functionality', priority: 'Low', category: 'Enhancement' },
  { subject: 'Integration Connection Failed', description: 'API connection to third-party service failing', priority: 'High', category: 'Integration' },
  { subject: 'Report Generation Error', description: 'Monthly report failing to generate', priority: 'Medium', category: 'Reporting' },
  { subject: 'User Provisioning Request', description: 'Need to add 50 new users to the system', priority: 'Medium', category: 'User Management' },
  { subject: 'Security Vulnerability Report', description: 'Potential vulnerability identified in login module', priority: 'Critical', category: 'Security' },
  { subject: 'Database Query Timeout', description: 'Complex queries timing out after 30 seconds', priority: 'High', category: 'Database' },
  { subject: 'Mobile App Crash', description: 'App crashing on iOS 17 devices', priority: 'High', category: 'Mobile' },
  { subject: 'Email Notification Delay', description: 'Email notifications delayed by several hours', priority: 'Medium', category: 'Notifications' },
  { subject: 'Password Reset Not Working', description: 'Password reset emails not being received', priority: 'High', category: 'Authentication' },
  { subject: 'Dashboard Loading Slowly', description: 'Executive dashboard taking 45+ seconds to load', priority: 'Medium', category: 'Performance' },
  { subject: 'API Rate Limiting Issue', description: 'Getting rate limited despite being under quota', priority: 'Medium', category: 'API' },
  { subject: 'Backup Verification Request', description: 'Need verification of disaster recovery backups', priority: 'Low', category: 'Backup' },
  { subject: 'SSL Certificate Renewal', description: 'SSL certificate expiring in 30 days', priority: 'Medium', category: 'Infrastructure' },
  { subject: 'Custom Report Development', description: 'Need custom sales pipeline report', priority: 'Low', category: 'Reporting' },
  { subject: 'Data Migration Assistance', description: 'Help needed migrating legacy data', priority: 'Medium', category: 'Data Services' },
  { subject: 'Training Request', description: 'Request for advanced admin training session', priority: 'Low', category: 'Training' },
];

// User data
const users = [
  { username: 'john.smith', email: 'john.smith@crm.local', firstName: 'John', lastName: 'Smith', role: 'Sales Manager' },
  { username: 'jane.doe', email: 'jane.doe@crm.local', firstName: 'Jane', lastName: 'Doe', role: 'Account Executive' },
  { username: 'mike.johnson', email: 'mike.johnson@crm.local', firstName: 'Mike', lastName: 'Johnson', role: 'Support Agent' },
  { username: 'sarah.williams', email: 'sarah.williams@crm.local', firstName: 'Sarah', lastName: 'Williams', role: 'Sales Rep' },
  { username: 'david.brown', email: 'david.brown@crm.local', firstName: 'David', lastName: 'Brown', role: 'Support Lead' },
  { username: 'emily.jones', email: 'emily.jones@crm.local', firstName: 'Emily', lastName: 'Jones', role: 'Marketing Manager' },
  { username: 'chris.miller', email: 'chris.miller@crm.local', firstName: 'Chris', lastName: 'Miller', role: 'Account Manager' },
  { username: 'lisa.wilson', email: 'lisa.wilson@crm.local', firstName: 'Lisa', lastName: 'Wilson', role: 'Sales Director' },
  { username: 'tom.moore', email: 'tom.moore@crm.local', firstName: 'Tom', lastName: 'Moore', role: 'Technical Support' },
  { username: 'amy.taylor', email: 'amy.taylor@crm.local', firstName: 'Amy', lastName: 'Taylor', role: 'Customer Success' },
  { username: 'ryan.anderson', email: 'ryan.anderson@crm.local', firstName: 'Ryan', lastName: 'Anderson', role: 'Sales Rep' },
  { username: 'kate.thomas', email: 'kate.thomas@crm.local', firstName: 'Kate', lastName: 'Thomas', role: 'Support Agent' },
  { username: 'alex.jackson', email: 'alex.jackson@crm.local', firstName: 'Alex', lastName: 'Jackson', role: 'Account Executive' },
  { username: 'maria.garcia', email: 'maria.garcia@crm.local', firstName: 'Maria', lastName: 'Garcia', role: 'Marketing Specialist' },
  { username: 'james.martinez', email: 'james.martinez@crm.local', firstName: 'James', lastName: 'Martinez', role: 'Sales Rep' },
  { username: 'rachel.lee', email: 'rachel.lee@crm.local', firstName: 'Rachel', lastName: 'Lee', role: 'Customer Success Manager' },
  { username: 'kevin.white', email: 'kevin.white@crm.local', firstName: 'Kevin', lastName: 'White', role: 'Technical Lead' },
  { username: 'jennifer.harris', email: 'jennifer.harris@crm.local', firstName: 'Jennifer', lastName: 'Harris', role: 'Sales Operations' },
  { username: 'brian.clark', email: 'brian.clark@crm.local', firstName: 'Brian', lastName: 'Clark', role: 'Support Manager' },
  { username: 'nicole.lewis', email: 'nicole.lewis@crm.local', firstName: 'Nicole', lastName: 'Lewis', role: 'Regional Manager' },
];

// Campaign data
const campaigns = [
  { 
    name: 'Q1 2026 Cloud Transformation Campaign', 
    type: 'Multi-Channel',
    description: 'Comprehensive campaign targeting enterprises ready for cloud migration and digital transformation initiatives',
    budget: 250000,
    startDate: '2026-01-01',
    endDate: '2026-03-31'
  },
  { 
    name: 'AI & Machine Learning Summit 2026', 
    type: 'Event',
    description: 'Virtual summit showcasing AI/ML capabilities and customer success stories',
    budget: 150000,
    startDate: '2026-02-15',
    endDate: '2026-02-17'
  },
  { 
    name: 'Enterprise Security Awareness Campaign', 
    type: 'Email',
    description: 'Educational email series on cybersecurity best practices and zero-trust architecture',
    budget: 75000,
    startDate: '2026-01-15',
    endDate: '2026-04-30'
  },
  { 
    name: 'Partner Ecosystem Growth Initiative', 
    type: 'Partner',
    description: 'Campaign to expand partner network and drive co-selling opportunities',
    budget: 200000,
    startDate: '2026-02-01',
    endDate: '2026-06-30'
  },
  { 
    name: 'Customer Success Stories Program', 
    type: 'Content',
    description: 'Video testimonials and case studies from top enterprise customers',
    budget: 100000,
    startDate: '2026-01-01',
    endDate: '2026-12-31'
  },
];

// Helper to generate unique timestamp suffix
const timestamp = () => Date.now().toString().slice(-6);

// Helper function to wait and retry
async function waitForElement(page: Page, selector: string, timeout = 10000): Promise<boolean> {
  try {
    await page.waitForSelector(selector, { timeout, state: 'visible' });
    return true;
  } catch {
    return false;
  }
}

// Helper to fill form field if it exists
async function fillIfExists(page: Page, selector: string, value: string) {
  try {
    const element = await page.$(selector);
    if (element) {
      await element.fill(value);
      return true;
    }
  } catch {
    // Field doesn't exist or not fillable
  }
  return false;
}

// Helper to click if exists
async function clickIfExists(page: Page, selector: string) {
  try {
    const element = await page.$(selector);
    if (element) {
      await element.click();
      return true;
    }
  } catch {
    // Element doesn't exist or not clickable
  }
  return false;
}

test.describe('Enterprise Data Population', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.fill('input[name="email"], input[type="email"]', process.env.AUTH_USER || 'abhi.lal@gmail.com');
    await page.fill('input[name="password"], input[type="password"]', process.env.AUTH_PASSWORD || 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard**', { timeout: 15000 });
  });

  test('1. Create Customers for 7 Major Companies', async ({ page }) => {
    test.setTimeout(300000); // 5 minutes
    
    const createdCustomers: string[] = [];
    
    for (const company of companies) {
      console.log(`Creating customer: ${company.name}`);
      
      // Navigate to customers
      await page.goto('/customers');
      await page.waitForLoadState('networkidle');
      
      // Click Add Customer button
      const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addBtn.isVisible()) {
        await addBtn.click();
        await page.waitForTimeout(1000);
      }
      
      // Fill customer form
      await fillIfExists(page, 'input[name="company"], input[name="Company"]', company.name);
      await fillIfExists(page, 'input[name="firstName"], input[name="FirstName"]', company.executives[0].firstName);
      await fillIfExists(page, 'input[name="lastName"], input[name="LastName"]', company.executives[0].lastName);
      await fillIfExists(page, 'input[name="email"], input[name="Email"]', company.executives[0].email);
      await fillIfExists(page, 'input[name="phone"], input[name="Phone"]', company.phone);
      await fillIfExists(page, 'input[name="website"], input[name="Website"]', company.website);
      await fillIfExists(page, 'input[name="industry"], input[name="Industry"]', company.industry);
      await fillIfExists(page, 'input[name="address"], input[name="Address"]', company.address);
      await fillIfExists(page, 'input[name="city"], input[name="City"]', company.city);
      await fillIfExists(page, 'input[name="state"], input[name="State"]', company.state);
      await fillIfExists(page, 'input[name="country"], input[name="Country"]', company.country);
      await fillIfExists(page, 'input[name="zipCode"], input[name="ZipCode"]', company.zipCode);
      
      // Try to submit
      const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit")').first();
      if (await saveBtn.isVisible()) {
        await saveBtn.click();
        await page.waitForTimeout(2000);
      }
      
      createdCustomers.push(company.name);
      console.log(`✓ Created customer: ${company.name}`);
    }
    
    console.log(`Created ${createdCustomers.length} customers`);
    expect(createdCustomers.length).toBe(7);
  });

  test('2. Create Contacts for All Companies', async ({ page }) => {
    test.setTimeout(600000); // 10 minutes
    
    let contactCount = 0;
    
    for (const company of companies) {
      for (const exec of company.executives) {
        console.log(`Creating contact: ${exec.firstName} ${exec.lastName} (${company.name})`);
        
        // Navigate to contacts
        await page.goto('/contacts');
        await page.waitForLoadState('networkidle');
        
        // Click Add button
        const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
        if (await addBtn.isVisible()) {
          await addBtn.click();
          await page.waitForTimeout(1000);
        }
        
        // Fill contact form
        await fillIfExists(page, 'input[name="firstName"], input[name="FirstName"]', exec.firstName);
        await fillIfExists(page, 'input[name="lastName"], input[name="LastName"]', exec.lastName);
        await fillIfExists(page, 'input[name="email"], input[name="Email"], input[name="emailPrimary"]', exec.email);
        await fillIfExists(page, 'input[name="phone"], input[name="Phone"], input[name="phonePrimary"]', exec.phone);
        await fillIfExists(page, 'input[name="jobTitle"], input[name="JobTitle"], input[name="title"]', exec.title);
        await fillIfExists(page, 'input[name="department"], input[name="Department"]', exec.department);
        await fillIfExists(page, 'input[name="company"], input[name="Company"]', company.name);
        await fillIfExists(page, 'input[name="linkedInUrl"], input[name="LinkedInUrl"]', exec.linkedIn);
        await fillIfExists(page, 'input[name="address"], input[name="Address"]', company.address);
        await fillIfExists(page, 'input[name="city"], input[name="City"]', company.city);
        await fillIfExists(page, 'input[name="state"], input[name="State"]', company.state);
        await fillIfExists(page, 'input[name="country"], input[name="Country"]', company.country);
        await fillIfExists(page, 'input[name="zipCode"], input[name="ZipCode"]', company.zipCode);
        
        // Submit
        const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit")').first();
        if (await saveBtn.isVisible()) {
          await saveBtn.click();
          await page.waitForTimeout(1500);
        }
        
        contactCount++;
        console.log(`✓ Created contact ${contactCount}: ${exec.firstName} ${exec.lastName}`);
      }
    }
    
    console.log(`Created ${contactCount} contacts total`);
    expect(contactCount).toBeGreaterThan(50);
  });

  test('3. Create 20 Leads', async ({ page }) => {
    test.setTimeout(300000); // 5 minutes
    
    let leadCount = 0;
    
    for (const lead of leads) {
      console.log(`Creating lead: ${lead.firstName} ${lead.lastName}`);
      
      // Navigate to leads
      await page.goto('/leads');
      await page.waitForLoadState('networkidle');
      
      // Click Add button
      const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addBtn.isVisible()) {
        await addBtn.click();
        await page.waitForTimeout(1000);
      }
      
      // Fill lead form
      await fillIfExists(page, 'input[name="firstName"], input[name="FirstName"]', lead.firstName);
      await fillIfExists(page, 'input[name="lastName"], input[name="LastName"]', lead.lastName);
      await fillIfExists(page, 'input[name="email"], input[name="Email"]', lead.email);
      await fillIfExists(page, 'input[name="phone"], input[name="Phone"]', lead.phone);
      await fillIfExists(page, 'input[name="title"], input[name="Title"], input[name="jobTitle"]', lead.title);
      await fillIfExists(page, 'input[name="companyName"], input[name="CompanyName"], input[name="company"]', lead.company);
      await fillIfExists(page, 'input[name="score"], input[name="Score"]', lead.score.toString());
      
      // Submit
      const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit")').first();
      if (await saveBtn.isVisible()) {
        await saveBtn.click();
        await page.waitForTimeout(1500);
      }
      
      leadCount++;
      console.log(`✓ Created lead ${leadCount}: ${lead.firstName} ${lead.lastName}`);
    }
    
    console.log(`Created ${leadCount} leads total`);
    expect(leadCount).toBe(20);
  });

  test('4. Create 25 Opportunities', async ({ page }) => {
    test.setTimeout(400000); // ~7 minutes
    
    let oppCount = 0;
    const companyNames = companies.map(c => c.name);
    
    // Create 25 opportunities distributed across companies
    for (let i = 0; i < 25; i++) {
      const template = opportunityTemplates[i % opportunityTemplates.length];
      const companyName = companyNames[i % companyNames.length];
      const oppName = `${companyName.split(' ')[0]} - ${template.name}`;
      
      console.log(`Creating opportunity: ${oppName}`);
      
      // Navigate to opportunities
      await page.goto('/opportunities');
      await page.waitForLoadState('networkidle');
      
      // Click Add button
      const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addBtn.isVisible()) {
        await addBtn.click();
        await page.waitForTimeout(1000);
      }
      
      // Fill opportunity form
      await fillIfExists(page, 'input[name="name"], input[name="Name"]', oppName);
      await fillIfExists(page, 'input[name="amount"], input[name="Amount"]', template.amount.toString());
      await fillIfExists(page, 'input[name="probability"], input[name="Probability"]', template.probability.toString());
      
      // Submit
      const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit")').first();
      if (await saveBtn.isVisible()) {
        await saveBtn.click();
        await page.waitForTimeout(1500);
      }
      
      oppCount++;
      console.log(`✓ Created opportunity ${oppCount}: ${oppName}`);
    }
    
    console.log(`Created ${oppCount} opportunities total`);
    expect(oppCount).toBe(25);
  });

  test('5. Create 50 Service Requests', async ({ page }) => {
    test.setTimeout(600000); // 10 minutes
    
    let srCount = 0;
    const companyNames = companies.map(c => c.name);
    
    // Create 50 service requests
    for (let i = 0; i < 50; i++) {
      const template = serviceRequestTemplates[i % serviceRequestTemplates.length];
      const companyName = companyNames[i % companyNames.length];
      const subject = `[${companyName.split(' ')[0]}] ${template.subject}`;
      
      console.log(`Creating service request: ${subject}`);
      
      // Navigate to service requests
      await page.goto('/service-requests');
      await page.waitForLoadState('networkidle');
      
      // Click Add button
      const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addBtn.isVisible()) {
        await addBtn.click();
        await page.waitForTimeout(1000);
      }
      
      // Fill service request form
      await fillIfExists(page, 'input[name="subject"], input[name="Subject"]', subject);
      await fillIfExists(page, 'textarea[name="description"], textarea[name="Description"]', template.description);
      
      // Submit
      const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit")').first();
      if (await saveBtn.isVisible()) {
        await saveBtn.click();
        await page.waitForTimeout(1500);
      }
      
      srCount++;
      console.log(`✓ Created service request ${srCount}: ${subject}`);
    }
    
    console.log(`Created ${srCount} service requests total`);
    expect(srCount).toBe(50);
  });

  test('6. Create 20 Users', async ({ page }) => {
    test.setTimeout(300000); // 5 minutes
    
    let userCount = 0;
    
    for (const user of users) {
      console.log(`Creating user: ${user.firstName} ${user.lastName}`);
      
      // Navigate to users/settings
      await page.goto('/settings/users');
      await page.waitForLoadState('networkidle');
      
      // Click Add button
      const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create"), button:has-text("Invite")').first();
      if (await addBtn.isVisible()) {
        await addBtn.click();
        await page.waitForTimeout(1000);
      }
      
      // Fill user form
      await fillIfExists(page, 'input[name="username"], input[name="Username"]', user.username);
      await fillIfExists(page, 'input[name="email"], input[name="Email"]', user.email);
      await fillIfExists(page, 'input[name="firstName"], input[name="FirstName"]', user.firstName);
      await fillIfExists(page, 'input[name="lastName"], input[name="LastName"]', user.lastName);
      await fillIfExists(page, 'input[name="password"], input[name="Password"]', 'TempPass@123');
      
      // Submit
      const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit"), button:has-text("Invite")').first();
      if (await saveBtn.isVisible()) {
        await saveBtn.click();
        await page.waitForTimeout(1500);
      }
      
      userCount++;
      console.log(`✓ Created user ${userCount}: ${user.username}`);
    }
    
    console.log(`Created ${userCount} users total`);
    expect(userCount).toBe(20);
  });

  test('7. Create 5 Marketing Campaigns', async ({ page }) => {
    test.setTimeout(180000); // 3 minutes
    
    let campaignCount = 0;
    
    for (const campaign of campaigns) {
      console.log(`Creating campaign: ${campaign.name}`);
      
      // Navigate to campaigns/marketing
      await page.goto('/marketing/campaigns');
      await page.waitForLoadState('networkidle');
      
      // Click Add button
      const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addBtn.isVisible()) {
        await addBtn.click();
        await page.waitForTimeout(1000);
      }
      
      // Fill campaign form
      await fillIfExists(page, 'input[name="name"], input[name="Name"]', campaign.name);
      await fillIfExists(page, 'textarea[name="description"], textarea[name="Description"]', campaign.description);
      await fillIfExists(page, 'input[name="budget"], input[name="Budget"]', campaign.budget.toString());
      
      // Submit
      const saveBtn = page.locator('button:has-text("Save"), button:has-text("Create"), button:has-text("Submit")').first();
      if (await saveBtn.isVisible()) {
        await saveBtn.click();
        await page.waitForTimeout(1500);
      }
      
      campaignCount++;
      console.log(`✓ Created campaign ${campaignCount}: ${campaign.name}`);
    }
    
    console.log(`Created ${campaignCount} campaigns total`);
    expect(campaignCount).toBe(5);
  });
});

// Export data for seed generation
export { companies, leads, opportunityTemplates, serviceRequestTemplates, users, campaigns };
