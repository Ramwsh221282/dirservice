using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;

namespace DirectoryService.Tests.DepartmentsContext;

public class DepartmentChildTests
{
    [Fact]
    private void Attach_Child_Success()
    {
        const string expectedPath = "first.second";
        const int expectedDepthParentLevel = 1;
        const int expectedDepthChildLevel = 2;

        DepartmentId firstId = new DepartmentId();
        DepartmentIdentifier firstIdentifier = DepartmentIdentifier.CreateNode("first");
        EntityLifeCycle firstlifeCycle = new EntityLifeCycle();
        DepartmentName firstName = DepartmentName.Create("First name");
        DepartmentPath firstPath = new DepartmentPath(firstIdentifier);
        DepartmentDepth firstDepth = DepartmentDepth.Create(firstPath, firstIdentifier);
        Department first = Department.Create(firstId, firstIdentifier, firstlifeCycle, firstName, firstPath, firstDepth);

        DepartmentId secondId = new DepartmentId();
        DepartmentIdentifier secondIdentifier = DepartmentIdentifier.CreateNode("second");
        EntityLifeCycle secondLifeCycle = new EntityLifeCycle();
        DepartmentName secondName = DepartmentName.Create("Second name");
        DepartmentPath secondPath = new DepartmentPath(secondIdentifier);
        DepartmentDepth secondDepth = DepartmentDepth.Create(secondPath, secondIdentifier);
        Department second = Department.Create(secondId, secondIdentifier, secondLifeCycle, secondName, secondPath, secondDepth);

        Department attached = first.AttachOtherDepartment(second);
        Assert.NotNull(attached.Parent);
        Assert.Equal(expectedPath, attached.Identifier.Value);
        Assert.Equal(expectedPath, attached.Path.Value);
        Assert.Equal(expectedDepthParentLevel, first.Depth.Value);
        Assert.Equal(expectedDepthChildLevel, attached.Depth.Value);
    }    
}
