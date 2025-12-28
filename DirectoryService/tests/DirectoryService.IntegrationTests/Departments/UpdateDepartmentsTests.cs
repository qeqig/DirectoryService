using DirectoryService.Application.Departments.UpdateDepartmentsLocation;
using DirectoryService.Contracts.Department.UpdateDepartments;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentsTests : DirectoryBaseTests
{
    protected TestDepartmentData BaseAdd = new();

    public UpdateDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartments_with_valid_data_should_succeed()
    {
        var cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        Location location1 = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        Location location2 = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location, location1, location2], cancellationToken));

        DepartmentId departmentId = DepartmentId.Create();

        List<DepartmentLocation> depLoc =
            [DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location.Id)];

        List<DepartmentLocation> newDep = [
            DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location1.Id),
            DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location2.Id)];

        Department department = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("identifier").Value,
            depLoc,
            departmentId).Value;
        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[department], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new UpdateDepartmentLocationCommand(
                department.Id.Value,
                new UpdateDepartmentLocationDto(newDep.Select(d => d.LocationId.Value).ToArray()));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeTrue();
            departmentId.Value.Should().Be(departmentId.Value);

            List<DepartmentLocation> depLocs = await dbContext.DepartmentLocations
                .Where(d => d.DepartmentId == department.Id).ToListAsync(cancellationToken);

            depLocs.Should().NotBeEmpty();
            depLocs.Count.Should().Be(newDep.Count);
        });
    }

    [Fact]
    public async Task UpdateDepartments_with_invalid_id_should_fail()
    {
        var cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        Location location1 = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location, location1], cancellationToken));

        DepartmentId departmentId = DepartmentId.Create();

        List<DepartmentLocation> depLoc = [DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location.Id)];

        List<DepartmentLocation> newDep = [DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location1.Id)];

        Department department = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("identifier").Value,
            depLoc,
            departmentId).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[department], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new UpdateDepartmentLocationCommand(
                department.Id.Value,
                new UpdateDepartmentLocationDto([DepartmentLocationId.Create().Value, DepartmentId.Create().Value, LocationId.Create().Value]));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    [Fact]
    public async Task UpdateDepartments_with_invalid_department_id_should_fail()
    {
        var cancellationToken = CancellationToken.None;

        Location location = Location.Create(
                    LocationName.Create("location").Value,
                    LocationTimezone.Create("Europe/Moscow").Value,
                    LocationAddress.Create("country", "city", "street", "house").Value).Value;

        Location location1 = Location.Create(
                    LocationName.Create("location").Value,
                    LocationTimezone.Create("Europe/Moscow").Value,
                    LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location, location1], cancellationToken));

        DepartmentId departmentId = DepartmentId.Create();

        List<DepartmentLocation> depLoc = [DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location.Id)];

        List<DepartmentLocation> newDep = [DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, location1.Id)];

        Department department = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("identifier").Value,
            depLoc,
            departmentId).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[department], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new UpdateDepartmentLocationCommand(
                DepartmentId.Create().Value,
                new UpdateDepartmentLocationDto(newDep.Select(d => d.LocationId.Value).ToArray()));

            return sut.Handle(command, cancellationToken);
            });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentLocationHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        UpdateDepartmentLocationHandler sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationHandler>();

        return await action(sut);
    }
}