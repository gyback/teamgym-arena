using TeamGym.Arena.Common;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>A section that a rule set offers, and the gender composition it requires of a team.</summary>
internal sealed record SectionRule(Section Section, GenderComposition GenderComposition);
