using System;
using Drifted.StateManagement;

namespace Drifted;

// Implementation of IScreenFactory for creating screens
public class ScreenFactory : IScreenFactory {
    public GameScreen CreateScreen(Type screenType) {
        // All of our screens have empty constructors so we can just use Activator
        return Activator.CreateInstance(screenType) as GameScreen;
    }
}