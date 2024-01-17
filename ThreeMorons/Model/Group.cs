using System;
using System.Collections.Generic;


namespace ThreeMorons.Model;

public partial class Group
{
    public string GroupName { get; set; } = null!;

    public Guid GroupCurator { get; set; }

    public int? Building { get; set; }

    public virtual User GroupCuratorNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
