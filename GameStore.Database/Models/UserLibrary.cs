using System;
using System.Collections.Generic;

namespace GameStore.Database.Models;

public partial class UserLibrary
{
    public int UserLibraryId { get; set; }

    public int UserId { get; set; }

    public int GameId { get; set; }

    public DateTime PurchaseDate { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
