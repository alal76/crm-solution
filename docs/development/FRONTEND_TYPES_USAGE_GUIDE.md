# Frontend Type Safety & Services Quick Reference Guide

## Import Patterns

### ✅ CORRECT WAY - Import from centralized types
```typescript
import { 
  Account, 
  Quote, 
  Invoice, 
  Incident, 
  Campaign,
  PaginatedResponse 
} from '../types';
```

### ❌ WRONG WAY - Don't scatter imports
```typescript
// DON'T do this:
import { Account } from '../types/accounts';
import { Quote } from '../types/sales';
```

---

## Using Type Definitions

### Domain Models
```typescript
import { Account, Contact, Opportunity } from '../types';

// Create a typed variable
const account: Account = {
  id: 1,
  company: 'Acme Corp',
  email: 'contact@acme.com'
};

// Type-safe operations
const handleAccountUpdate = (updated: Partial<Account>) => {
  // IDE will suggest all Account properties
};
```

### Response Wrappers
```typescript
import { PaginatedResponse, ApiResponse, ApiErrorResponse } from '../types';

// Paginated list response
const response: PaginatedResponse<Account> = {
  items: [...],
  totalCount: 100,
  page: 1,
  pageSize: 20,
  totalPages: 5
};

// API response
const success: ApiResponse<Account> = {
  success: true,
  data: account
};

const error: ApiErrorResponse = {
  message: 'Account not found',
  statusCode: 404
};
```

### DTOs (Data Transfer Objects)
```typescript
import { CreateQuoteDto, UpdateQuoteDto } from '../types';

// For creating new entities
const newQuote: CreateQuoteDto = {
  accountId: 1,
  expiryDate: '2026-03-16',
  lineItems: [...]
};

// For updates
const updateQuote: UpdateQuoteDto = {
  status: QuoteStatus.Sent,
  discount: 10
};
```

---

## Using Service Layer

### Import Services
```typescript
import itsmService from '../services/itsmService';
import salesService from '../services/salesService';
import marketingService from '../services/marketingService';
```

### ITSM Service Examples
```typescript
// Get incidents
const response = await itsmService.getIncidents(1, 20);
const incidents: Incident[] = response.data.items;

// Get single incident
const incident = await itsmService.getIncidentById(123);
console.log(incident.data.title); // Fully typed

// Create incident
const newIncident = await itsmService.createIncident({
  title: 'Server Down',
  description: 'Production server not responding',
  priority: IncidentPriority.Critical,
  urgency: IncidentUrgency.High
});

// Update incident
await itsmService.updateIncident(123, {
  status: IncidentStatus.Resolved,
  resolution: 'Restarted service'
});

// Resolve incident
await itsmService.resolveIncident(123, 'Fixed database connection');

// Get SLA status
const slaStatus = await itsmService.getIncidentSLAStatus(123);
console.log(slaStatus.data.responseBreached); // Boolean
```

### Sales Service Examples
```typescript
// Get orders with pagination
const orders = await salesService.getOrders(1, 20);

// Create order
const order = await salesService.createOrder({
  accountId: 1,
  orderDate: '2026-02-16',
  lineItems: [
    { productId: 1, quantity: 2, unitPrice: 100 }
  ]
});

// Create invoice from order
const invoice = await salesService.createInvoiceFromOrder(order.data.id);

// Mark invoice as paid
await salesService.markInvoiceAsPaid(123);

// Create payment
const payment = await salesService.createPayment({
  invoiceId: 123,
  amount: 500,
  paymentDate: '2026-02-16',
  paymentMethod: PaymentMethod.CreditCard
});

// Get overdue invoices
const overdue = await salesService.getOverdueInvoices();
```

