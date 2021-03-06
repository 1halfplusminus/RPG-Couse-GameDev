
using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Burst;


public struct BlendTree1DSetup : IAnimationSetup
{
    public int BlendTreeIndex;
}

public struct BlendTree1DData : IAnimationData
{

    public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeNode;
    public NodeHandle<TimeCounterNode> TimeCounterNode;
    public NodeHandle<TimeLoopNode> TimeLoopNode;
    public NodeHandle<FloatRcpNode> FloatRcpNode;
    public NodeHandle<BlendTree1DNode> BlendTreeNode;
    public NodeHandle<ExtractBlendTree1DParametersNode> BlendTreeInputNode;
    public NodeHandle<ComponentNode> EntityNode;

    public BlobAssetReference<BlendTree1D> BlendTreeAsset;
    public float paramX;
}

public class BlendTree1DGraphSystem : AnimationSystemBase<
BlendTree1DSetup,
BlendTree1DData,
ProcessDefaultAnimationGraph
>
{
    protected override BlendTree1DData CreateGraph(Entity e, ref Rig rig, ProcessDefaultAnimationGraph graphSystem, ref BlendTree1DSetup setup)
    {
        var set = graphSystem.Set;
        var blendTreeComponent = EntityManager.GetBuffer<BlendTree1DResource>(e);
        var blendTreeAsset = BlendTreeBuilder.CreateBlendTree1DFromComponents(blendTreeComponent[setup.BlendTreeIndex], EntityManager, e);
        var data = new BlendTree1DData();

        data.BlendTreeNode = set.Create<BlendTree1DNode>();
        data.BlendTreeAsset = blendTreeAsset;

        data.EntityNode = set.CreateComponentNode(e);
        data.DeltaTimeNode = set.Create<ConvertDeltaTimeToFloatNode>();
        data.TimeCounterNode = set.Create<TimeCounterNode>();
        data.TimeLoopNode = set.Create<TimeLoopNode>();
        data.FloatRcpNode = set.Create<FloatRcpNode>();
        data.BlendTreeInputNode = set.Create<ExtractBlendTree1DParametersNode>();

        set.Connect(data.EntityNode, data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
        set.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.TimeCounterNode, TimeCounterNode.KernelPorts.DeltaTime);
        set.Connect(data.TimeCounterNode, TimeCounterNode.KernelPorts.Time, data.TimeLoopNode, TimeLoopNode.KernelPorts.InputTime);
        set.Connect(data.TimeLoopNode, TimeLoopNode.KernelPorts.OutputTime, data.BlendTreeNode, BlendTree1DNode.KernelPorts.NormalizedTime);

        set.Connect(data.BlendTreeNode, BlendTree1DNode.KernelPorts.Duration, data.FloatRcpNode, FloatRcpNode.KernelPorts.Input);
        set.Connect(data.FloatRcpNode, FloatRcpNode.KernelPorts.Output, data.TimeCounterNode, TimeCounterNode.KernelPorts.Speed);

        set.Connect(data.BlendTreeNode, BlendTree1DNode.KernelPorts.Output, data.EntityNode, NodeSet.ConnectionType.Feedback);
        set.Connect(data.EntityNode, data.BlendTreeInputNode, ExtractBlendTree1DParametersNode.KernelPorts.Input, NodeSet.ConnectionType.Feedback);
        set.Connect(data.BlendTreeInputNode, ExtractBlendTree1DParametersNode.KernelPorts.Output, data.BlendTreeNode, BlendTree1DNode.KernelPorts.BlendParameter);

        set.SendMessage(data.TimeLoopNode, TimeLoopNode.SimulationPorts.Duration, 1.0f);
        set.SendMessage(data.BlendTreeNode, BlendTree1DNode.SimulationPorts.Rig, rig);
        set.SendMessage(data.BlendTreeNode, BlendTree1DNode.SimulationPorts.BlendTree, data.BlendTreeAsset);
        return data;
    }

    protected override void DestroyGraph(Entity e, ProcessDefaultAnimationGraph graphSystem, ref BlendTree1DData data)
    {
        var set = graphSystem.Set;
        set.Destroy(data.DeltaTimeNode);
        set.Destroy(data.TimeCounterNode);
        set.Destroy(data.TimeLoopNode);
        set.Destroy(data.BlendTreeNode);
        set.Destroy(data.BlendTreeInputNode);
        set.Destroy(data.EntityNode);
    }
}
public class ExtractBlendTree1DParametersNode : KernelNodeDefinition<ExtractBlendTree1DParametersNode.KernelDefs>
{
    public struct KernelDefs : IKernelPortDefinition
    {
        public DataInput<ExtractBlendTree1DParametersNode, BlendTree1DData> Input;
        public DataOutput<ExtractBlendTree1DParametersNode, float> Output;
    }

    public struct KernelData : IKernelData { }

    [BurstCompile]
    public struct Kernel : IGraphKernel<KernelData, KernelDefs>
    {
        public void Execute(RenderContext ctx, in KernelData data, ref KernelDefs ports)
        {
            ctx.Resolve(ref ports.Output) = ctx.Resolve(ports.Input).paramX;
        }
    }
}