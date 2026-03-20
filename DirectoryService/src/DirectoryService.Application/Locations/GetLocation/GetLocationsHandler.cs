using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public class GetLocationsHandler : IQueryHandler<GetLocationsDto, GetLocationsQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetLocationsQuery> _validator;
    private readonly ILogger<GetLocationsHandler> _logger;

    public GetLocationsHandler(
        IReadDbContext readDbContext,
        IValidator<GetLocationsQuery> validator,
        ILogger<GetLocationsHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetLocationsDto, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation in GetLocationsHandler failed: {Error}", validationResult.ToError());
            return validationResult.ToError();
        }

        var request = query.GetLocationsRequest;

        IQueryable<Location> locationsQuery = _readDbContext.LocationRead;
        IQueryable<DepartmentLocation> departmentLocationQuery = _readDbContext.DepartmentLocationRead;

        if (request.DepartmentIds is not null && request.DepartmentIds.Count != 0)
        {
            var departmentIds = request.DepartmentIds
                .Select(id => new DepartmentId(id))
                .ToList();

            locationsQuery = locationsQuery.Where(l =>
                departmentLocationQuery
                    .Any(dl =>
                        dl.LocationId == l.Id &&
                        departmentIds.Contains(dl.DepartmentId)));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            string search = request.SearchName.ToLower();
            locationsQuery = locationsQuery.Where(l =>
                EF.Functions.Like(l.Name.Value.ToLower(), $"%{search}%"));
        }

        if (request.IsActive is not null)
        {
            locationsQuery = locationsQuery.Where(l => l.IsActive == request.IsActive);
        }

        switch (request.SortBy)
        {
            case "name":
                locationsQuery = request.SortByOrder == "desc"
                    ? locationsQuery.OrderByDescending(l => l.Name.Value)
                    : locationsQuery.OrderBy(l => l.Name.Value);
                break;
            case "date":
                locationsQuery = request.SortByOrder == "desc"
                    ? locationsQuery.OrderByDescending(l => l.CreatedAt).ThenBy(l => l.Id)
                    : locationsQuery.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id);
                break;
            default:
                locationsQuery = request.SortByOrder == "desc"
                    ? locationsQuery.OrderByDescending(l => l.Name.Value)
                    : locationsQuery.OrderBy(l => l.Name.Value);
                break;
        }

        long totalCount = await locationsQuery.LongCountAsync(cancellationToken);

        int page = request.Page!.Value;
        int pageSize = request.PageSize!.Value;

        var locationsResult = await locationsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationDto
            {
                Id = l.Id.Value,
                Name = l.Name.Value,
                Address = new AddressDto(
                    l.Address.Country,
                    l.Address.City,
                    l.Address.Street,
                    l.Address.Building,
                    l.Address.OfficeNumber),
                Timezone = l.Timezone.Value,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return new GetLocationsDto(locationsResult, totalCount);
    }
}