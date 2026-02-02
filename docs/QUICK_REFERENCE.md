# Quick Reference Guide - Architecture Review

**For Developers:** Quick access to key findings and action items

---

## 🚨 Top Priority Actions

### 1. Stop Using TypeScript 'any' ⛔

**Problem:** 54 instances found, causes runtime errors

**❌ Don't Do This:**
```typescript
const data = response.data as any;
const value = (formData as any)[fieldName];
```

**✅ Do This Instead:**
```typescript
import { Customer } from '../types/entities';
import { getFormValue } from '../types/forms';

const data = response.data as Customer;
const value = getFormValue<string>(formData, fieldName);
```

**New Types Available:**
- `types/permissions.ts` - Permission system
- `types/forms.ts` - Form handling
- `types/entities.ts` - All CRM entities

---

### 2. Fix React Hooks Dependencies 🔧

**Problem:** 7+ disabled ESLint rules hiding bugs

**❌ Don't Do This:**
```typescript
useEffect(() => {
  fetchData(id, filter);
}, [id]); // eslint-disable-line react-hooks/exhaustive-deps
```

**✅ Do This Instead:**
```typescript
const fetchData = useCallback(async () => {
  const result = await apiClient.get('/data', { id, filter });
  setData(result);
}, [id, filter]);

useEffect(() => {
  fetchData();
}, [fetchData]);
```

---

### 3. Add Tests for Your Code 🧪

**Problem:** Frontend coverage only 41%

**Required Coverage:**
- Services: 80%+
- Utilities: 90%+
- Components: 60%+
- Hooks: 70%+

**Template:**
```typescript
// customerService.test.ts
import { customerService } from './customerService';
import axios from 'axios';

jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('customerService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should fetch customer by id', async () => {
    const mockCustomer = { id: 1, name: 'Test' };
    mockedAxios.get.mockResolvedValue({ data: mockCustomer });
    
    const result = await customerService.getById(1);
    
    expect(result).toEqual(mockCustomer);
    expect(mockedAxios.get).toHaveBeenCalledWith('/api/customers/1');
  });
});
```

---

### 4. Handle Errors Properly 🛡️

**Problem:** Silent failures, generic messages

**❌ Don't Do This:**
```typescript
try {
  await someAction();
} catch (e) {
  console.warn('Failed', e); // User never sees error!
}
```

**✅ Do This Instead:**
```typescript
try {
  await someAction();
} catch (error) {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 400) {
      showError(error.response.data.message);
    } else if (error.response?.status === 401) {
      showError('Unauthorized. Please log in.');
      navigate('/login');
    } else {
      showError('An unexpected error occurred');
    }
  } else {
    showError('Network error. Please try again.');
  }
  logError(error); // Always log for debugging
}
```

---

## 📋 Code Review Checklist

Before submitting PR:

- [ ] **No TypeScript 'any' casts** - Use proper types
- [ ] **Tests added** - For new code
- [ ] **Error handling** - All async ops have try-catch
- [ ] **Dependencies complete** - No eslint-disable for exhaustive-deps
- [ ] **Components small** - < 150 lines
- [ ] **No console.log** - Use logger service
- [ ] **ESLint passes** - `npm run lint`
- [ ] **Tests pass** - `npm test`

---

## 📚 Key Documents

| Document | Purpose | Location |
|----------|---------|----------|
| **Coding Standards** | How to write code | `docs/CODING_STANDARDS.md` |
| **Architecture Decisions** | Why we made choices | `docs/ARCHITECTURE_DECISIONS.md` |
| **Testing Strategy** | How to test | `docs/TESTING_STRATEGY.md` |
| **Stabilization TODO** | What needs fixing | `STABILIZATION_TODO.md` |
| **Review Summary** | Overall findings | `ARCHITECTURE_REVIEW_SUMMARY.md` |

---

## 🔍 Common Issues & Solutions

### Issue: Large Component (150+ lines)

**Solution:** Extract sub-components
```typescript
// Before: One large component
function CustomerPage() {
  // 200 lines of code...
}

// After: Multiple focused components
function CustomerPage() {
  return (
    <>
      <CustomerHeader />
      <CustomerForm onSubmit={handleSubmit} />
      <CustomerList items={customers} />
    </>
  );
}
```

### Issue: Multiple useState Hooks

**Solution:** Use useReducer
```typescript
// Before: Multiple related useState
const [loading, setLoading] = useState(false);
const [error, setError] = useState(null);
const [data, setData] = useState([]);

// After: Single useReducer
const [state, dispatch] = useReducer(reducer, {
  loading: false,
  error: null,
  data: [],
});
```

### Issue: Direct localStorage Access

**Solution:** Use StorageService (to be created)
```typescript
// Before
localStorage.setItem('token', token);

// After
StorageService.set('token', token, true); // Encrypted
```

---

## 🎯 Current Priorities (Next 2 Weeks)

1. **Week 1:**
   - Replace 20+ 'as any' casts in high-traffic files
   - Add tests for customerService, opportunityService
   - Fix Navigation.tsx React hooks violations

2. **Week 2:**
   - Replace remaining 'as any' casts
   - Add tests for custom hooks (useCustomer, usePermissions)
   - Fix auth redirect vulnerability

---

## 🚀 Quick Commands

```bash
# Type check
npx tsc --noEmit

# Lint check
npm run lint

# Run tests
npm test

# Test with coverage
npm test -- --coverage

# Find 'any' usage
grep -r "as any" src/ --include="*.ts" --include="*.tsx"

# Find disabled eslint rules
grep -r "eslint-disable" src/ --include="*.ts" --include="*.tsx"
```

---

## 💡 Pro Tips

1. **Use Type Guards**: Create `is*` functions for runtime checking
2. **Extract Custom Hooks**: Reuse logic across components
3. **Keep Components Pure**: Separate business logic
4. **Write Tests First**: TDD for critical features
5. **Review Your Own PR**: Before asking others

---

## ❓ Need Help?

- **Type Safety Issues:** See `types/` folder for examples
- **Testing Questions:** See `TESTING_STRATEGY.md`
- **Code Standards:** See `CODING_STANDARDS.md`
- **Architecture Questions:** See `ARCHITECTURE_DECISIONS.md`

---

## 📊 Success Metrics

Track your impact:

| Metric | Current | Your Target |
|--------|---------|-------------|
| 'any' casts in your files | ? | 0 |
| Test coverage | ? | 70%+ |
| Component size | ? | <150 lines |
| ESLint violations | ? | 0 |

---

**Remember:** Clean code is not written by following rules. Clean code is written by caring about your craft. 🎨

**Last Updated:** February 2, 2026  
**Version:** 1.0
