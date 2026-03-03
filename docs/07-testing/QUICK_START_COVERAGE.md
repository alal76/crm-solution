# Test Coverage Improvement - Quick Start Guide

**Status:** Ready to implement  
**Target:** 20.29% → 70%+ coverage  
**Estimated Effort:** 10-12 developer days

---

## Files Created

### 1. Analysis & Planning
- ✅ `docs/07-testing/TEST_COVERAGE_ANALYSIS.md` - Full coverage analysis & implementation plan
- ✅ `analyze_test_coverage.py` - Coverage analysis script

### 2. Test Infrastructure
- ✅ `CRM.Backend/tests/Helpers/ValidatorTestFixtureBase.cs` - Base class for validator tests
- ✅ `scripts/generate_dto_tests.py` - Auto-generate DTO validation tests
- ✅ `scripts/generate_validator_tests.py` - Auto-generate validator test stubs

---

## Quick Start: Phase 1 (Validators)

### Step 1: Generate validator tests
```bash
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend
python3 ../scripts/generate_validator_tests.py \
  --src src/CRM.Core/Validation \
  --output tests/Validators
```

**Expected Output:** 13 test files for validator classes

### Step 2: Fill in test data
Edit generated files in `tests/Validators/` and replace TODOs with actual test data

### Step 3: Run tests
```bash
cd CRM.Backend
dotnet test tests/CRM.Tests.csproj --filter "Validators"
```

### Step 4: Measure coverage
```bash
cd CRM.Backend
dotnet test CRM.sln --collect:"XPlat Code Coverage" --results-directory ./tests/TestResults/phase1
cd tests
python3 ../../analyze_test_coverage.py
```

**Expected:** Coverage increases from 20.29% → ~40%

---

## Quick Start: Phase 2 (DTOs)

### Step 1: Generate DTO tests
```bash
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend
python3 ../scripts/generate_dto_tests.py \
  --src src/CRM.Core \
  --output tests/Dtos/Generated \
  --pattern "*Dto.cs"
```

**Expected Output:** 50+ test files

### Step 2: Review and customize
Generated tests will have TODOs for:
- Valid default values in `CreateValidXDto()` methods
- Property-specific test cases

### Step 3: Run tests
```bash
dotnet test tests/CRM.Tests.csproj --filter "ValidationTests"
```

**Expected:** Coverage increases to ~55%

---

## Quick Commands

### Run coverage analysis
```bash
cd CRM.Backend/tests
python3 ../../analyze_test_coverage.py
```

### Run specific test category
```bash
dotnet test --filter "Category=Validators"
dotnet test --filter "Category=DTOs"
dotnet test --filter "Category=Integration"
```

### Generate fresh coverage report
```bash
cd CRM.Backend
dotnet test CRM.sln --collect:"XPlat Code Coverage" --results-directory ./tests/TestResults/latest
```

---

## Current Coverage Breakdown

| Category | Coverage | Priority |
|----------|----------|----------|
| **Validators** | 0.0% (0/422 lines) | 🔴 CRITICAL |
| **Providers** | 0.0% (0/412 lines) | 🟡 LOW (DTOs) |
| **Services** | ~90%+ | ✅ GOOD |
| **Controllers** | ~70%+ | ✅ GOOD |
| **Other** | 21.9% | 🟠 NEEDS WORK |

---

## Measurement

### Before (v0.614.84)
- Line Coverage: 20.29%
- Branch Coverage: 16.49%
- Uncovered Classes: 1094

### Target (v0.615.x)
- Line Coverage: 70%+
- Branch Coverage: 60%+
- Uncovered Classes: <100

---

## Next Steps

1. **Today:** Run validator test generator, fill in first 3 validator tests
2. **This Week:** Complete Phase 1 (all validators)
3. **Next Week:** Phase 2 (DTO tests)
4. **Week 3-4:** Phases 3-4 (integration tests, edge cases)

---

## Help

**See full analysis:** `docs/07-testing/TEST_COVERAGE_ANALYSIS.md`  
**Report issues:** Create issue with label `testing/coverage`  
**Questions:** Ask in #testing channel
