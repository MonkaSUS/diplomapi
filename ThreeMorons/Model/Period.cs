using System;
using System.Collections.Generic;

namespace ThreeMorons.Model;

/// <summary>
/// РАСПИСАНИЕ ЗВОНКОВ, БЕЗ УЧЁТА ВОЗМОЖНЫХ СОКРАЩЕНИЙ ПАР ИТД
/// </summary>
public partial class Period
{
    public int Id { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
