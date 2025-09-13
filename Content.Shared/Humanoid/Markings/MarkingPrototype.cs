using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array; // AuroraSong
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings
{
    [Prototype]
    // AuroraSong: Make markings inheriting (IInheritingPrototype)
    public sealed partial class MarkingPrototype : IPrototype, IInheritingPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = "uwu";

        // AuroraSong: Make markings inheriting
        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<MarkingPrototype>))]
        public string[]? Parents { get; }

        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; }
        // End AuroraSong

        public string Name { get; private set; } = default!;

        [DataField("bodyPart", required: true)]
        public HumanoidVisualLayers BodyPart { get; private set; }

        [DataField("markingCategory", required: true)]
        public MarkingCategories MarkingCategory { get; private set; }

        [DataField("speciesRestriction")]
        public List<string>? SpeciesRestrictions { get; private set; }

        // DEN - Invert marking restrictions
        [DataField]
        public bool InvertSpeciesRestriction { get; private set; }

        [DataField]
        public Sex? SexRestriction { get; private set; }

        // DEN - Invert marking restrictions
        [DataField]
        public bool InvertSexRestriction { get; private set; }

        [DataField]
        public bool FollowSkinColor { get; private set; }

        [DataField]
        public bool ForcedColoring { get; private set; }

        [DataField]
        public MarkingColors Coloring { get; private set; } = new();

        /// <summary>
        /// Do we need to apply any displacement maps to this marking? Set to false if your marking is incompatible
        /// with a standard human doll, and is used for some special races with unusual shapes
        /// </summary>
        [DataField]
        public bool CanBeDisplaced { get; private set; } = true;

        [DataField("sprites", required: true)]
        public List<SpriteSpecifier> Sprites { get; private set; } = default!;

        // impstation edit - allow markings to support shaders
        [DataField("shader")]
        public string? Shader { get; private set; } = null;
        // end impstation edit
        public Marking AsMarking()
        {
            return new Marking(ID, Sprites.Count);
        }
    }
}
