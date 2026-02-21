import os
import re

# simple plural -> singular helper

def singularize(name: str) -> str:
    if name.endswith("ies"):
        return name[:-3] + "y"
    if name.endswith("s") and not name.endswith("ss"):
        return name[:-1]
    return name

# default values for primitive types
primitive_defaults = {
    "string": '"Test"',
    "bool": "true",
    "int": "1",
    "long": "1",
    "short": "1",
    "decimal": "1",
    "double": "1",
    "float": "1",
    "byte": "1",
    "DateTime": "DateTime.UtcNow",
}

skip_props = {"Id", "CreatedAt", "UpdatedAt", "RowVersion", "DisplayName"}

dto_dir = 'CRM.Backend/src/CRM.Core/Dtos'
dto_props = {}

# parse DTO files for properties
for root, dirs, files in os.walk(dto_dir):
    for f in files:
        if not f.endswith('.cs'):
            continue
        dto_name = os.path.splitext(f)[0]
        props = []
        path = os.path.join(root, f)
        with open(path, 'r', encoding='utf-8') as fh:
            for line in fh:
                m = re.search(r'public\s+([^\s]+)\s+([A-Za-z0-9_]+)\s*\{\s*get;\s*set;\s*\}', line)
                if not m:
                    continue
                t = m.group(1).strip()
                n = m.group(2).strip()
                if n in skip_props:
                    continue
                if '<' in t:
                    continue
                if t.endswith('Dto') and t not in primitive_defaults:
                    continue
                props.append((t, n))
        if props:
            dto_props[dto_name] = props

# walk controllers
path = 'CRM.Backend/src/CRM.Api/Controllers'
controllers = []
for root, dirs, files in os.walk(path):
    for f in files:
        if f.endswith('Controller.cs'):
            controllers.append(f)
controllers = sorted(set(controllers))
skip = {'AuthController','AccountsController','HealthController','FeaturesController','FeatureFlagManagementController','WebhooksController','DocuSealWebhookController','IntercomWebhookController','NovuWebhookController','SendGridWebhookController','StripeWebhookController','TwilioWebhookController','ITSMWebhooksController','ImportExportController','ImportJobsController','AdminDashboardController','AdminSeedController','AdminSettingsController','CICDIntegrationController','CalendarIntegrationController','CloudDeploymentController','BrandingController','DashboardController','DashboardConfigController','MonitoringController','MonitoringIntegrationController','LandingPageController','ITSMDashboardController','SampleDataController','SelfServiceChatbotController','WorkflowTasksController','WorkflowTriggersController','AIChatbotController','AIAnalyticsController','AIAgentUsageController','AILeadScoringController','AIEmailController','AnalyticsController','AnalyticsEventsController'}
controllers = [c for c in controllers if c not in skip]

for c in controllers:
    entity = c.replace('Controller.cs','')
    route = entity.lower()

    # choose DTO
    base = singularize(entity)
    candidates = [f"{base}Dto", f"Create{base}Dto", f"Update{base}Dto"]
    chosen = None
    for cand in candidates:
        if cand in dto_props:
            chosen = cand
            break
    props = dto_props.get(chosen, []) if chosen else []

    def default_val(t):
        clean = t.rstrip('?')
        if clean.startswith('DateTime'):
            return "DateTime.UtcNow"
        if clean in primitive_defaults:
            return primitive_defaults[clean]
        return "null"

    if props:
        assignments = [f"{n} = {default_val(t)}" for t, n in props]
        create_init = ", ".join(assignments)
    else:
        create_init = 'name = "Test"'

    # patch initializer
    if props:
        first_string = None
        for t, n in props:
            if first_string is None and t.rstrip('?') == 'string':
                first_string = n
        patch_assignments = []
        for t, n in props:
            if n == first_string:
                patch_assignments.append(f"{n} = \"Test2\"")
            else:
                patch_assignments.append(f"{n} = {default_val(t)}")
        patch_init = ", ".join(patch_assignments)
    else:
        patch_init = 'name = "Test2"'

    print(f"--- File: CRM.Backend/tests/Integration/Controllers/{entity}ControllerTests.cs ---")
    print("using CRM.Tests.Helpers;")
    print("using FluentAssertions;")
    print("using System.Net;")
    print("using System.Net.Http.Json;")
    print("using Xunit;\n")
    print("namespace CRM.Backend.Tests.Integration.Controllers")
    print("{")
    print(f"    public class {entity}ControllerTests : IClassFixture<ApiTestFactory>")
    print("    {")
    print("        private readonly HttpClient _client;")
    print(f"        public {entity}ControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();\n")
    print("        [Fact]")
    print(f"        public async Task Crud_{entity}_Succeeds()")
    print("        {")
    print(f"            var create = new {{ {create_init} }};")
    print(f"            var cRes = await _client.PostAsJsonAsync(\"/api/{route}\", create);")
    print("            cRes.StatusCode.Should().Be(HttpStatusCode.Created);")
    print("            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();\n")
    if props:
        for _, n in props:
            print(f"            item.{n}.Should().Be(create.{n});")
        print("")
    print(f"            var getRes = await _client.GetAsync($\"/api/{route}/{{{{item.Id}}}}\");")
    print("            getRes.StatusCode.Should().Be(HttpStatusCode.OK);")
    print(f"            var patch = new {{ {patch_init} }};")
    print(f"            var pRes = await _client.PatchAsJsonAsync($\"/api/{route}/{{{{item.Id}}}}\", patch);")
    print("            pRes.StatusCode.Should().Be(HttpStatusCode.OK);")
    print(f"            var del = await _client.DeleteAsync($\"/api/{route}/{{{{item.Id}}}}\");")
    print("            del.StatusCode.Should().Be(HttpStatusCode.NoContent);")
    print(f"            var nf = await _client.GetAsync($\"/api/{route}/{{{{item.Id}}}}\");")
    print("            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);")
    print("        }\n")
    print("        [Fact]")
    print("        public async Task Get_Nonexistent_Returns404()")
    print("        {")
    print(f"            var res = await _client.GetAsync(\"/api/{route}/999999\");")
    print("            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);")
    print("        }")
    print("    }")
    print("}\n")
