namespace Mossmark.Development
{
    public interface IDependencyCondition
    {
        bool IsSatisfied(DevelopableEntity entity);
        string GetNeedsDescription(DevelopableEntity entity);
    }
}
