using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Department.CreateDepartment;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{
    protected TestDepartmentData BaseAdd = new();

    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartament_with_valid_data_should_succeed()
    {
        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        var cancellationToken = CancellationToken.None;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentDto(
                    "dep",
                    "dep",
                    null,
                    [location.Id.Value]));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBe(Guid.Empty);

            var departament = await dbContext.Departments
                .FirstAsync(d => d.Id == DepartmentId.Current(result.Value), cancellationToken);

            departament.Should().NotBeNull();
            departament.Id.Value.Should().Be(result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartmentChild_with_valid_data_should_succeed()
    {
        var cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        var parentResult = await ExecuteHandler((sut) =>
        {
            var parentCommand = new CreateDepartmentCommand(
                new CreateDepartmentDto("parent", "parentIdentifier", null, [location.Id.Value]));
            return sut.Handle(parentCommand, cancellationToken);
        });

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentDto("name", "identifier", parentResult.Value, [location.Id.Value]));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBe(Guid.Empty);

            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == DepartmentId.Current(result.Value), cancellationToken);

            department.Should().NotBeNull();
            department.ParentId.Value.Should().Be(parentResult.Value);
        });
    }

    [Fact]
    public async Task CreateDepartmentChild_with_invalid_parentid_should_succeed()
    {
        var cancellationToken = CancellationToken.None;

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        var parentResult = await ExecuteHandler((sut) =>
        {
            var parentCommand = new CreateDepartmentCommand(
                new CreateDepartmentDto("parent", "parentIdentifier", null, [location.Id.Value]));
            return sut.Handle(parentCommand, cancellationToken);
        });

        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentDto("name", "identifier", DepartmentId.Create().Value, [location.Id.Value]));

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();

        });
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_dto_should_fail()
    {
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(async (sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentDto(string.Empty, string.Empty, null, []));

            return await sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    [Fact]
    public async Task CreateDepartment_with_not_exist_parent_should_fail()
    {
        var cancellationToken = CancellationToken.None;

        var parent = DepartmentId.Create();

        Location location = Location.Create(
            LocationName.Create("location").Value,
            LocationTimezone.Create("Europe/Moscow").Value,
            LocationAddress.Create("country", "city", "street", "house").Value).Value;

        await ExecuteInDb(db => BaseAdd.AddLocationToBase(db,[location], cancellationToken));

        var result = await ExecuteHandler(async (sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentDto("name", "identifier", parent.Value, [location.Id.Value]));

            return await sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async dbContext =>
        {
            result.IsSuccess.Should().BeFalse();
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        CreateDepartmentHandler
            sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}
