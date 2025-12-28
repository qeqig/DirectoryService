using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Contracts.Department.MoveDepartments;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentsTests : DirectoryBaseTests
{
    protected TestDepartmentData BaseAdd = new();

    public MoveDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task MoveDepartments_with_valid_data_should_succeed()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        DepartmentId depId1 = DepartmentId.Create();

        Department dep1 = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("one").Value,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId1,  location.Id)],
            depId1).Value;

        DepartmentId depId2 = DepartmentId.Create();

        Department dep2 = Department.CreateChild(
            DepartmentName.Create("name2").Value,
            Identifier.Create("two").Value,
            dep1,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId2,  location.Id)],
            depId2).Value;

        DepartmentId depId3 = DepartmentId.Create();

        Department dep3 = Department.CreateChild(
            DepartmentName.Create("name3").Value,
            Identifier.Create("three").Value,
            dep2,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId3,  location.Id)],
            depId3).Value;

        DepartmentId depId4 = DepartmentId.Create();

        Department dep4 = Department.CreateChild(
            DepartmentName.Create("name4").Value,
            Identifier.Create("four").Value,
            dep3,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId4, location.Id)],
            depId4).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[dep1, dep2, dep3, dep4], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(depId3.Value, new MoveDepartmentsDto(depId1.Value));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeTrue();

            var movedDep = await dbContext.Departments
                .Where(d => d.Id == depId3)
                .FirstAsync(cancellationToken);

            movedDep.Path.Value.Should().Be("one.three");

            movedDep.ParentId.Value.Should().Be(depId1.Value);

            var child = await dbContext.Departments
                .Where(d => d.Id == depId4)
                .FirstAsync(cancellationToken);

            child.Path.Value.Should().Be("one.three.four");
        });
    }

    [Fact]
    public async Task MoveDepartments_with_null_parent_id_should_succeed()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        DepartmentId depId1 = DepartmentId.Create();

        Department dep1 = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("one").Value,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId1, location.Id)],
            depId1).Value;

        DepartmentId depId2 = DepartmentId.Create();

        Department dep2 = Department.CreateChild(
            DepartmentName.Create("name2").Value,
            Identifier.Create("two").Value,
            dep1,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId2, location.Id)],
            depId2).Value;

        DepartmentId depId3 = DepartmentId.Create();

        Department dep3 = Department.CreateChild(
            DepartmentName.Create("name3").Value,
            Identifier.Create("three").Value,
            dep2,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId3, location.Id)],
            depId3).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[dep1, dep2, dep3], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(depId3.Value, new MoveDepartmentsDto(null));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeTrue();

            var movedDep = await dbContext.Departments
                .Where(d => d.Id == depId3)
                .FirstAsync(cancellationToken);

            movedDep.Path.Value.Should().Be("three");

            movedDep.ParentId.Should().Be(null);
        });
    }

    [Fact]
    public async Task MoveDepartments_with_same_ids_should_fail()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        DepartmentId depId1 = DepartmentId.Create();

        Department dep1 = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("one").Value,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId1, location.Id)],
            depId1).Value;

        DepartmentId depId2 = DepartmentId.Create();

        Department dep2 = Department.CreateChild(
            DepartmentName.Create("name2").Value,
            Identifier.Create("two").Value,
            dep1,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId2, location.Id)],
            depId2).Value;

        DepartmentId depId3 = DepartmentId.Create();

        Department dep3 = Department.CreateChild(
            DepartmentName.Create("name3").Value,
            Identifier.Create("three").Value,
            dep2,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId3, location.Id)],
            depId3).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[dep1, dep2, dep3], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(depId3.Value, new MoveDepartmentsDto(depId3.Value));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    [Fact]
    public async Task MoveDepartments_with_invalid_department_id_should_fail()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        DepartmentId depId1 = DepartmentId.Create();

        Department dep1 = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("one").Value,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId1, location.Id)],
            depId1).Value;

        DepartmentId depId2 = DepartmentId.Create();

        Department dep2 = Department.CreateChild(
            DepartmentName.Create("name2").Value,
            Identifier.Create("two").Value,
            dep1,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId2, location.Id)],
            depId2).Value;

        DepartmentId depId3 = DepartmentId.Create();

        Department dep3 = Department.CreateChild(
            DepartmentName.Create("name3").Value,
            Identifier.Create("three").Value,
            dep2,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId3, location.Id)],
            depId3).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[dep1, dep2, dep3], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(DepartmentId.Create().Value, new MoveDepartmentsDto(depId1.Value));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    [Fact]
    public async Task MoveDepartments_with_invalid_parent_id_should_fail()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        DepartmentId depId1 = DepartmentId.Create();

        Department dep1 = Department.CreateParent(
            DepartmentName.Create("name").Value,
            Identifier.Create("one").Value,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId1, location.Id)],
            depId1).Value;

        DepartmentId depId2 = DepartmentId.Create();

        Department dep2 = Department.CreateChild(
            DepartmentName.Create("name2").Value,
            Identifier.Create("two").Value,
            dep1,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId2, location.Id)],
            depId2).Value;

        DepartmentId depId3 = DepartmentId.Create();

        Department dep3 = Department.CreateChild(
            DepartmentName.Create("name3").Value,
            Identifier.Create("three").Value,
            dep2,
            [DepartmentLocation.Create(DepartmentLocationId.Create(), depId3, location.Id)],
            depId3).Value;

        await ExecuteInDb(db => BaseAdd.AddDepartamentToBase(db,[dep1, dep2, dep3], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new MoveDepartmentCommand(depId3.Value, new MoveDepartmentsDto(DepartmentId.Create().Value));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<MoveDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        MoveDepartmentHandler sut = scope.ServiceProvider.GetRequiredService<MoveDepartmentHandler>();

        return await action(sut);
    }
}