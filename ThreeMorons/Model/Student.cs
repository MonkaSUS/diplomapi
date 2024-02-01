using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ThreeMorons.Model;

public partial class Student
{
    public string StudNumber { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual Group GroupNameNavigation { get; set; } = null!;

    public bool IsDeleted { get; set; }
    [JsonIgnore]
    public virtual ICollection<SkippedClass> SkippedClasses { get; set; } = new List<SkippedClass>();
    [JsonIgnore]
    public virtual ICollection<StudentDelay> StudentDelays { get; set; } = new List<StudentDelay>();
    [NotMapped]
    public string SerachTerm => String.Join(" ", Name, Surname, Patronymic, StudNumber);
}

