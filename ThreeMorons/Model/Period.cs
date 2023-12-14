using System;
using System.Collections.Generic;

namespace ThreeMorons.Model;

public partial class Period
{
    public int Id { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
