namespace IncidentAgentDemo.Tests;

public class RiskClassificationTests
{
    [Theory]
    [InlineData(0, "Healthy", "Low")]
    [InlineData(0, "Degraded", "Medium")]
    [InlineData(1, "Healthy", "Medium")]
    [InlineData(1, "Degraded", "Medium")]
    [InlineData(2, "Healthy", "High")]
    [InlineData(2, "Degraded", "High")]
    [InlineData(3, "Down", "High")]
    [InlineData(0, "Down", "High")]
    public void ClassifyRisk_ReturnsCorrectLevel(int highSeverityCount, string healthStatus, string expectedRisk)
    {
        var risk = ClassifyRisk(highSeverityCount, healthStatus);
        Assert.Equal(expectedRisk, risk);
    }

    /// <summary>
    /// Mirrors the risk classification logic described in the agent instructions.
    /// </summary>
    private static string ClassifyRisk(int highSeverityCount, string healthStatus)
    {
        if (highSeverityCount >= 2 || healthStatus == "Down")
            return "High";

        if (highSeverityCount >= 1 || healthStatus == "Degraded")
            return "Medium";

        return "Low";
    }
}