### Marketing Service Examples
```typescript
// Get campaigns
const campaigns = await marketingService.getCampaigns(1, 20);

// Create campaign
const campaign = await marketingService.createCampaign({
  name: 'Spring Sale 2026',
  channel: CampaignChannel.Email,
  startDate: '2026-03-01',
  budget: 5000
});

// Get email templates
const templates = await marketingService.getEmailTemplates();

// Create email sequence
const sequence = await marketingService.createEmailSequence({
  name: 'Welcome Series',
  steps: [
    { sequence: 1, type: SequenceStepType.Email, emailTemplateId: 1 },
    { sequence: 2, type: SequenceStepType.Delay, delayDays: 2 },
    { sequence: 3, type: SequenceStepType.Email, emailTemplateId: 2 }
  ],
  triggerType: 'automatic',
  triggerEvent: 'lead_created'
});

// Activate sequence
await marketingService.activateEmailSequence(sequence.data.id);
```

---

## Using Validation Schemas

### Order Validation
```typescript
import { orderValidationSchema, calculateOrderTotal } from '../validation/orderSchema';

// Validate form data
try {
  const validated = await orderValidationSchema.validate(formData);
  // formData is now type-safe
} catch (error) {
  console.error(error.message); // User-friendly validation message
}

// Calculate totals
const lineItems = [
  { productId: 1, quantity: 2, unitPrice: 100 }
];
const total = calculateOrderTotal(
  lineItems,
  shippingCost = 10,
  taxRate = 0.08,  // 8%
  discount = 5
);
console.log(total); // 220.6
```

### Invoice Validation
```typescript
import { 
  invoiceValidationSchema,
  calculateDueDate,
  isInvoiceOverdue,
  daysUntilDue,
  PAYMENT_TERMS
} from '../validation/invoiceSchema';

// Calculate due date based on terms
const dueDate = calculateDueDate(
  new Date(),
  PAYMENT_TERMS.NET_30  // 30 days from invoice date
);

// Check if overdue
if (isInvoiceOverdue('2026-02-01')) {
  console.log('Invoice is overdue!');
}

// Days remaining
const days = daysUntilDue('2026-03-16');
console.log(`${days} days until due`);
```

### Quote Validation
```typescript
import { quoteValidationSchema } from '../validation/quoteSchema';

// Use with Formik
<Formik
  initialValues={initialQuote}
  validationSchema={quoteValidationSchema}
  onSubmit={handleSubmit}
>
  {/* Form fields */}
</Formik>
```

---

## React Components Examples

### Using Service in Component
```typescript
import { useEffect, useState } from 'react';
import { Incident, PaginatedResponse } from '../types';
import itsmService from '../services/itsmService';

export function IncidentsList() {
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadIncidents = async () => {
      try {
        const response = await itsmService.getIncidents(1, 20);
        setIncidents(response.data.items);
      } catch (err) {
        setError('Failed to load incidents');
      } finally {
        setLoading(false);
      }
    };

    loadIncidents();
  }, []);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <List>
      {incidents.map((incident: Incident) => (
        <ListItem key={incident.id}>
          <ListItemText
            primary={incident.title}
            secondary={incident.description}
          />
        </ListItem>
      ))}
    </List>
  );
}
```

### Using Validation with Form
```typescript
import { Formik, Form, Field } from 'formik';
import { orderValidationSchema } from '../validation/orderSchema';
import { CreateOrderDto } from '../types';
import salesService from '../services/salesService';

export function CreateOrderForm() {
  const initialValues: CreateOrderDto = {
    accountId: 0,
    orderDate: '',
    lineItems: []
  };

  const handleSubmit = async (values: CreateOrderDto) => {
    try {
      const response = await salesService.createOrder(values);
      console.log('Order created:', response.data.id);
    } catch (error) {
      console.error('Failed to create order:', error);
    }
  };

  return (
    <Formik
      initialValues={initialValues}
      validationSchema={orderValidationSchema}
      onSubmit={handleSubmit}
    >
      {({ errors, touched }) => (
        <Form>
          <Field name="accountId" as={TextField} />
          {errors.accountId && touched.accountId && (
            <div>{errors.accountId}</div>
          )}
          {/* More fields... */}
        </Form>
      )}
    </Formik>
  );
}
```

