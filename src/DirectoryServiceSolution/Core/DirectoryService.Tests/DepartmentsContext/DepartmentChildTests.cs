using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.Tests.DepartmentsContext;

public class DepartmentChildTests
{
    [Fact]
    private void Attach_Child_Success()
    {
        const string expectedPath = "first.second";
        const int expectedDepthParentLevel = 0;
        const int expectedDepthChildLevel = 1;
        const int exptectedParentChildCount = 1;

        DepartmentName firstName = DepartmentName.Create("First name");
        DepartmentIdentifier firstIdentifier = DepartmentIdentifier.Create("first");
        Department first = Department.CreateNew(firstName, firstIdentifier);

        DepartmentIdentifier secondIdentifier = DepartmentIdentifier.Create("second");
        DepartmentName secondName = DepartmentName.Create("Second name");
        Department second = Department.CreateNew(secondName, secondIdentifier);
        
        Result attaching = first.AttachOtherDepartment(second);
        Assert.True(attaching.IsSuccess);
        Assert.True(second.Includes(first));
        Assert.Equal(exptectedParentChildCount, first.ChildrensCount.Value);
        Assert.Equal(expectedDepthParentLevel, first.Depth.Value);
        Assert.Equal(expectedDepthChildLevel, second.Depth.Value);
        Assert.Equal(expectedPath, second.Path.Value);
        Assert.Equal(1, first.Attachments.Count());
    }

    [Fact]
    private void Attach_Child_Again_Failure()
    {
        const string expectedPath = "first.second";
        const int expectedDepthParentLevel = 0;
        const int expectedDepthChildLevel = 1;
        const int expectedParentChildsCount = 1;

        DepartmentName firstName = DepartmentName.Create("First name");
        DepartmentIdentifier firstIdentifier = DepartmentIdentifier.Create("first");
        Department first = Department.CreateNew(firstName, firstIdentifier);

        DepartmentIdentifier secondIdentifier = DepartmentIdentifier.Create("second");
        DepartmentName secondName = DepartmentName.Create("Second name");
        Department second = Department.CreateNew(secondName, secondIdentifier);

        Result attaching = first.AttachOtherDepartment(second);
        Assert.True(attaching.IsSuccess);
        Assert.True(second.Includes(first));
        Assert.Equal(expectedParentChildsCount, first.ChildrensCount.Value);
        Assert.Equal(expectedDepthParentLevel, first.Depth.Value);
        Assert.Equal(expectedDepthChildLevel, second.Depth.Value);
        Assert.Equal(expectedPath, second.Path.Value);
        Assert.Equal(1, first.Attachments.Count());

        Result attachingAgain = first.AttachOtherDepartment(second);
        Assert.True(attachingAgain.IsFailure);
        Assert.True(second.Includes(first));
        Assert.Equal(expectedParentChildsCount, first.ChildrensCount.Value);
        Assert.Equal(expectedDepthParentLevel, first.Depth.Value);
        Assert.Equal(expectedDepthChildLevel, second.Depth.Value);
        Assert.Equal(expectedPath, second.Path.Value);
    }
}
