using System;
using System.Collections.Generic;

namespace GameStore.Database.Models;

public partial class Game
{
    public int GameId { get; set; }

    public string GameName { get; set; } = null!;

    public string Genre { get; set; } = null!;

    public string? Description { get; set; }

    public int? Price { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<UserLibrary> UserLibraries { get; set; } = new List<UserLibrary>();
}
