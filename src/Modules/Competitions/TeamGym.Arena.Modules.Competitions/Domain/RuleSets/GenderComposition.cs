namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// Which competitive genders a section allows in a team (ADR-0006). A rule set chooses one per section, so the
/// <b>Mixed</b> ratio and the open composition of <b>Open</b> are configuration, not constants.
/// </summary>
internal enum GenderComposition
{
    /// <summary>Every gymnast competes as female, as in <b>Women</b>.</summary>
    FemaleOnly,

    /// <summary>Every gymnast competes as male, as in <b>Men</b>.</summary>
    MaleOnly,

    /// <summary>As many gymnasts compete as female as compete as male, as in <b>Mixed</b>.</summary>
    EqualFemaleAndMale,

    /// <summary>Any composition, as in <b>Open</b>.</summary>
    Any,
}
