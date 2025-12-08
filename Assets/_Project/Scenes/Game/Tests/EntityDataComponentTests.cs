using NUnit.Framework;
using UnityEngine;
using Lineage.Components;
using Lineage.Database;

public class EntityDataComponentTests
{
    [Test]
    public void GetStat_Uninitialized_ReturnsDefault()
    {
        var go = new GameObject();
        var component = go.AddComponent<EntityDataComponent>();

        var stat = component.GetStat(Stat.ID.Health);
        Assert.AreEqual(0f, stat.currentValue);
    }
}
