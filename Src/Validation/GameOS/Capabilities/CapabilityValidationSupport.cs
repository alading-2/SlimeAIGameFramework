using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.SceneTests.GameOS.Capabilities;

internal static class CapabilityValidationSupport
{
    public static void Run(
        Node node,
        string scenePath,
        string logContext,
        string layer,
        string artifactFileName,
        string passMarker,
        string failMarker,
        IReadOnlyList<string> dependencies,
        IReadOnlyList<string> notes,
        IReadOnlyList<string> expectedInputs,
        IReadOnlyList<string> expectedObservations,
        IReadOnlyList<string> passCriteria,
        IReadOnlyList<string> failCriteria,
        Action<SceneValidationSession> registerChecks)
    {
        using var observation = GameOSObservationSession.FromEnvironment(
            scenePath,
            "validation",
            Path.Combine(Directory.GetCurrentDirectory(), ".ai-temp", "scene-tests", "manual", "artifacts"));
        using var validation = new SceneValidationSession(
            observation,
            logContext,
            layer,
            artifactFileName,
            dependencies,
            notes,
            expectedInputs,
            expectedObservations,
            passCriteria,
            failCriteria);

        validation.Info("validation start");
        registerChecks(validation);

        var success = validation.Success;
        if (success)
        {
            validation.Pass("all checks passed");
        }
        else
        {
            validation.Fail($"{validation.FailureReasons.Count} checks failed");
        }

        validation.WriteArtifact();
        GD.Print(success ? passMarker : failMarker);
        if (!success)
        {
            GD.Print($"{logContext} failures: {string.Join("; ", validation.FailureReasons)}");
        }

        node.GetTree().Quit(success ? 0 : 1);
    }

    public static RuntimeEntity Spawn(string id)
    {
        return EntityManager.Spawn(new EntitySpawnConfig { EntityId = new EntityId(id) });
    }

    public static Dictionary<string, object?> Details(params (string Key, object? Value)[] values)
    {
        var details = new Dictionary<string, object?>();
        for (var i = 0; i < values.Length; i++)
        {
            details[values[i].Key] = values[i].Value;
        }

        return details;
    }

    public static bool Approximately(float actual, float expected, float epsilon = 0.001f)
    {
        return MathF.Abs(actual - expected) <= epsilon;
    }
}

internal sealed class FixedAITargetQuery : IAITargetQuery
{
    private readonly IReadOnlyList<IEntity> candidates;

    public FixedAITargetQuery(IReadOnlyList<IEntity> candidates)
    {
        this.candidates = candidates;
    }

    public int Calls { get; private set; }

    public IReadOnlyList<IEntity> GetCandidates(IEntity self)
    {
        Calls++;
        return candidates;
    }
}

internal sealed class FixedAbilityTargetQuery : IAbilityTargetQuery
{
    private readonly IReadOnlyList<IEntity> candidates;

    public FixedAbilityTargetQuery(IReadOnlyList<IEntity> candidates)
    {
        this.candidates = candidates;
    }

    public int Calls { get; private set; }

    public IReadOnlyList<IEntity> GetCandidates(IEntity caster)
    {
        Calls++;
        return candidates;
    }
}

internal sealed class FixedMovementCollisionTargetQuery : IMovementCollisionTargetQuery
{
    private readonly IReadOnlyList<IEntity> candidates;

    public FixedMovementCollisionTargetQuery(IReadOnlyList<IEntity> candidates)
    {
        this.candidates = candidates;
    }

    public int Calls { get; private set; }

    public IReadOnlyList<IEntity> GetCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended)
    {
        Calls++;
        return candidates;
    }
}

internal sealed class CountingFeatureAction : IFeatureAction
{
    public int Count { get; private set; }

    public void Execute(FeatureContext context)
    {
        Count++;
    }
}

internal sealed class ProbeFeatureResult : IFeatureExecutionResult
{
    public ProbeFeatureResult(int value)
    {
        Value = value;
    }

    public int Value { get; }
}

internal sealed class ProbeFeatureHandler : IFeatureHandler
{
    public ProbeFeatureHandler(string featureId)
    {
        FeatureId = featureId;
    }

    public string FeatureId { get; }

    public int Granted { get; private set; }
    public int Removed { get; private set; }
    public int Enabled { get; private set; }
    public int Disabled { get; private set; }
    public int Activated { get; private set; }
    public int Executed { get; private set; }
    public int Ended { get; private set; }
    public bool SawTypedPayload { get; private set; }

    public void OnGranted(FeatureContext context) => Granted++;

    public void OnRemoved(FeatureContext context) => Removed++;

    public void OnEnabled(FeatureContext context) => Enabled++;

    public void OnDisabled(FeatureContext context) => Disabled++;

    public void OnActivated(FeatureContext context)
    {
        Activated++;
        SawTypedPayload |= context.ActivationPayload != null || context.SourceEventPayload != null;
    }

    public IFeatureExecutionResult OnExecute(FeatureContext context)
    {
        Executed++;
        return new ProbeFeatureResult(Executed);
    }

    public void OnEnded(FeatureContext context, FeatureEndReason reason) => Ended++;
}
