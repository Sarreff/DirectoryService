using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.GetDepartment;

public class GetRootDepartmentsHandler : IQueryHandler<GetRootDepartmentsDto, GetRootDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetRootDepartmentsQuery> _validator;
    private readonly ILogger<GetRootDepartmentsHandler> _logger;

    public GetRootDepartmentsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<GetRootDepartmentsQuery> validator,
        ILogger<GetRootDepartmentsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetRootDepartmentsDto, Errors>> Handle(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Failed: {Errors}", validationResult.Errors);
            return validationResult.ToError();
        }

        try
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var parameters = new
            {
                offset = (query.Page - 1) * query.Size, root_limit = query.Size, child_limit = query.Prefetch,
            };

            var departmentNodes = (await connection.QueryAsync<DepartmentNodeDto>(
                """
                WITH roots as (SELECT d.id,
                                      d.parent_id,
                                      d.name,
                                      d.identifier,
                                      d.path,
                                      d.depth,
                                      d.is_active,
                                      d.created_at,
                                      d.updated_at,
                                      COUNT(*) OVER () AS total_count
                               FROM departments d
                               WHERE d.parent_id IS NULL AND d.is_active = true
                               ORDER BY d.created_at DESC, d.name ASC
                               OFFSET @offset LIMIT @root_limit)
                -- Root departments
                SELECT r.*,
                       (EXISTS(SELECT 1 FROM departments 
                       WHERE parent_id = r.id AND is_active = true 
                       ORDER BY created_at DESC, name ASC OFFSET @child_limit)) AS has_more_children
                FROM roots r

                UNION ALL

                -- Child departments
                SELECT c.*, r.total_count,
                       (EXISTS(SELECT 1 FROM departments 
                       WHERE parent_id = c.id AND is_active = true)) AS has_more_children
                FROM roots r
                         CROSS JOIN LATERAL (SELECT d.id,
                                                    d.parent_id,
                                                    d.name,
                                                    d.identifier,
                                                    d.path,
                                                    d.depth,
                                                    d.is_active,
                                                    d.created_at,
                                                    d.updated_at
                                             FROM departments d
                                             WHERE d.parent_id = r.id
                                               AND d.is_active = true
                                             ORDER BY d.created_at DESC, d.name ASC
                                             LIMIT @child_limit) c;
                """,
                param: parameters)).ToList();

            if (departmentNodes.Count == 0)
                return new GetRootDepartmentsDto([], 0);

            int totalCount = departmentNodes.First().TotalCount;

            var departments = departmentNodes.Select(n => new DepartmentDto
            {
                Id = n.Id,
                Name = n.Name,
                Identifier = n.Identifier,
                ParentId = n.ParentId,
                Depth = n.Depth,
                Path = n.Path,
                IsActive = n.IsActive,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                HasMoreChildren = n.HasMoreChildren,
            }).ToList();
            var departmentDictionary = departments.ToDictionary(d => d.Id);

            foreach (DepartmentDto department in departments)
            {
                if (department.ParentId != null &&
                    departmentDictionary.TryGetValue(department.ParentId.Value, out var parent))
                {
                    parent.Children.Add(department);
                }
            }

            var rootsDepartments = departmentDictionary.Values
                .Where(d => d.ParentId == null).ToList();
            var resultDto = new GetRootDepartmentsDto(rootsDepartments, totalCount);

            return resultDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get root departments");
            return Error.Failure("query_failure", "Failed to get root departments").ToErrors();
        }
    }
}