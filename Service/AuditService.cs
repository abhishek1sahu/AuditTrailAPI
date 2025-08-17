using AuditService.DTO;
using AuditService.Repository.IRepository;
using AuditService.Service.IService;
using System.Text.Json;

namespace AuditService.Service
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IAuditRepository repository, ILogger<AuditService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<AuditResponse> CreateAuditEntryAsync(AuditRequest request)
        {
            try
            {
                    _logger.LogInformation("Creating audit entry for entity {EntityName} with ID {EntityId}",
                        request.EntityName, request.EntityId);

                    var auditEntry = new AuditEntry
                    {
                        EntityName = request.EntityName,
                        EntityId = request.EntityId,
                        Action = request.Action,
                        UserId = request.UserId,
                        Timestamp = DateTime.UtcNow
                    };

                    // Set additional metadata if provided
                    if (request.AdditionalMetadata != null)
                    {
                        auditEntry.AdditionalMetadata = JsonSerializer.Serialize(request.AdditionalMetadata);
                    }

                // Compare objects and extract changes 
                // objectbefore and after should be calculated via frontend when it is apssed or it should be in the same method

                var before = ParseToJsonElement(request.ObjectBefore ?? default);
                var after = ParseToJsonElement(request.ObjectAfter ?? default);


                var changes = ExtractChanges(before, after, request.Action);
                    auditEntry.FieldChanges = changes;
                    auditEntry.Changes = JsonSerializer.Serialize(changes.Select(c => new
                    {
                        c.FieldName,
                        c.OldValue,
                        c.NewValue,
                        c.FieldType
                    }));

                    var savedEntry = await _repository.CreateAuditEntryAsync(auditEntry);

                _logger.LogInformation("Successfully created audit entry with ID {AuditId}", savedEntry.Id);

                return MapToAuditResponse(savedEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit entry for entity {EntityName}", request.EntityName);
                throw;
            }
        }

        public async Task<PagedResult<AuditResponse>> GetAuditEntriesAsync(AuditQueryRequest query)
        {
            try
            {
                var result = await _repository.GetAuditEntriesAsync(query);

                return new PagedResult<AuditResponse>
                {
                    Items = result.Items.Select(MapToAuditResponse).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit entries");
                throw;
            }
        }

        public async Task<AuditResponse?> GetAuditEntryByIdAsync(int id)
        {
            try
            {
                var entry = await _repository.GetAuditEntryByIdAsync(id);
                return entry != null ? MapToAuditResponse(entry) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit entry with ID {AuditId}", id);
                throw;
            }
        }

        public async Task<List<AuditResponse>> GetEntityHistoryAsync(string entityName, string entityId)
        {
            try
            {
                var entries = await _repository.GetEntityHistoryAsync(entityName, entityId);
                return entries.Select(MapToAuditResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history for entity {EntityName} with ID {EntityId}",
                    entityName, entityId);
                throw;
            }
        }

        private List<AuditFieldChange> ExtractChanges(JsonElement? objectBefore, JsonElement? objectAfter, AuditAction action)
        {
            var changes = new List<AuditFieldChange>();

            try
            {
                switch (action)
                {
                    case AuditAction.Created:
                        if (objectAfter.HasValue)
                            changes.AddRange(ExtractCreatedChanges(objectAfter.Value));
                        break;

                    case AuditAction.Updated:
                        if (objectBefore.HasValue && objectAfter.HasValue)
                            changes.AddRange(ExtractUpdatedChanges(objectBefore.Value, objectAfter.Value));
                        break;

                    case AuditAction.Deleted:
                        if (objectBefore.HasValue)
                            changes.AddRange(ExtractDeletedChanges(objectBefore.Value));
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting changes for action {Action}", action);
            }

            return changes;
        }

        private List<AuditFieldChange> ExtractCreatedChanges(JsonElement obj)
        {
            var changes = new List<AuditFieldChange>();

            foreach (var property in obj.EnumerateObject())
            {
                changes.Add(new AuditFieldChange
                {
                    FieldName = property.Name,
                    OldValue = null,
                    NewValue = GetValueAsString(property.Value),
                    FieldType = property.Value.ValueKind.ToString()
                });
            }

            return changes;
        }


        private JsonElement ParseToJsonElement(JsonElement input)
        {
            if (input.ValueKind == JsonValueKind.String)
            {
                return JsonSerializer.Deserialize<JsonElement>(input.GetString()!);
            }
            return input;
        }
        private List<AuditFieldChange> ExtractUpdatedChanges(JsonElement before, JsonElement after)
        {
            var changes = new List<AuditFieldChange>();
            var beforeProps = before.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
            var afterProps = after.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

            // Check for modified and new properties
            foreach (var afterProp in afterProps)
            {
                if (beforeProps.TryGetValue(afterProp.Key, out var beforeValue))
                {
                    var beforeStr = GetValueAsString(beforeValue);
                    var afterStr = GetValueAsString(afterProp.Value);

                    if (beforeStr != afterStr)
                    {
                        changes.Add(new AuditFieldChange
                        {
                            FieldName = afterProp.Key,
                            OldValue = beforeStr,
                            NewValue = afterStr,
                            FieldType = afterProp.Value.ValueKind.ToString()
                        });
                    }
                }
                else
                {
                    // New property
                    changes.Add(new AuditFieldChange
                    {
                        FieldName = afterProp.Key,
                        OldValue = null,
                        NewValue = GetValueAsString(afterProp.Value),
                        FieldType = afterProp.Value.ValueKind.ToString()
                    });
                }
            }

            // Check for removed properties
            foreach (var beforeProp in beforeProps)
            {
                if (!afterProps.ContainsKey(beforeProp.Key))
                {
                    changes.Add(new AuditFieldChange
                    {
                        FieldName = beforeProp.Key,
                        OldValue = GetValueAsString(beforeProp.Value),
                        NewValue = null,
                        FieldType = beforeProp.Value.ValueKind.ToString()
                    });
                }
            }

            return changes;
        }

        private List<AuditFieldChange> ExtractDeletedChanges(JsonElement obj)
        {
            var changes = new List<AuditFieldChange>();

            foreach (var property in obj.EnumerateObject())
            {
                changes.Add(new AuditFieldChange
                {
                    FieldName = property.Name,
                    OldValue = GetValueAsString(property.Value),
                    NewValue = null,
                    FieldType = property.Value.ValueKind.ToString()
                });
            }

            return changes;
        }

        private string? GetValueAsString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        private AuditResponse MapToAuditResponse(AuditEntry entry)
        {
            var response = new AuditResponse
            {
                AuditId = entry.Id,
                EntityName = entry.EntityName,
                EntityId = entry.EntityId,
                Action = entry.Action,
                UserId = entry.UserId,
                Timestamp = entry.Timestamp,
                Changes = entry.FieldChanges.Select(fc => new FieldChangeDto
                {
                    FieldName = fc.FieldName,
                    OldValue = fc.OldValue,
                    NewValue = fc.NewValue,
                    FieldType = fc.FieldType
                }).ToList()
            };

            if (!string.IsNullOrEmpty(entry.AdditionalMetadata))
            {
                try
                {
                    response.AdditionalMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entry.AdditionalMetadata);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Error deserializing additional metadata for audit entry {AuditId}", entry.Id);
                }
            }

            return response;
        }
    }

}
