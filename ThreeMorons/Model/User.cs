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

    [JsonIgnore]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual UserClass UserClass { get; set; } = null!;
    public byte[] Salt { get; set; } = null!;

}