---

## Common Type Patterns

### Optional Properties
```typescript
// Use Partial<T> for optional updates
const update: Partial<Account> = {
  email: 'new@example.com'
  // other properties are optional
};

// Use Pick<T, K> to select subset
type AccountPreview = Pick<Account, 'id' | 'company' | 'email'>;
```

### Array Types
```typescript
// Array of entities
const accounts: Account[] = [];

// Paginated response automatically types items
const response: PaginatedResponse<Account>;
const firstAccount: Account = response.items[0];
```

### Enums as Union Types
```typescript
// Strongly typed status
const status: IncidentStatus = IncidentStatus.Active;

// Use in conditionals
if (incident.status === IncidentStatus.Resolved) {
  // TypeScript knows this branch
}
```

---

## Error Handling

### Type-Safe Error Handling
```typescript
import { ApiErrorResponse } from '../types';

try {
  const result = await itsmService.getIncidentById(999);
} catch (error) {
  const typedError = error as ApiErrorResponse;
  console.error(typedError.message); // "Incident not found"
  console.error(typedError.statusCode); // 404
}
```

### Validation Error Handling
```typescript
import { ValidationError } from '../types';

try {
  await quoteValidationSchema.validate(data);
} catch (err) {
  if (err.name === 'ValidationError') {
    const validationError: ValidationError = {
      field: err.path,
      message: err.message
    };
    // Display user-friendly error
  }
}
```

---

## Best Practices

### ✅ DO
```typescript
// 1. Always import from central types
import { Account, Invoice } from '../types';

// 2. Use DTOs for API calls
const createDto: CreateAccountDto = { ... };
await accountService.create(createDto);

// 3. Type components properly
interface AccountProps {
  account: Account;
  onUpdate: (account: Account) => Promise<void>;
}

export const AccountCard: React.FC<AccountProps> = ({ account }) => { ... };

// 4. Use enums for status values
switch (incident.status) {
  case IncidentStatus.Active:
    // ...
}

// 5. Handle errors with proper types
const error: ApiErrorResponse = { ... };
```

### ❌ DON'T
```typescript
// 1. Don't use 'any'
const account: any = data; // WRONG

// 2. Don't duplicate type definitions
interface Account { ... } // WRONG - use from ../types

// 3. Don't ignore TypeScript errors
// @ts-ignore; // WRONG - fix the error instead

// 4. Don't use loose types
const data: object = { ... }; // TOO LOOSE
const data: Account = { ... }; // CORRECT

// 5. Don't create response wrappers yourself
const response = { success: true, data: account }; // WRONG
const response: ApiResponse<Account> = { ... }; // CORRECT
```

---

## Testing Examples

### Unit Test with Mocked Service
```typescript
import { renderHook, act, waitFor } from '@testing-library/react';
import useIncidents from '../hooks/useIncidents';

jest.mock('../services/itsmService', () => ({
  getIncidents: jest.fn()
}));

test('loads incidents', async () => {
  const mockIncidents: Incident[] = [
    { id: 1, title: 'Test', description: 'Test' }
  ];

  itsmService.getIncidents.mockResolvedValue({
    data: { items: mockedIncidents, totalCount: 1, page: 1, pageSize: 20, totalPages: 1 }
  });

  const { result } = renderHook(() => useIncidents());

  await waitFor(() => {
    expect(result.current.incidents).toHaveLength(1);
  });
});
```

---

## Need Help?

Check these resources:
- 📖 Full type definitions: `src/types/`
- 🔧 Service implementations: `src/services/`
- ✅ Validation schemas: `src/validation/`
- 📝 Component examples: `src/pages/`
- 🧪 Tests: `src/__tests__/`
