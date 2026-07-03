namespace Mossmark.Inventory
{
    public class PropertyDefinition
    {
        public string Id { get; }
        public string Phrase { get; }

        public PropertyDefinition(string id, string phrase)
        {
            Id = id;
            Phrase = phrase;
        }
    }
}
