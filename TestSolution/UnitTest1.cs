using Editor.SceneUtils;

namespace TestSolution;

public class Tests {
    [SetUp]
    public void Setup() {
    }

    [Test]
    public void SceneInRoot() {
        Assert.That(ScenePaths.GetSceneDisplayPath("assets/scenes/main.unity"), Is.EqualTo("main"));
    }
    
    [Test]
    public void SceneInSubFolder() {
        Assert.That(ScenePaths.GetSceneDisplayPath("assets/scenes/experimental/exp1.unity"), Is.EqualTo("experimental/exp1"));
    }
}