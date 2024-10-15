using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class FilterDisposalTests {
    private readonly ITestOutputHelper output;
    private readonly World world;

    public FilterDisposalTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));

        this.world = World.Create();
    }

    [Fact]
    public void DisposeInvalidatesFilter() {
        var ent = this.world.CreateEntity(); 
        ent.AddComponent<Test1>();
        this.world.Commit();

        var filter = this.world.Filter.With<Test1>().Build();

        Assert.False(filter.IsEmpty());
        foreach (var filterEnt in filter) {
            Assert.Equal(ent, filterEnt);
        }

        filter.Dispose();
        foreach (var _ in filter) {
            Assert.Fail("Filter should be invalid after Dispose");
        }
        Assert.Equal(-1, filter.id);
    }

    [Fact]
    public void DisposeCalledTwiceDoesNotThrow() { 
        var filter = this.world.Filter.With<Test1>().Build();
        filter.Dispose();
        filter.Dispose();
    }

    [Fact]
    public void DisposeInvalidatesAllFilterReferences() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        this.world.Commit();

        var filter0 = this.world.Filter.With<Test1>().Build();
        var filter1 = this.world.Filter.With<Test1>().Build();
        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.Equal(filter0, filter1);

        filter0.Dispose();
        foreach (var _ in filter0) {
            Assert.Fail("Filter0 should be invalid after Dispose");
        }
        foreach (var _ in filter1) {
            Assert.Fail("Filter1 should be invalid after Dispose");
        }
        Assert.Equal(-1, filter0.id);
        Assert.Equal(-1, filter1.id);
    }

    [Fact]
    public void DisposeRemovesFilterFromWorld() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        this.world.Commit();

        var filterBuilder = this.world.Filter.With<Test1>();
        var filter = filterBuilder.Build();
        var filterId = filter.id;

        Assert.False(filter.IsEmpty());
        foreach (var filterEnt in filter) {
            Assert.Equal(ent, filterEnt);
        }

        this.world.Commit();

        var filterArchetype = filter.archetypes[0];
        var entArchetype = world.entities[ent.Id].currentArchetype;
        Assert.NotNull(filterArchetype);
        Assert.NotNull(entArchetype);
        Assert.Equal(filterArchetype.hash, entArchetype.hash);

        var typeId = ComponentId<Test1>.info.id;

        {
            var filtersWith = world.componentsFiltersWith.GetFilters(typeId);
            Assert.Single(filtersWith);
            Assert.Contains(filter, filtersWith);
            Assert.Equal(filtersWith[0], filter);

            var lookup = this.world.filtersLookup;
            if (lookup.TryGetValue(filterBuilder.includeHash.GetValue(), out var excludeMap)) {
                Assert.True(excludeMap.TryGetValue(filterBuilder.excludeHash.GetValue(), out var lfilter));
                Assert.Equal(filter, lfilter);
            }
        }

        filter.Dispose();

        {
            var filtersWith = world.componentsFiltersWith.GetFilters(typeId);
            Assert.Empty(filtersWith);
            Assert.Null(filter.archetypes);

            var lookup = this.world.filtersLookup;
            if (lookup.TryGetValue(filterBuilder.includeHash.GetValue(), out var excludeMap)) {
                Assert.False(excludeMap.TryGetValue(filterBuilder.excludeHash.GetValue(), out _));
            }

            Assert.Equal(0, world.filterCount);
            Assert.Equal(1, world.freeFilterIDs.length);
            Assert.Equal(-1, filter.id);

            var freeId = world.freeFilterIDs.Pop();
            Assert.Equal(filterId, freeId);
            Assert.NotEqual(filter.id, freeId);
        }
    }

    [Fact]
    public void DisposeFreesIdAndAssignsItToNewFilter() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        ent.AddComponent<Test2>();
        this.world.Commit();

        var filterBuilder0 = this.world.Filter.With<Test1>();
        var filter0 = filterBuilder0.Build();
        var filterId0 = filter0.id;

        var filterBuilder1 = this.world.Filter.With<Test1>().With<Test2>();
        var filter1 = filterBuilder1.Build();
        var filterId1 = filter1.id;

        var filterBuilder2 = this.world.Filter.With<Test2>();
        var filter2 = filterBuilder2.Build();
        var filterId2 = filter2.id;

        Assert.NotEqual(filterId0, filterId1);
        Assert.NotEqual(filterId0, filterId2);
        Assert.NotEqual(filterId1, filterId2);

        this.world.Commit();

        filter1.Dispose();

        Assert.Equal(1, world.freeFilterIDs.length);

        var filterBuilder3 = this.world.Filter.With<Test1>().Without<Test4>();
        var filter3 = filterBuilder3.Build();
        var filterId3 = filter3.id;

        Assert.Equal(0, world.freeFilterIDs.length);
        Assert.Equal(filterId1, filterId3);

        Assert.Equal(filterId0, filter0.id);
        Assert.Equal(filterId2, filter2.id);

        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter2.IsEmpty());
        foreach (var filterEnt in filter2) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter3.IsEmpty());
        foreach (var filterEnt in filter3) {
            Assert.Equal(ent, filterEnt);
        }
    }

    [Fact]
    public void DisposeRemovesMatchingFilterAndKeepsOtherValid() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        this.world.Commit();

        var filterBuilder0 = this.world.Filter.With<Test1>();
        var incHash0 = filterBuilder0.includeHash;
        var excHash0 = filterBuilder0.excludeHash;
        var filter0 = filterBuilder0.Build();
        var filterId0 = filter0.id;

        var filterBuilder1 = this.world.Filter.With<Test1>().Without<Test2>();
        var incHash1 = filterBuilder1.includeHash;
        var excHash1 = filterBuilder1.excludeHash;
        var filter1 = filterBuilder1.Build();

        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }

        this.world.Commit();

        var typeIdWith = ComponentId<Test1>.info.id;
        var typeIdWithout = ComponentId<Test2>.info.id;

        {
            var filtersWith = world.componentsFiltersWith.GetFilters(typeIdWith);
            Assert.Contains(filter0, filtersWith);
            Assert.Contains(filter1, filtersWith);

            var filtersWithout = world.componentsFiltersWithout.GetFilters(typeIdWithout);
            Assert.Single(filtersWithout);
            Assert.Contains(filter1, filtersWithout);
            Assert.Equal(filtersWithout[0], filter1);

            var lookup = this.world.filtersLookup;
            if (lookup.TryGetValue(incHash0.GetValue(), out var excludeMap0)) {
                Assert.True(excludeMap0.TryGetValue(excHash0.GetValue(), out var lfilter0));
                Assert.Equal(filter0, lfilter0);
            }

            if (lookup.TryGetValue(incHash1.GetValue(), out var excludeMap1)) {
                Assert.True(excludeMap1.TryGetValue(excHash1.GetValue(), out var lfilter1));
                Assert.Equal(filter1, lfilter1);
            }
        }

        filter0.Dispose();

        {
            var filtersWith = world.componentsFiltersWith.GetFilters(typeIdWith);
            Assert.Single(filtersWith);
            Assert.DoesNotContain(filter0, filtersWith);
            Assert.Contains(filter1, filtersWith);

            var filtersWithout = world.componentsFiltersWithout.GetFilters(typeIdWithout);
            Assert.Single(filtersWithout);
            Assert.Contains(filter1, filtersWithout);
            Assert.Equal(filtersWithout[0], filter1);

            var lookup = this.world.filtersLookup;
            if (lookup.TryGetValue(incHash0.GetValue(), out var excludeMap0)) {
                Assert.False(excludeMap0.TryGetValue(excHash0.GetValue(), out _));
            }

            if (lookup.TryGetValue(incHash1.GetValue(), out var excludeMap1)) {
                Assert.True(excludeMap1.TryGetValue(excHash1.GetValue(), out var lfilter1));
                Assert.Equal(filter1, lfilter1);
            }

            Assert.Equal(1, world.filterCount);
            Assert.Equal(1, world.freeFilterIDs.length);
            Assert.Equal(-1, filter0.id);

            var freeId = world.freeFilterIDs.Pop();
            Assert.Equal(filterId0, freeId);
            Assert.NotEqual(filter0.id, freeId);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }
    }

    [Fact]
    public void DisposeRemovesMatchingFilterAndKeepsOtherValid2()
    {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        ent.AddComponent<Test2>();
        this.world.Commit();

        var filterBuilder0 = this.world.Filter.With<Test1>();
        var filter0 = filterBuilder0.Build();
        var filterId0 = filter0.id;

        var filterBuilder1 = this.world.Filter.With<Test1>().With<Test2>();
        var filter1 = filterBuilder1.Build();

        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }

        this.world.Commit();

        var typeIdWith1 = ComponentId<Test1>.info.id;
        var typeIdWith2 = ComponentId<Test2>.info.id;

        {
            var filtersWith1 = world.componentsFiltersWith.GetFilters(typeIdWith1);
            Assert.Contains(filter0, filtersWith1);
            Assert.Contains(filter1, filtersWith1);

            var filtersWith2 = world.componentsFiltersWith.GetFilters(typeIdWith2);
            Assert.Single(filtersWith2);
            Assert.Contains(filter1, filtersWith2);
            Assert.Equal(filtersWith2[0], filter1);

            var lookup = this.world.filtersLookup;
            if (lookup.TryGetValue(filterBuilder0.includeHash.GetValue(), out var excludeMap0)) {
                Assert.True(excludeMap0.TryGetValue(filterBuilder0.excludeHash.GetValue(), out var lfilter0));
                Assert.Equal(filter0, lfilter0);
            }

            if (lookup.TryGetValue(filterBuilder1.includeHash.GetValue(), out var excludeMap1)) {
                Assert.True(excludeMap1.TryGetValue(filterBuilder1.excludeHash.GetValue(), out var lfilter1));
                Assert.Equal(filter1, lfilter1);
            }
        }

        filter0.Dispose();

        {
            var filtersWith1 = world.componentsFiltersWith.GetFilters(typeIdWith1);
            Assert.Single(filtersWith1);
            Assert.DoesNotContain(filter0, filtersWith1);
            Assert.Contains(filter1, filtersWith1);

            var filtersWith2 = world.componentsFiltersWith.GetFilters(typeIdWith2);
            Assert.Single(filtersWith2);
            Assert.Contains(filter1, filtersWith2);
            Assert.Equal(filtersWith2[0], filter1);

            var lookup = this.world.filtersLookup;
            if (lookup.TryGetValue(filterBuilder0.includeHash.GetValue(), out var excludeMap0)) {
                Assert.False(excludeMap0.TryGetValue(filterBuilder0.excludeHash.GetValue(), out _));
            }

            if (lookup.TryGetValue(filterBuilder1.includeHash.GetValue(), out var excludeMap1)) {
                Assert.True(excludeMap1.TryGetValue(filterBuilder1.excludeHash.GetValue(), out var lfilter1));
                Assert.Equal(filter1, lfilter1);
            }

            Assert.Equal(1, world.filterCount);
            Assert.Equal(1, world.freeFilterIDs.length);
            Assert.Equal(-1, filter0.id);

            var freeId = world.freeFilterIDs.Pop();
            Assert.Equal(filterId0, freeId);
            Assert.NotEqual(filter0.id, freeId);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) { 
            Assert.Equal(ent, filterEnt);
        }
    }

    [Fact]
    public void StructuralChangesFilter() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        ent.AddComponent<Test2>();

        this.world.Commit();

        var filterBuilder = this.world.Filter.With<Test1>().With<Test3>();
        var filter = filterBuilder.Build();

        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }

        this.world.Commit();

        ent.AddComponent<Test3>();

        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }

        this.world.Commit();

        Assert.False(filter.IsEmpty());
        foreach (var filterEnt in filter) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.NotNull(filter.archetypes);
        Assert.Equal(1, this.world.filterCount);
        Assert.Equal(0, this.world.freeFilterIDs.length);
    }

    [Fact]
    public void StructuralChangesDoNotThrow() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        ent.AddComponent<Test2>();

        this.world.Commit();

        var filterBuilder = this.world.Filter.With<Test1>().With<Test3>();
        var filter = filterBuilder.Build();

        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }

        this.world.Commit();

        ent.AddComponent<Test3>();

        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }

        filter.Dispose();
        this.world.Commit();

        Assert.Null(filter.archetypes);
        Assert.Equal(0, this.world.filterCount);
        Assert.Equal(1, this.world.freeFilterIDs.length);
        Assert.Equal(-1, filter.id);
    }

    [Fact]
    public void StructuralChangesWithMultipleMatchesFilters() {
        var ent = this.world.CreateEntity();
        ent.AddComponent<Test1>();
        ent.AddComponent<Test2>();
        ent.AddComponent<Test3>();
        this.world.Commit();

        var filterBuilder0 = this.world.Filter.With<Test1>().With<Test2>();
        var filter0 = filterBuilder0.Build();

        var filterBuilder1 = this.world.Filter.With<Test1>().With<Test3>();
        var filter1 = filterBuilder1.Build();

        var filterBuilder2 = this.world.Filter.With<Test2>().With<Test3>();
        var filter2 = filterBuilder2.Build();

        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter2.IsEmpty());
        foreach (var filterEnt in filter2) {
            Assert.Equal(ent, filterEnt);
        }

        ent.AddComponent<Test4>();

        this.world.Commit();

        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }

        Assert.False(filter2.IsEmpty());
        foreach (var filterEnt in filter2) {
            Assert.Equal(ent, filterEnt);
        }

        ent.RemoveComponent<Test2>();
        this.world.Commit();

        foreach (var _ in filter0) {
            Assert.Fail("Filter0 should be empty");
        }

        Assert.False(filter1.IsEmpty());
        foreach (var filterEnt in filter1) {
            Assert.Equal(ent, filterEnt);
        }

        foreach (var _ in filter2) {
            Assert.Fail("Filter2 should be empty");
        }

        ent.AddComponent<Test2>();
        filter1.Dispose();

        foreach (var _ in filter0) {
            Assert.Fail("Filter0 should be empty");
        }
        foreach (var _ in filter1) {
            Assert.Fail("Filter1 should be invalid after Dispose");
        }
        foreach (var _ in filter2) {
            Assert.Fail("Filter2 should be empty");
        }

        this.world.Commit();

        Assert.False(filter0.IsEmpty());
        foreach (var filterEnt in filter0) {
            Assert.Equal(ent, filterEnt);
        }
        foreach (var _ in filter1) {
            Assert.Fail("Filter1 should be invalid after Dispose");
        }
        foreach (var filterEnt in filter2) {
            Assert.Equal(ent, filterEnt);
        }

        var typeIdWith0 = ComponentId<Test1>.info.id;
        var typeIdWith1 = ComponentId<Test2>.info.id;
        var typeIdWith2 = ComponentId<Test3>.info.id;

        var filtersWith0 = world.componentsFiltersWith.GetFilters(typeIdWith0);
        Assert.Single(filtersWith0);
        Assert.Contains(filter0, filtersWith0);

        var filtersWith1 = world.componentsFiltersWith.GetFilters(typeIdWith1);
        Assert.Equal(2, filtersWith1.Length);
        Assert.Contains(filter0, filtersWith1);
        Assert.Contains(filter2, filtersWith1);

        var filtersWith2 = world.componentsFiltersWith.GetFilters(typeIdWith2);
        Assert.Single(filtersWith2);
        Assert.Contains(filter2, filtersWith1);

        Assert.DoesNotContain(filter1, filtersWith0);
        Assert.DoesNotContain(filter1, filtersWith2);

        var lookup = this.world.filtersLookup;
        if (lookup.TryGetValue(filterBuilder0.includeHash.GetValue(), out var excludeMap0)) {
            Assert.True(excludeMap0.TryGetValue(filterBuilder0.excludeHash.GetValue(), out var lfilter0));
            Assert.Equal(filter0, lfilter0);
        }

        if (lookup.TryGetValue(filterBuilder1.includeHash.GetValue(), out var excludeMap1)) {
            Assert.False(excludeMap1.TryGetValue(filterBuilder1.excludeHash.GetValue(), out _)); 
        }

        if (lookup.TryGetValue(filterBuilder2.includeHash.GetValue(), out var excludeMap2)) {
            Assert.True(excludeMap2.TryGetValue(filterBuilder2.excludeHash.GetValue(), out var lfilter2));
            Assert.Equal(filter2, lfilter2);
        }

        Assert.Equal(2, world.filterCount);
        Assert.Equal(1, world.freeFilterIDs.length);
    }
}