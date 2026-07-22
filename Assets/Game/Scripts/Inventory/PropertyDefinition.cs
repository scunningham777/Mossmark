namespace Mossmark.Inventory
{
    public class PropertyDefinition
    {
        public string Id { get; }
        public string Phrase { get; }

        // Most phrases are verb phrases ("binds fast", "burns slow") that read fine
        // dropped straight after "what"/"it"/"that". A few ("heavy and true") are
        // adjectival and need a linking "is" to stay grammatical in the same slot.
        public bool IsAdjectival { get; }

        public PropertyDefinition(string id, string phrase, bool isAdjectival = false)
        {
            Id = id;
            Phrase = phrase;
            IsAdjectival = isAdjectival;
        }

        // The phrase as a relative-clause predicate — what every call site that
        // splices Phrase after "what"/"it"/"that" should use instead of Phrase
        // directly, so adjectival properties don't need their own special-casing
        // at each call site.
        public string Clause => IsAdjectival ? $"is {Phrase}" : Phrase;
    }
}
