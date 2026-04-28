import sys

def main():
    filepath = sys.argv[1]
    with open(filepath, 'r') as f:
        content = f.read()

    search = """    [Fact]
    public void AttackStat_IncrementsCorrectly_WithAttributeStatLogic()
    {
        // Arrange
        StatTracker tracker = new StatTracker(new global::Core.Services.EventBus());
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Attack, 10f }
        };
        tracker.InitializeStats(baseStats);

        // Act - Add permanent +1
        tracker.AddPermanentBonus(StatType.Attack, 1f);

        // Assert
        Assert.Equal(11f, tracker.GetCurrentValue(StatType.Attack));
        Assert.Equal(11f, tracker.GetEffectiveMaxValue(StatType.Attack));
    }"""

    replace = """    [Fact]
    public void AttackStat_IncrementsCorrectly_WithAttributeStatLogic()
    {
        // Arrange
        StatTracker tracker = new StatTracker(new global::Core.Services.EventBus());
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Attack, 10f }
        };
        tracker.InitializeStats(baseStats);

        // Act - Add permanent +1
        tracker.AddPermanentBonus(StatType.Attack, 1f);

        // Assert
        Assert.Equal(11f, tracker.GetCurrentValue(StatType.Attack));
        Assert.Equal(11f, tracker.GetEffectiveMaxValue(StatType.Attack));
    }

    [Fact]
    public void LuckStat_IncrementsCorrectly_WithAttributeStatLogic()
    {
        // Arrange
        StatTracker tracker = new StatTracker(new global::Core.Services.EventBus());
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Luck, 1f }
        };
        tracker.InitializeStats(baseStats);

        // Act - Add permanent +1
        tracker.AddPermanentBonus(StatType.Luck, 1f);

        // Assert
        Assert.Equal(2f, tracker.GetCurrentValue(StatType.Luck));
        Assert.Equal(2f, tracker.GetEffectiveMaxValue(StatType.Luck));
    }"""

    if search in content:
        content = content.replace(search, replace)
        with open(filepath, 'w') as f:
            f.write(content)
        print("Success")
    else:
        print("Search string not found")

if __name__ == "__main__":
    main()
