using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.UseCases.Positions.CreatePosition;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Positions;

public sealed class CreatePositionTests : IClassFixture<TestApplicationFactory>
{
    private readonly IServiceProvider _services;

    public CreatePositionTests(TestApplicationFactory factory)
    {
        _services = factory.Services;
    }
    
    [Fact]
    private async Task Create_Position_Empty_Name_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        List<Guid> createdDepartmentIds = [];
        
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
            createdLocationIds.Add((Guid)createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add((Guid)createdLocationId);
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
            createdDepartmentIds.Add(departmentId);
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
            createdDepartmentIds.Add(childDepartmentId);
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        CreatePositionCommand createPositionCommand = new(" ", "Test Position Description", createdDepartmentIds);
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsFailure);
        }
    }

    [Fact]
    private async Task Create_Position_Description_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        List<Guid> createdDepartmentIds = [];
        
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
            createdLocationIds.Add((Guid)createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add((Guid)createdLocationId);
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
            createdDepartmentIds.Add(departmentId);
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
            createdDepartmentIds.Add(childDepartmentId);
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        CreatePositionCommand createPositionCommand = new("Test Position", "   ", createdDepartmentIds);
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsFailure);
        }
    }
    
    [Fact]
    private async Task Create_Position_Empty_Department_NotExist_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        List<Guid> createdDepartmentIds = [];
        
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
            createdLocationIds.Add((Guid)createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add((Guid)createdLocationId);
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
            createdDepartmentIds.Add(departmentId);
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
            createdDepartmentIds.Add(childDepartmentId);
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        CreatePositionCommand createPositionCommand = 
            new("Test Position", "Test Position Description", [Guid.NewGuid(), Guid.NewGuid()]);
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsFailure);
        }
    }
    
    [Fact]
    private async Task Create_Position_Empty_Department_Identifiers_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        List<Guid> createdDepartmentIds = [];
        
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
            createdLocationIds.Add((Guid)createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add((Guid)createdLocationId);
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
            createdDepartmentIds.Add(departmentId);
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
            createdDepartmentIds.Add(childDepartmentId);
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        CreatePositionCommand createPositionCommand = new("Test Position", "Test Position Description", []);
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsFailure);
        }
    }
    
    [Fact]
    private async Task Create_Position_Success()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        List<Guid> createdDepartmentIds = [];
        
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
            createdLocationIds.Add((Guid)createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add((Guid)createdLocationId);
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
            createdDepartmentIds.Add(departmentId);
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
            createdDepartmentIds.Add(childDepartmentId);
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        Guid positionId = Guid.Empty;
        CreatePositionCommand createPositionCommand = new("Test Position", "Test Position Description", createdDepartmentIds);
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsSuccess);
            positionId = createdPositionId.Value;
        }
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            
            Result<Department> created = await repository.GetById(departmentId);
            Department department = created.Value;
            Assert.Contains(department.Positions, p => p.PositionId.Value == positionId);
        }
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            
            Result<Department> created = await repository.GetById(childDepartmentId);
            Department department = created.Value;
            Assert.Contains(department.Positions, p => p.PositionId.Value == positionId);
        }
    }
    
    [Fact]
    private async Task Create_Position_Duplicate_Name_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        
        List<Guid> createdLocationIds = [];
        List<Guid> createdDepartmentIds = [];
        
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
            createdLocationIds.Add((Guid)createdLocationId);
        }

        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreateLocationCommand> createLocationHandler =
                scope.GetService<ICommandHandler<Guid, CreateLocationCommand>>();
            Result<Guid> createdLocationId = await createLocationHandler.Handle(createLocationSecond);
            Assert.True(createdLocationId.IsSuccess);
            createdLocationIds.Add((Guid)createdLocationId);
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
            createdDepartmentIds.Add(departmentId);
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
            createdDepartmentIds.Add(childDepartmentId);
        }
        
        Assert.NotEqual(childDepartmentId, Guid.Empty);
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            await using ServiceDbContext dbContext = scope.GetService<ServiceDbContext>();
            IDepartmentsRepository repository = new DepartmentsRepository(dbContext);
            Result<Department> created = await repository.GetById(childDepartmentId);
            Assert.True(created.IsSuccess);
        }
        
        CreatePositionCommand createPositionCommand = new("Test Position", "Test Position Description", createdDepartmentIds);
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsSuccess);
        }
        
        await using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICommandHandler<Guid, CreatePositionCommand> handler =
                scope.GetService<ICommandHandler<Guid, CreatePositionCommand>>();
            Result<Guid> createdPositionId = await handler.Handle(createPositionCommand);
            Assert.True(createdPositionId.IsFailure);
        }
    }
}