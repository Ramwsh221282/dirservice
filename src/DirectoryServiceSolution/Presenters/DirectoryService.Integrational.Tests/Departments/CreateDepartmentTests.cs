using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class CreateDepartmentTests : IClassFixture<TestApplicationFactory>
{
    private readonly IServiceProvider _services;

    public CreateDepartmentTests(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }

    [Fact]
    private async Task Create_Department_Root_Success()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        const string expectedDepartmentPath = "test-identifier";
        const int expectedDepartmentDepthLevel = 0;
        
        List<Guid> createdLocationIds = [];
        CreateLocationCommand createLocationFirst =
            new CreateLocationCommand("Test Location First", ["Test", "Location", "First"], "Test/Location");
        CreateLocationCommand createLocationSecond = 
            new CreateLocationCommand("Test Location Second", ["Test", "Location", "Second"], "Test/Location");

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationFirst);
            
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add(createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add(createdLocationId);
        }
        
        Assert.Equal(2, expectedLocationsCount);

        CreateDepartmentCommand createDepartment =
            new CreateDepartmentCommand(expectedDepartmentName, expectedDepartmentIdentifier, createdLocationIds);

        Guid departmentId = Guid.Empty;
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler =
                scope.GetService<ICommandHandler<Guid, CreateDepartmentCommand>>();
            
            Result<Guid> createdDepartmentId = await createDepartmentHandler.Handle(createDepartment);
            Assert.True(createdDepartmentId.IsSuccess);
            departmentId = createdDepartmentId.Value;
        }
        
        Assert.NotEqual(departmentId, Guid.Empty);

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            
            Result<Department> created = await repository.GetById(departmentId);
            Assert.True(created.IsSuccess);
            
            Department department = created.Value;
            Assert.Equal(expectedDepartmentName, department.Name.Value);
            Assert.Equal(expectedDepartmentIdentifier, department.Identifier.Value);
            Assert.Equal(expectedDepartmentPath, department.Path.Value);
            Assert.Equal(expectedDepartmentDepthLevel, department.Depth.Value);
            Assert.Equal(0, department.Attachments.Count());;
            Assert.True(department.Locations.All(l => createdLocationIds.Any(cr => cr == l.LocationId.Value)));
        }
    }

    [Fact]
    private async Task Create_Child_Department_Success()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        CreateLocationCommand createLocationFirst =
            new CreateLocationCommand("Test Location First", ["Test", "Location", "First"], "Test/Location");
        CreateLocationCommand createLocationSecond = 
            new CreateLocationCommand("Test Location Second", ["Test", "Location", "Second"], "Test/Location");

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationFirst);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add(createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add(createdLocationId);
        }
        
        Assert.Equal(2, expectedLocationsCount);

        CreateDepartmentCommand createDepartment =
            new CreateDepartmentCommand(expectedDepartmentName, expectedDepartmentIdentifier, createdLocationIds);

        Guid departmentId = Guid.Empty;
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler =
                scope.GetService<ICommandHandler<Guid, CreateDepartmentCommand>>();
            Result<Guid> createdDepartmentId = await createDepartmentHandler.Handle(createDepartment);
            Assert.True(createdDepartmentId.IsSuccess);
            departmentId = createdDepartmentId.Value;
        }
        
        Assert.NotEqual(departmentId, Guid.Empty);

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(departmentId);
            Assert.True(created.IsSuccess);
        }

        CreateDepartmentCommand childDepartment =
            new CreateDepartmentCommand("Child Dep", "child-dep", createdLocationIds, departmentId);
        
        Guid childDepartmentId = Guid.Empty;
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler =
                scope.GetService<ICommandHandler<Guid, CreateDepartmentCommand>>();
            Result<Guid> createdDepartmentId = await createDepartmentHandler.Handle(childDepartment);
            Assert.True(createdDepartmentId.IsSuccess);
            childDepartmentId = createdDepartmentId.Value;
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            
            Result<Department> created = await repository.GetById(departmentId);
            Assert.True(created.IsSuccess);
            
            Department department = created.Value;
            Assert.Equal(1, department.Attachments.Count());
            Assert.Equal(1, department.ChildrensCount.Value);
        }
    }
    
    [Fact]
    private async Task Create_Child_Department_Twice_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        CreateLocationCommand createLocationFirst =
            new CreateLocationCommand("Test Location First", ["Test", "Location", "First"], "Test/Location");
        CreateLocationCommand createLocationSecond = 
            new CreateLocationCommand("Test Location Second", ["Test", "Location", "Second"], "Test/Location");

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationFirst);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add(createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add(createdLocationId);
        }
        
        Assert.Equal(2, expectedLocationsCount);

        CreateDepartmentCommand createDepartment =
            new CreateDepartmentCommand(expectedDepartmentName, expectedDepartmentIdentifier, createdLocationIds);

        Guid departmentId = Guid.Empty;
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler =
                scope.GetService<ICommandHandler<Guid, CreateDepartmentCommand>>();
            Result<Guid> createdDepartmentId = await createDepartmentHandler.Handle(createDepartment);
            Assert.True(createdDepartmentId.IsSuccess);
            departmentId = createdDepartmentId.Value;
        }
        
        Assert.NotEqual(departmentId, Guid.Empty);

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(departmentId);
            Assert.True(created.IsSuccess);
        }

        CreateDepartmentCommand childDepartment =
            new CreateDepartmentCommand("Child Dep", "child-dep", createdLocationIds, departmentId);
        
        Guid childDepartmentId = Guid.Empty;
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler =
                scope.GetService<ICommandHandler<Guid, CreateDepartmentCommand>>();
            Result<Guid> createdDepartmentId = await createDepartmentHandler.Handle(childDepartment);
            Assert.True(createdDepartmentId.IsSuccess);
            childDepartmentId = createdDepartmentId.Value;
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateDepartmentCommand> createDepartmentHandler =
                scope.GetService<ICommandHandler<Guid, CreateDepartmentCommand>>();
            Result<Guid> createdDepartmentId = await createDepartmentHandler.Handle(childDepartment);
            Assert.True(createdDepartmentId.IsFailure);
        }
    }
}