namespace ThreeMorons.Model;
public partial class UserClass
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
