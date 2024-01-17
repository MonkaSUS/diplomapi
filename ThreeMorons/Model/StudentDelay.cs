using System;
using System.Collections.Generic;

namespace ThreeMorons.Model;

/// <summary>
/// ОПОЗДАНИЕ
/// </summary>
public partial class StudentDelay
{
    public Guid Id { get; set; }

    public string StudNumber { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public TimeOnly Delay { get; set; }

    public virtual Student StudNumberNavigation { get; set; } = null!;
}
