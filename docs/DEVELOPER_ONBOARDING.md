# Developer Onboarding Guide

> **Welcome to the CRM Solution Development Team!**  
> Version: 0.614.84  
> Last Updated: March 3, 2026

---

## Table of Contents

1. [Welcome](#welcome)
2. [Day 1: Environment Setup](#day-1-environment-setup)
3. [Day 2-3: Codebase Exploration](#day-2-3-codebase-exploration)
4. [Week 1: First Contribution](#week-1-first-contribution)
5. [Ongoing: Development Workflow](#ongoing-development-workflow)
6. [Resources & Support](#resources--support)

---

## Welcome

### About This Project

You're joining the development of a **comprehensive enterprise CRM solution** with:

- **~1 million lines of code** across backend, frontend, and infrastructure
- **Full-stack architecture**: ASP.NET Core 10 + React 18 + TypeScript
- **Modern practices**: Hexagonal architecture, SOLID principles, TDD
- **AI-powered features**: 19 specialized agents using Semantic Kernel
- **Multi-database support**: MariaDB, PostgreSQL, SQL Server, SQLite
- **Cloud-ready**: Docker, Kubernetes, Azure, AWS, GCP

### Team Structure

| Role | Responsibilities |
|------|------------------|
| **Backend Developers** | API development, services, EF Core, testing |
| **Frontend Developers** | React components, state management, UI/UX |
| **DevOps Engineers** | CI/CD, Docker, Kubernetes, cloud deployment |
| **QA Engineers** | Test automation, E2E testing, quality assurance |
| **Tech Leads** | Architecture decisions, code reviews, mentoring |

### Communication Channels

- **Slack:** #crm-dev (general), #crm-backend, #crm-frontend
- **GitHub:** Issues, Pull Requests, Discussions
- **Daily Standup:** 9:30 AM (remote-friendly)
- **Sprint Planning:** Every 2 weeks

---

## Day 1: Environment Setup

### 1. Hardware Requirements

**Minimum:**
- 16 GB RAM
- 50 GB free disk space
- Modern CPU (4+ cores)

**Recommended:**
- 32 GB RAM
- 100 GB free SSD space
- 8+ core CPU

### 2. Install Required Software

#### macOS

```bash
# Install Homebrew
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install development tools
brew install git
brew install --cask docker
brew install dotnet@10
brew install node@20
brew install python@3.12
brew install postgresql@16  # Optional: for local DB

# Start Docker Desktop
open /Applications/Docker.app
```

#### Windows

```powershell
# Install via Chocolatey
choco install git
choco install docker-desktop
choco install dotnet-10.0-sdk
choco install nodejs-lts
choco install python312

# Or download installers from official websites
```

#### Linux (Ubuntu/Debian)

```bash
# Update package list
sudo apt update

# Install Git
sudo apt install git

# Install Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Install Node.js 20
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install nodejs

# Install Python 3.12
sudo apt install python3.12 python3.12-venv
```

### 3. Clone Repository

```bash
# Clone the repository
git clone https://github.com/your-org/crm-solution.git
cd crm-solution

# Check out develop branch (default branch for development)
git checkout develop

# Create your feature branch
git checkout -b feature/your-name-init
```

### 4. IDEs & Extensions

#### Visual Studio Code (Recommended for Full-Stack)

```bash
# Install VS Code
# macOS: brew install --cask visual-studio-code
# Windows: choco install vscode
# Linux: sudo snap install code --classic

# Install essential extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension dsznajder.es7-react-js-snippets
code --install-extension dbaeumer.vscode-eslint
code --install-extension esbenp.prettier-vscode
code --install-extension ms-azuretools.vscode-docker
code --install-extension ms-kubernetes-tools.vscode-kubernetes-tools
code --install-extension GitHub.copilot  # If you have access
```

#### Visual Studio 2022 (Recommended for Backend)

Download: https://visualstudio.microsoft.com/downloads/

**Workloads to install:**
- ASP.NET and web development
- .NET desktop development
- Azure development (optional)

#### Rider (Alternative for Backend)

Download: https://www.jetbrains.com/rider/

### 5. Backend Setup

```bash
# Navigate to backend
cd CRM.Backend

# Restore NuGet packages
dotnet restore CRM.sln

# Build solution
dotnet build CRM.sln

# Verify build
# Should see: Build succeeded. 0 Warning(s). 0 Error(s).
```

### 6. Frontend Setup

```bash
# Navigate to frontend
cd CRM.Frontend

# Install npm packages
npm install

# Verify installation
npm run build
# Should complete without errors
```

### 7. Database Setup

**Option A: Use Remote Dev Database (Recommended for Day 1)**

```bash
# Configuration already points to 192.168.0.9
# No setup needed - database is shared
```

**Option B: Local Database with Docker**

```bash
# Start MariaDB and Redis
docker-compose -f docker/docker-compose.databases.yml up -d crm-mariadb crm-redis

# Wait for database to be ready (30 seconds)
sleep 30

# Apply migrations
cd CRM.Backend/src/CRM.Api
dotnet ef database update

# Seed sample data
curl -X POST http://localhost:5000/api/sampledata/seed
```

### 8. Run the Application

**Terminal 1 - Backend:**
```bash
cd CRM.Backend/src/CRM.Api
dotnet run
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

**Terminal 2 - Frontend:**
```bash
cd CRM.Frontend
npm start
# Frontend: http://localhost:3000
```

**Terminal 3 - CDT (Optional):**
```bash
cd CRM.Infrastructure/deployment-tool
source .venv/bin/activate
python gui/app.py
# CDT: http://localhost:5050
```

### 9. Verify Setup

```bash
# Test backend health
curl http://localhost:5000/health
# Expected: {"status":"Healthy"}

# Test frontend
open http://localhost:3000
# Login: admin@crm.local / Admin@123
```

### 10. Run Tests

```bash
# Backend tests
cd CRM.Backend
dotnet test

# Frontend tests
cd CRM.Frontend
npm test

# E2E tests (optional for Day 1)
cd e2e-tests
npx playwright install
npx playwright test
```

---

## Day 2-3: Codebase Exploration

### Architecture Overview

Spend time understanding the architecture before diving into code.

#### Backend Architecture

```
CRM.Backend/
├── src/
│   ├── CRM.Api/           # REST API, Controllers, Middleware
│   ├── CRM.Core/          # Domain layer (Entities, DTOs, Interfaces)
│   └── CRM.Infrastructure # Services, Repositories, Providers
└── tests/                 # Unit & Integration tests
```

**Key concepts to understand:**
1. **Hexagonal Architecture** - Domain at center, dependencies point inward
2. **Dependency Injection** - All services registered in `Program.cs`
3. **Entity Framework Core** - ORM for database access
4. **DTO Pattern** - Never expose entities directly in API

**Read these files first:**
- `CRM.Backend/src/CRM.Api/Program.cs` - Application startup
- `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs` - Database context
- `CRM.Backend/src/CRM.Core/Entities/BaseEntity.cs` - Entity base class
- Any `I*Service.cs` file in `CRM.Core/Interfaces` - Service contracts

#### Frontend Architecture

```
CRM.Frontend/src/
├── pages/          # Route-level components (186 pages)
├── components/     # Reusable UI components
├── services/       # API communication layer (78 services)
├── contexts/       # React Context providers (13 contexts)
└── hooks/          # Custom React hooks (14 hooks)
```

**Key concepts:**
1. **React 18 + TypeScript** - Type-safe component development
2. **Material-UI** - Component library
3. **React Router v6** - Client-side routing
4. **Context API** - State management (no Redux)

**Read these files first:**
- `CRM.Frontend/src/App.tsx` - Root component
- `CRM.Frontend/src/routes.tsx` - Route definitions
- `CRM.Frontend/src/services/apiClient.ts` - Axios setup
- `CRM.Frontend/src/contexts/AuthContext.tsx` - Authentication

### Explore Key Features

**Day 2: Core CRM Features**

1. **Accounts (Customers)**
   - Backend: `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs`
   - Frontend: `CRM.Frontend/src/pages/CustomersPage.tsx`
   - API: `GET/POST/PUT/DELETE /api/accounts`

2. **Contacts**
   - Backend: `CRM.Backend/src/CRM.Infrastructure/Services/ContactService.cs`
   - Frontend: `CRM.Frontend/src/pages/ContactsPage.tsx`
   - API: `GET/POST/PUT/DELETE /api/contacts`

3. **Opportunities**
   - Backend: `CRM.Backend/src/CRM.Infrastructure/Services/OpportunityService.cs`
   - Frontend: `CRM.Frontend/src/pages/OpportunitiesPage.tsx`
   - API: `GET/POST/PUT/DELETE /api/opportunities`

**Day 3: Advanced Features**

1. **ITSM Module** (Service Desk)
   - Service Requests, Knowledge Base, Workflows
   - Location: `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/`

2. **AI Agents** (Semantic Kernel)
   - 19 specialized agents
   - Location: `CRM.Backend/src/CRM.Infrastructure/AI/SK/`

3. **Pluggable Providers**
   - Search (Meilisearch), AI (Ollama), Chat (Chatwoot)
   - Location: `CRM.Backend/src/CRM.Infrastructure/Providers/`

### Code Reading Exercises

**Exercise 1: Follow a Request**

Trace an API request from frontend to database:
1. Frontend makes request: `accountService.getAll()`
2. Axios sends HTTP GET to `/api/accounts`
3. `AccountsController.GetAll()` receives request
4. Controller calls `IAccountService.GetAllAsync()`
5. `AccountService` queries `DbContext.Accounts`
6. EF Core generates SQL query
7. Database returns results
8. Service maps entities to DTOs
9. Controller returns JSON response
10. Frontend receives and displays data

**Exercise 2: Create a Feature**

Mentally design how you would add a "Notes" feature to Accounts:
1. Create `Note` entity in `CRM.Core/Entities/`
2. Add `DbSet<Note>` to `CrmDbContext`
3. Create migration: `dotnet ef migrations add AddNotes`
4. Create `NoteDto` in `CRM.Core/Dtos/`
5. Create `INoteService` interface
6. Implement `NoteService`
7. Create `NotesController`
8. Write unit tests
9. Create frontend `noteService.ts`
10. Create `NotesPage.tsx` component

---

## Week 1: First Contribution

### Finding a Good First Issue

Look for issues labeled:
- `good-first-issue` - Beginner-friendly
- `documentation` - Documentation improvements
- `bug` - Bug fixes (small bugs are good starters)
- `frontend` or `backend` - Based on your expertise

**Example first issues:**
1. Fix a typo in documentation
2. Add validation message to a form
3. Write missing unit test
4. Update outdated README section

### Development Workflow

#### 1. Create Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/add-notes-to-accounts
```

**Branch naming conventions:**
- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Critical production fixes
- `refactor/` - Code refactoring
- `docs/` - Documentation changes

#### 2. Make Changes

Follow coding standards:
- **Backend:** StyleCop rules, XML comments, async/await
- **Frontend:** ESLint rules, TypeScript strict mode
- **Tests:** Write tests for all new code

```bash
# Backend example: Add Note entity
# 1. Create entity
# 2. Add DbSet
# 3. Create migration
# 4. Create DTO
# 5. Create service
# 6. Create controller
# 7. Write tests
```

#### 3. Test Your Changes

```bash
# Run all tests
cd CRM.Backend && dotnet test
cd CRM.Frontend && npm test

# Run specific tests
dotnet test --filter "FullyQualifiedName~AccountServiceTests"

# Manual testing
dotnet run (backend)
npm start (frontend)
```

#### 4. Commit Changes

```bash
git add .
git commit -m "feat(accounts): add notes feature

- Add Note entity and DbSet
- Create NoteService and controller
- Add frontend NotesPage component
- Write unit tests"
```

**Commit message format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:** `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

#### 5. Push and Create Pull Request

```bash
git push origin feature/add-notes-to-accounts
```

Then create PR on GitHub with:
- Descriptive title
- Detailed description
- Link to issue (#123)
- Screenshots (for UI changes)
- Checklist of changes

**PR Template:**
```markdown
## Description
Brief description of changes

## Related Issue
Closes #123

## Changes Made
- [ ] Added Note entity
- [ ] Created NoteService
- [ ] Added NotesPage component
- [ ] Wrote unit tests

## Screenshots
(if applicable)

## Testing
- [ ] Backend tests pass
- [ ] Frontend tests pass
- [ ] Manual testing completed
```

#### 6. Code Review

Respond to feedback professionally:
- Address all comments
- Ask questions if unclear
- Make requested changes
- Re-request review when ready

#### 7. Merge

Once approved:
- Squash commits if needed
- Merge to `develop` branch
- Delete feature branch

---

## Ongoing: Development Workflow

### Daily Workflow

**Morning:**
1. Pull latest changes: `git pull origin develop`
2. Check Slack for updates
3. Review assigned issues/PRs
4. Join standup (9:30 AM)

**During Day:**
5. Work on assigned tasks
6. Commit frequently (small, focused commits)
7. Push to remote at least once per day
8. Respond to PR comments
9. Help teammates if blocked

**End of Day:**
10. Push all work-in-progress
11. Update issue status
12. Plan tomorrow's work

### Testing Practices

**Before Every Commit:**
```bash
# Run relevant tests
dotnet test  # Backend
npm test     # Frontend

# Check for build errors
dotnet build --no-restore
npm run build

# Check code quality
dotnet format  # Backend
npm run lint   # Frontend
```

**Write Tests For:**
- All new services and controllers
- All new React components
- All bug fixes
- All API endpoints

### Code Quality Standards

**Backend:**
- Follow StyleCop rules
- Add XML documentation comments
- Use async/await properly
- Pass CancellationToken to all async methods
- Use dependency injection
- Never expose entities in API (use DTOs)

**Frontend:**
- Follow ESLint rules
- Use TypeScript strict mode
- Avoid `any` type
- Use functional components with hooks
- Memoize expensive computations
- Handle loading and error states

### Git Best Practices

✅ **DO:**
- Commit frequently with clear messages
- Write descriptive commit messages
- Keep commits focused (one logical change per commit)
- Pull before pushing
- Review your own changes before PR

❌ **DON'T:**
- Commit directly to `main` or `develop`
- Push broken code
- Commit secrets or credentials
- Create giant PRs (>500 lines)
- Force push to shared branches

---

## Resources & Support

### Documentation

| Resource | Link |
|----------|------|
| **Backend Developer Guide** | [docs/backend/DEVELOPER_GUIDE.md](backend/DEVELOPER_GUIDE.md) |
| **Frontend Developer Guide** | [docs/frontend/DEVELOPER_GUIDE.md](frontend/DEVELOPER_GUIDE.md) |
| **Testing Guide** | [docs/testing/TESTING_GUIDE.md](testing/TESTING_GUIDE.md) |
| **Docker Guide** | [docs/deployment/DOCKER_GUIDE.md](deployment/DOCKER_GUIDE.md) |
| **CDT Guide** | [docs/deployment/CDT_GUIDE.md](deployment/CDT_GUIDE.md) |
| **Copilot Instructions** | [.github/copilot-instructions.md](../.github/copilot-instructions.md) |

### Learning Resources

**Backend (.NET):**
- Microsoft Learn: https://learn.microsoft.com/aspnet/core
- Entity Framework Core: https://learn.microsoft.com/ef/core
- Clean Architecture: https://github.com/jasontaylordev/CleanArchitecture

**Frontend (React):**
- React Documentation: https://react.dev/
- TypeScript Handbook: https://www.typescriptlang.org/docs/
- Material-UI: https://mui.com/material-ui/getting-started/

**Testing:**
- xUnit: https://xunit.net/
- Jest: https://jestjs.io/
- Playwright: https://playwright.dev/

### Getting Help

**Stuck on something?**

1. **Search documentation** - Check relevant developer guide first
2. **Search codebase** - Look for similar implementations
3. **Ask teammate** - Slack #crm-dev channel
4. **Create GitHub Discussion** - For design questions
5. **Ask in standup** - If blocking progress

**Escalation Path:**
1. Teammate → 2. Tech Lead → 3. Engineering Manager

### Common Issues & Solutions

See: [docs/common_development_issues.md](common_development_issues.md)

---

## Next Steps

### Week 2+

- Pick up medium complexity issues
- Review others' PRs
- Improve test coverage
- Refactor legacy code
- Optimize performance

### Month 2+

- Lead feature development
- Mentor new developers
- Contribute to architecture decisions
- Write documentation
- Present at team meetings

### Month 3+

- Become domain expert in one module
- Drive technical initiatives
- Participate in sprint planning
- Improve development processes

---

## Welcome Aboard!

You're now ready to contribute to the CRM solution. Remember:

- **Ask questions** - No question is too small
- **Start small** - Build confidence with small wins
- **Write tests** - Quality over speed
- **Help others** - Knowledge sharing strengthens the team
- **Have fun** - We're building something great together!

---

**Document Version:** 1.0  
**Last Updated:** March 3, 2026  
**Maintained By:** CRM Development Team
