using Drifted.StateManagement;

namespace Drifted;

public class TitleRecord : TextRecord {
    public override Title Create(ScreenManager screenManager) {
        return new Title(screenManager, this);
    }
}