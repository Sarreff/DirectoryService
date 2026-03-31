using System.Data;
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

public class GetChildDepartmentsHandler : IQueryHandler<GetChildDepartmentsDto, GetChildDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetChildDepartmentsQuery> _validator;
    private readonly ILogger<GetChildDepartmentsHandler> _logger;

    public GetChildDepartmentsHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<GetChildDepartmentsQuery> validator,
        ILogger<GetChildDepartmentsHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetChildDepartmentsDto, Errors>> Handle(
        GetChildDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Failed: {Errors}", validationResult.Errors);
            return validationResult.ToError();
        }

        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        int offset = (query.Page - 1) * query.Size;
        var parameters = new DynamicParameters();
        parameters.Add("parentId", query.ParentId, DbType.Guid);
        parameters.Add("offset", offset, DbType.Int32);
        parameters.Add("limit", query.Size, DbType.Int32);

        var departmentNodes = (await connection.QueryAsync<DepartmentNodeDto>(
            """
            SELECT d.id,
                    d.parent_id,
                    d.name,
                    d.identifier,
                    d.path,
                    d.depth,
                    d.is_active,
                    d.created_at,
                    d.updated_at,
                    COUNT(*) OVER () AS total_count,
                    EXISTS(SELECT 1 FROM departments c WHERE c.parent_id = d.Id AND c.is_active = true)
                    AS has_more_children
            FROM departments d
            WHERE d.parent_id = @parentId AND d.is_active = true
            ORDER BY d.created_at DESC, d.name ASC
            OFFSET @offset LIMIT @limit
            """,
            param: parameters)).ToList();

        if (departmentNodes.Count == 0)
            return new GetChildDepartmentsDto([], 0);

        int totalCount = departmentNodes.First().TotalCount;

        var departments = departmentNodes.Select(n => new ChildDepartmentDto
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

        return new GetChildDepartmentsDto(departments, totalCount);
    }
}