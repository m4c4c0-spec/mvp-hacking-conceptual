using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    public sealed class ConceptCard
    {
        public ConceptId Id { get; }
        public string Name { get; }
        public string Category { get; }
        public string Definition { get; }
        public string Importance { get; }
        public string Defense { get; }

        public ConceptCard(ConceptId id, string name, string category, string definition, string importance, string defense)
        {
            Id = id;
            Name = name ?? "";
            Category = category ?? "";
            Definition = definition ?? "";
            Importance = importance ?? "";
            Defense = defense ?? "";
        }
    }
}
