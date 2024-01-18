using System;
using System.Collections.Generic;

namespace ThreeMorons.Model;

/// <summary>
/// ПРОПУСК
/// </summary>
public partial class SkippedClass
{
    public Guid Id { get; set; }

    public string StudNumber { get; set; } = null!;


    public DateOnly DateOfSkip { get; set; }

    public virtual Student StudNumberNavigation { get; set; } = null!;
}

;