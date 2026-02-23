import sys

path = '/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/src/CRM.Api/Controllers/AdminConfigurationController.cs'
with open(path, 'r') as f:
    content = f.read()

replacements = [
    ('CreateCommissionRuleAsync(request, cancellationToken)', 'CreateCommissionRuleAsync(request, null, cancellationToken)'),
    ('UpdateCommissionRuleAsync(id, request, cancellationToken)', 'UpdateCommissionRuleAsync(id, request, null, cancellationToken)'),
    ('DeleteCommissionRuleAsync(id, cancellationToken)', 'DeleteCommissionRuleAsync(id, null, cancellationToken)'),
    ('CreateDiscountRuleAsync(request, cancellationToken)', 'CreateDiscountRuleAsync(request, null, cancellationToken)'),
    ('UpdateDiscountRuleAsync(id, request, cancellationToken)', 'UpdateDiscountRuleAsync(id, request, null, cancellationToken)'),
    ('DeleteDiscountRuleAsync(id, cancellationToken)', 'DeleteDiscountRuleAsync(id, null, cancellationToken)'),
    ('CreateSLAPolicyAsync(request, cancellationToken)', 'CreateSLAPolicyAsync(request, null, cancellationToken)'),
    ('UpdateSLAPolicyAsync(id, request, cancellationToken)', 'UpdateSLAPolicyAsync(id, request, null, cancellationToken)'),
    ('DeleteSLAPolicyAsync(id, cancellationToken)', 'DeleteSLAPolicyAsync(id, null, cancellationToken)'),
    ('CreateEscalationRuleAsync(request, cancellationToken)', 'CreateEscalationRuleAsync(request, null, cancellationToken)'),
    ('UpdateEscalationRuleAsync(id, request, cancellationToken)', 'UpdateEscalationRuleAsync(id, request, null, cancellationToken)'),
    ('DeleteEscalationRuleAsync(id, cancellationToken)', 'DeleteEscalationRuleAsync(id, null, cancellationToken)'),
    ('CreateServiceQueueAsync(request, cancellationToken)', 'CreateServiceQueueAsync(request, null, cancellationToken)'),
    ('UpdateServiceQueueAsync(id, request, cancellationToken)', 'UpdateServiceQueueAsync(id, request, null, cancellationToken)'),
    ('DeleteServiceQueueAsync(id, cancellationToken)', 'DeleteServiceQueueAsync(id, null, cancellationToken)'),
]

total = 0
for old, new in replacements:
    n = content.count(old)
    content = content.replace(old, new)
    total += n
    print(f'  {n}x: {old[:60]}')

with open(path, 'w') as f:
    f.write(content)
print(f'Total: {total} replacements done')
