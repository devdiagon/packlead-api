using Packlead.Domain.Entities;
using Packlead.Domain.Enums;

namespace Packlead.Domain.Tests.Entities;
public class DispatcherTests
{
    private static Dispatcher NewDispatcher(string firebaseUid = "uid-123") =>
        new(firebaseUid, "Hugo Moncayo", "hugo@packlead.com", "Moto", "ABC-123");

    // D.DIS.01
    [Fact]
    public void Constructor_WithEmptyFirebaseUid_Throws()
    {
        Assert.Throws<ArgumentException>(() => NewDispatcher(firebaseUid: ""));
    }

    // D.DIS.02
    [Fact]
    public void SetAvailable_FromInactive_TransitionsToAvailable()
    {
        var dispatcher = NewDispatcher();
        dispatcher.SetState(DispatcherState.Inactive);

        dispatcher.SetState(DispatcherState.Available);

        Assert.Equal(DispatcherState.Available, dispatcher.State);
    }

    // D.DIS.03
    [Fact]
    public void SetInactive_FromAvailable_TransitionsToInactive()
    {
        var dispatcher = NewDispatcher();

        dispatcher.SetState(DispatcherState.Inactive);

        Assert.Equal(DispatcherState.Inactive, dispatcher.State);
    }
}