using AutoGen.Core;

namespace Senparc.AI.Agents.AgentExtensions;

public static class GraphBuilder
{
    public static Graph Start(IEnumerable<Transition>? transitions = null)
    {
        return new Graph(transitions);
    }
}
