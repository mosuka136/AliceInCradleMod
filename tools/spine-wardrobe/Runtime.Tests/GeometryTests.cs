using System.Text.Json;
using BetterExperience.Patches.ReplaceTexture;
using Spine;
using Xunit;

namespace Wardrobe.Tests;

public class GeometryTests
{
    private const string AtlasText = "page.png\nsize:16,16\npart\nbounds:0,0,16,16\n";
    private const string Source = """
        {"skeleton":{"spine":"4.1.24","hash":"test"},"bones":[{"name":"root"},{"name":"rotated","parent":"root","rotation":90,"scaleX":2}],
        "slots":[{"name":"cloth","bone":"root","attachment":"part"}],
        "skins":[{"name":"default","attachments":{}},{"name":"clothed","attachments":{"cloth":{"part":{
        "type":"mesh","uvs":[0,0,1,0,0,1],"triangles":[0,1,2],
        "vertices":[2,0,0,0,0.5,1,0,0,0.5,2,0,10,0,0.5,1,0,-10,0.5,2,0,0,10,0.5,1,5,0,0.5],"hull":3}}}}],
        "animations":{"stand":{"bones":{"root":{"rotate":[{"value":0},{"time":1,"value":10}]}}}}}
        """;

    private static Atlas Atlas() => PortraitCatalog.ReadAtlas(AtlasText);

    [Fact]
    public void WeightedDisplacementUsesEachInfluenceInverseTransform()
    {
        var request = JsonDocument.Parse("""
            {"edits":[{"attachment":"clothed/cloth/part","slot":"cloth","deltas":[{"vertex":0,"dx":4,"dy":6}]}]}
            """).RootElement;
        string updated = JsonSerializer.Serialize(Program.Geometry(Source, Atlas(), request));
        var a = Pose(Source);
        var b = Pose(updated);
        Assert.Equal(4, b[0] - a[0], 4);
        Assert.Equal(6, b[1] - a[1], 4);
        Assert.Equal(a.Skip(2), b.Skip(2));
    }

    [Fact]
    public void SamplingIncludesAttachmentsFromNonDefaultSkin()
    {
        var data = new SkeletonJson(Atlas()).ReadSkeletonData(new StringReader(Source));
        var frames = JsonSerializer.SerializeToElement(Program.Frames(data, JsonDocument.Parse("{\"setupOnly\":true}").RootElement));
        var clothed = frames.GetProperty("frames").EnumerateArray().Single(f => f.GetProperty("name").GetString() == "clothed/setup");
        Assert.Single(clothed.GetProperty("draws").EnumerateArray());
    }

    [Fact]
    public void GeometryActivatesBonesRequiredByAttachmentSkin()
    {
        string source = Source.Replace("\"rotation\":90", "\"skin\":true,\"rotation\":90")
            .Replace("\"name\":\"clothed\",\"attachments\"", "\"name\":\"clothed\",\"bones\":[\"rotated\"],\"attachments\"");
        var request = JsonDocument.Parse("""
            {"edits":[{"attachment":"clothed/cloth/part","slot":"cloth","deltas":[{"vertex":0,"dx":4,"dy":6}]}]}
            """).RootElement;
        string updated = JsonSerializer.Serialize(Program.Geometry(source, Atlas(), request));
        var before = Pose(source);
        var after = Pose(updated);
        Assert.Equal(4, after[0] - before[0], 4);
        Assert.Equal(6, after[1] - before[1], 4);
    }

    [Theory]
    [InlineData("\"rotation\":90", "\"rotation\":80")]
    [InlineData("\"hull\":3", "\"hull\":2")]
    [InlineData("\"value\":10", "\"value\":20")]
    public void WardrobeRejectsContractAndTopologyChanges(string before, string after)
    {
        Assert.Throws<InvalidDataException>(() => Program.StrictContract(Source, Source.Replace(before, after)));
    }

    [Fact]
    public void VertexOutOfRangeIsRejected()
    {
        var request = JsonDocument.Parse("""
            {"edits":[{"attachment":"clothed/cloth/part","slot":"cloth","deltas":[{"vertex":3,"dx":4,"dy":6}]}]}
            """).RootElement;
        Assert.Throws<InvalidDataException>(() => Program.Geometry(Source, Atlas(), request));
    }

    private static float[] Pose(string json)
    {
        var data = new SkeletonJson(Atlas()).ReadSkeletonData(new StringReader(json));
        var skeleton = new Skeleton(data);
        skeleton.SetSkin(data.FindSkin("clothed"), false);
        skeleton.SetSlotsToSetupPose();
        skeleton.UpdateWorldTransform();
        var mesh = (MeshAttachment)skeleton.FindSlot("cloth").Attachment;
        var vertices = new float[mesh.WorldVerticesLength];
        mesh.ComputeWorldVertices(skeleton.FindSlot("cloth"), 0, vertices.Length, vertices, 0, 2);
        return vertices;
    }
}
