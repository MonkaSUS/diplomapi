using System.ComponentModel.DataAnnotations.Schema;

namespace ThreeMorons.Model;
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Patronymic { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;

    public int UserClassId { get; set; }

    public string? StudNumber { get; set; }

    public byte[] Salt { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DateOfRegistration { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [JsonIgnore]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();


#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public virtual UserClass UserClass { get; set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

    public virtual Student? Student { get; set; }
    [NotMapped]
    public string SearchTerm => String.Join(' ', Name, Surname, Patronymic);



}
