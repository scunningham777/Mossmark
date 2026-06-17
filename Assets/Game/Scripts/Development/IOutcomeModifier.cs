namespace Mossmark.Development
{
    // One implementation per influence on an outcome - parallel to IDependencyCondition
    // but for graduated biasing rather than binary gating. Dependencies answer "can this
    // proceed at all"; modifiers answer "how strongly" once it can.
    public interface IOutcomeModifier
    {
        void Apply(OutcomeRequest request);
    }
}
