using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

using Scellecs.Morpeh.Collections;

[Collection("Sequential")]
public class HashHelpersTests {
    private readonly ITestOutputHelper output;
    
    public HashHelpersTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
    }

    [Fact]
    public void CheckCapacitySizesBinaryRepresentation() {
        for (int i = 0, length = HashHelpers.capacitySizes.Length; i < length; ++i) {
            var value = HashHelpers.capacitySizes[i];
            Assert.True((value & (value + 1)) == 0, $"Value represented as 0b{Convert.ToString(value, 2)} but should contain only 1s");
        }
    }

    [Fact]
    public void CheckSmallCapacitySizesBinaryRepresentation() {
        for (int i = 0, length = HashHelpers.smallCapacitySizes.Length; i < length; ++i) {
            var value = HashHelpers.smallCapacitySizes[i];
            Assert.True((value & (value + 1)) == 0, $"Value represented as 0b{Convert.ToString(value, 2)} but should contain only 1s");
        }
    }
}